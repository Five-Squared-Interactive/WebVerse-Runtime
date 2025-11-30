// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.X3D.Abstraction;

namespace X3D
{
    /// <summary>
    /// Builds a world from X3D data using the IX3DWorldAdapter interface.
    /// This class bridges X3DParser/X3DUnityConverter output to any world engine.
    /// </summary>
    public class X3DWorldBuilder
    {
        private readonly IX3DWorldAdapter adapter;
        private readonly Dictionary<string, X3DEntityHandle> defToEntity = new Dictionary<string, X3DEntityHandle>();
        private readonly Dictionary<string, GameObject> defToGameObject = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, X3DNode> defToNode = new Dictionary<string, X3DNode>();

        /// <summary>
        /// Initialize the world builder with a world adapter.
        /// </summary>
        /// <param name="adapter">The world engine adapter to use.</param>
        public X3DWorldBuilder(IX3DWorldAdapter adapter)
        {
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        /// <summary>
        /// Build a world from an X3D document.
        /// </summary>
        /// <param name="x3dContent">The X3D document content (XML string).</param>
        /// <param name="onComplete">Callback when world building is complete.</param>
        public void BuildWorld(string x3dContent, Action onComplete = null)
        {
            try
            {
                // Parse X3D content to node tree
                X3DNode rootNode = X3DParser.Parse(x3dContent);
                
                if (rootNode == null)
                {
                    Debug.LogError("[X3DWorldBuilder] Failed to parse X3D content");
                    return;
                }

                // Process the node tree
                ProcessNode(rootNode, null);

                // Process ROUTE nodes for animation wiring
                ProcessRoutes(rootNode);

                onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[X3DWorldBuilder] Error building world: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Build a world from an already-parsed X3D node tree.
        /// </summary>
        /// <param name="rootNode">The root X3D node.</param>
        /// <param name="onComplete">Callback when world building is complete.</param>
        public void BuildFromNodes(X3DNode rootNode, Action onComplete = null)
        {
            try
            {
                if (rootNode == null)
                {
                    Debug.LogError("[X3DWorldBuilder] Root node is null");
                    return;
                }

                ProcessNode(rootNode, null);
                ProcessRoutes(rootNode);

                onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[X3DWorldBuilder] Error building world: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Process an X3D node and its children.
        /// </summary>
        private void ProcessNode(X3DNode node, X3DEntityHandle parent)
        {
            if (node == null) return;

            // Handle USE references - clone the original DEF'd node at this location
            if (node.Attributes.TryGetValue("USE", out string useRef))
            {
                if (defToNode.TryGetValue(useRef, out X3DNode originalNode))
                {
                    // Process the original node at the current parent location
                    // This effectively clones/instantiates the DEF'd node
                    ProcessNode(originalNode, parent);
                }
                else
                {
                    Debug.LogWarning($"[X3DWorldBuilder] USE reference '{useRef}' not found. Make sure DEF is defined before USE.");
                }
                return;
            }

            X3DEntityHandle entityHandle = null;
            string defName = null;
            node.Attributes.TryGetValue("DEF", out defName);

            switch (node.Name)
            {
                case "X3D":
                case "Scene":
                case "head":
                case "component":
                case "meta":
                case "unit":
                    // Container nodes - process children only
                    foreach (var child in node.Children)
                    {
                        ProcessNode(child, parent);
                    }
                    break;

                case "Transform":
                    entityHandle = ProcessTransform(node, parent);
                    break;

                case "Group":
                case "StaticGroup":
                case "Anchor":
                    entityHandle = ProcessGroup(node, parent);
                    break;

                case "Shape":
                    entityHandle = ProcessShape(node, parent);
                    break;

                case "DirectionalLight":
                    entityHandle = ProcessDirectionalLight(node, parent);
                    break;

                case "PointLight":
                    entityHandle = ProcessPointLight(node, parent);
                    break;

                case "SpotLight":
                    entityHandle = ProcessSpotLight(node, parent);
                    break;

                case "Background":
                    ProcessBackground(node);
                    break;

                case "Fog":
                    ProcessFog(node);
                    break;

                case "Viewpoint":
                    ProcessViewpoint(node, parent);
                    break;

                case "GeoViewpoint":
                    ProcessGeoViewpoint(node, parent);
                    break;

                case "NavigationInfo":
                    ProcessNavigationInfo(node);
                    break;

                case "WorldInfo":
                    // Metadata only, no world creation needed
                    break;

                case "Inline":
                    // TODO: Load external X3D file
                    Debug.LogWarning($"[X3DWorldBuilder] Inline node not yet implemented");
                    break;

                case "TimeSensor":
                case "PositionInterpolator":
                case "OrientationInterpolator":
                case "ColorInterpolator":
                case "ScalarInterpolator":
                case "ROUTE":
                    // Animation nodes - handled separately
                    break;

                case "Switch":
                case "LOD":
                case "Billboard":
                    // Advanced grouping nodes
                    entityHandle = ProcessGroup(node, parent);
                    break;

                default:
                    // Unknown node type - try to convert via X3DUnityConverter and process result
                    Debug.LogWarning($"[X3DWorldBuilder] Unknown node type: {node.Name}");
                    break;
            }

            // Store DEF reference and original node for USE cloning
            if (!string.IsNullOrEmpty(defName))
            {
                if (entityHandle != null)
                {
                    defToEntity[defName] = entityHandle;
                }
                defToNode[defName] = node;
            }
        }

        #region Node Processors

        private X3DEntityHandle ProcessTransform(X3DNode node, X3DEntityHandle parent)
        {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;
            string tag = null;

            node.Attributes.TryGetValue("DEF", out tag);

            if (node.Attributes.TryGetValue("translation", out string transStr))
                position = ParseVector3(transStr);

            if (node.Attributes.TryGetValue("rotation", out string rotStr))
                rotation = ParseRotation(rotStr);

            if (node.Attributes.TryGetValue("scale", out string scaleStr))
                scale = ParseVector3(scaleStr);

            var handle = adapter.CreateContainerEntity(parent, position, rotation, scale, tag);

            // Process children
            foreach (var child in node.Children)
            {
                ProcessNode(child, handle);
            }

            return handle;
        }

        private X3DEntityHandle ProcessGroup(X3DNode node, X3DEntityHandle parent)
        {
            string tag = null;
            node.Attributes.TryGetValue("DEF", out tag);

            var handle = adapter.CreateContainerEntity(parent, Vector3.zero, Quaternion.identity, Vector3.one, tag);

            // Process children
            foreach (var child in node.Children)
            {
                ProcessNode(child, handle);
            }

            return handle;
        }

        private X3DEntityHandle ProcessShape(X3DNode node, X3DEntityHandle parent)
        {
            string tag = null;
            node.Attributes.TryGetValue("DEF", out tag);

            // First, use X3DUnityConverter to get the geometry data
            GameObject tempGO = X3DUnityConverter.X3DNodeToGameObject(node);
            
            if (tempGO == null)
            {
                Debug.LogWarning("[X3DWorldBuilder] Failed to convert Shape node to GameObject");
                return null;
            }

            // Extract mesh, material, and scale from the temporary GameObject
            X3DShape shapeComponent = tempGO.GetComponent<X3DShape>();
            Mesh mesh = null;
            Material material = null;
            Vector3 geometryScale = Vector3.one;

            if (shapeComponent != null)
            {
                // Get geometry from the shape's geometry child
                if (shapeComponent.geometry != null)
                {
                    MeshFilter mf = shapeComponent.geometry.GetComponent<MeshFilter>();
                    if (mf != null) mesh = mf.sharedMesh;

                    MeshRenderer mr = shapeComponent.geometry.GetComponent<MeshRenderer>();
                    if (mr != null) material = mr.sharedMaterial;

                    // Capture the geometry's scale (from Box size, Sphere radius, etc.)
                    geometryScale = shapeComponent.geometry.transform.localScale;
                }

                // Get material from appearance
                if (shapeComponent.appearance != null)
                {
                    var appearanceComp = shapeComponent.appearance.GetComponent<X3DAppearance>();
                    if (appearanceComp != null)
                    {
                        // Check for X3DMaterial component on the material child object
                        if (appearanceComp.material != null)
                        {
                            var matComp = appearanceComp.material.GetComponent<X3DMaterialComponent>();
                            if (matComp != null)
                            {
                                material = CreateMaterialFromX3D(matComp);
                            }
                            else
                            {
                                Debug.LogWarning("[X3DWorldBuilder] appearanceComp.material exists but has no X3DMaterialComponent");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[X3DWorldBuilder] appearanceComp has no material child");
                        }

                        // TODO: Handle textures from appearanceComp.texture
                    }
                    else
                    {
                        Debug.LogWarning("[X3DWorldBuilder] shapeComponent.appearance has no X3DAppearance component");
                    }

                    // Fallback: try getting material directly from appearance renderer
                    if (material == null)
                    {
                        MeshRenderer mr = shapeComponent.appearance.GetComponent<MeshRenderer>();
                        if (mr != null) material = mr.sharedMaterial;
                    }
                }
                else
                {
                    Debug.Log("[X3DWorldBuilder] Shape has no appearance, using default material");
                }
            }
            else
            {
                // Direct mesh on the shape
                MeshFilter mf = tempGO.GetComponent<MeshFilter>();
                if (mf != null) mesh = mf.sharedMesh;

                MeshRenderer mr = tempGO.GetComponent<MeshRenderer>();
                if (mr != null) material = mr.sharedMaterial;

                // Capture scale from the temp object itself
                geometryScale = tempGO.transform.localScale;
            }

            // Default material if none found
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
            }

            X3DEntityHandle handle = null;

            if (mesh != null)
            {
                handle = adapter.CreateMeshEntity(parent, mesh, material, Vector3.zero, Quaternion.identity, geometryScale, tag);
            }
            else
            {
                // If no mesh was extracted, just create a container
                handle = adapter.CreateContainerEntity(parent, Vector3.zero, Quaternion.identity, Vector3.one, tag);
            }

            // Clean up temporary GameObject
            UnityEngine.Object.DestroyImmediate(tempGO);

            return handle;
        }

        private X3DEntityHandle ProcessDirectionalLight(X3DNode node, X3DEntityHandle parent)
        {
            string tag = null;
            node.Attributes.TryGetValue("DEF", out tag);

            Color color = Color.white;
            float intensity = 1f;
            Vector3 direction = new Vector3(0, -1, 0);

            if (node.Attributes.TryGetValue("color", out string colorStr))
                color = ParseColor(colorStr);

            if (node.Attributes.TryGetValue("intensity", out string intensityStr) && float.TryParse(intensityStr, out float i))
                intensity = i;

            if (node.Attributes.TryGetValue("direction", out string dirStr))
                direction = ParseVector3(dirStr);

            // Convert direction to rotation
            Quaternion rotation = Quaternion.LookRotation(direction);

            return adapter.CreateLightEntity(parent, X3DLightType.Directional, color, intensity, Vector3.zero, rotation, 0, 0, tag);
        }

        private X3DEntityHandle ProcessPointLight(X3DNode node, X3DEntityHandle parent)
        {
            string tag = null;
            node.Attributes.TryGetValue("DEF", out tag);

            Color color = Color.white;
            float intensity = 1f;
            Vector3 location = Vector3.zero;
            float radius = 10f;

            if (node.Attributes.TryGetValue("color", out string colorStr))
                color = ParseColor(colorStr);

            if (node.Attributes.TryGetValue("intensity", out string intensityStr) && float.TryParse(intensityStr, out float i))
                intensity = i;

            if (node.Attributes.TryGetValue("location", out string locStr))
                location = ParseVector3(locStr);

            if (node.Attributes.TryGetValue("radius", out string radiusStr) && float.TryParse(radiusStr, out float r))
                radius = r;

            return adapter.CreateLightEntity(parent, X3DLightType.Point, color, intensity, location, Quaternion.identity, radius, 0, tag);
        }

        private X3DEntityHandle ProcessSpotLight(X3DNode node, X3DEntityHandle parent)
        {
            string tag = null;
            node.Attributes.TryGetValue("DEF", out tag);

            Color color = Color.white;
            float intensity = 1f;
            Vector3 location = Vector3.zero;
            Vector3 direction = new Vector3(0, -1, 0);
            float radius = 10f;
            float cutOffAngle = 0.78539816339f; // PI/4

            if (node.Attributes.TryGetValue("color", out string colorStr))
                color = ParseColor(colorStr);

            if (node.Attributes.TryGetValue("intensity", out string intensityStr) && float.TryParse(intensityStr, out float i))
                intensity = i;

            if (node.Attributes.TryGetValue("location", out string locStr))
                location = ParseVector3(locStr);

            if (node.Attributes.TryGetValue("direction", out string dirStr))
                direction = ParseVector3(dirStr);

            if (node.Attributes.TryGetValue("radius", out string radiusStr) && float.TryParse(radiusStr, out float r))
                radius = r;

            if (node.Attributes.TryGetValue("cutOffAngle", out string cutOffStr) && float.TryParse(cutOffStr, out float angle))
                cutOffAngle = angle;

            Quaternion rotation = Quaternion.LookRotation(direction);
            float spotAngleDegrees = cutOffAngle * Mathf.Rad2Deg * 2; // X3D uses half-angle

            return adapter.CreateLightEntity(parent, X3DLightType.Spot, color, intensity, location, rotation, radius, spotAngleDegrees, tag);
        }

        private void ProcessBackground(X3DNode node)
        {
            var settings = new X3DBackgroundSettings();

            // Parse sky colors - X3D skyColor is a list of RGB triplets
            // First color is at zenith, subsequent colors are at skyAngle[i] from zenith
            if (node.Attributes.TryGetValue("skyColor", out string skyColorStr))
            {
                settings.SkyColors = ParseColorArray(skyColorStr);
            }
            else
            {
                settings.SkyColors = new Color[] { Color.black };
            }

            // Parse sky angles - angles in radians from zenith for colors 1..n
            if (node.Attributes.TryGetValue("skyAngle", out string skyAngleStr))
            {
                settings.SkyAngles = ParseFloatArray(skyAngleStr);
            }

            // Parse ground colors - X3D groundColor is a list of RGB triplets
            // First color is at nadir, subsequent colors are at groundAngle[i] from nadir
            if (node.Attributes.TryGetValue("groundColor", out string groundColorStr))
            {
                settings.GroundColors = ParseColorArray(groundColorStr);
            }
            else
            {
                settings.GroundColors = new Color[] { Color.black };
            }

            // Parse ground angles - angles in radians from nadir for colors 1..n
            if (node.Attributes.TryGetValue("groundAngle", out string groundAngleStr))
            {
                settings.GroundAngles = ParseFloatArray(groundAngleStr);
            }

            // Parse skybox texture URLs
            if (node.Attributes.TryGetValue("frontUrl", out string frontUrl))
                settings.FrontUrl = ParseStringArray(frontUrl);
            if (node.Attributes.TryGetValue("backUrl", out string backUrl))
                settings.BackUrl = ParseStringArray(backUrl);
            if (node.Attributes.TryGetValue("leftUrl", out string leftUrl))
                settings.LeftUrl = ParseStringArray(leftUrl);
            if (node.Attributes.TryGetValue("rightUrl", out string rightUrl))
                settings.RightUrl = ParseStringArray(rightUrl);
            if (node.Attributes.TryGetValue("topUrl", out string topUrl))
                settings.TopUrl = ParseStringArray(topUrl);
            if (node.Attributes.TryGetValue("bottomUrl", out string bottomUrl))
                settings.BottomUrl = ParseStringArray(bottomUrl);

            adapter.SetBackground(settings);
        }

        private void ProcessFog(X3DNode node)
        {
            var settings = new X3DFogSettings();
            settings.Enabled = true;
            settings.Color = Color.white;
            settings.VisibilityRange = 1000f;
            settings.FogType = "LINEAR";

            if (node.Attributes.TryGetValue("color", out string colorStr))
                settings.Color = ParseColor(colorStr);

            if (node.Attributes.TryGetValue("visibilityRange", out string rangeStr) && float.TryParse(rangeStr, out float range))
                settings.VisibilityRange = range;

            if (node.Attributes.TryGetValue("fogType", out string fogType))
                settings.FogType = fogType;

            adapter.SetFog(settings);
        }

        private void ProcessViewpoint(X3DNode node, X3DEntityHandle parent)
        {
            Vector3 position = Vector3.zero;
            Quaternion orientation = Quaternion.identity;
            float fieldOfView = 0.7854f; // PI/4 radians = 45 degrees
            string description = null;

            if (node.Attributes.TryGetValue("position", out string posStr))
                position = ParseVector3(posStr);

            if (node.Attributes.TryGetValue("orientation", out string oriStr))
                orientation = ParseRotation(oriStr);

            if (node.Attributes.TryGetValue("fieldOfView", out string fovStr) && float.TryParse(fovStr, out float fov))
                fieldOfView = fov;

            node.Attributes.TryGetValue("description", out description);

            adapter.SetViewpoint(position, orientation, fieldOfView, description);
        }

        private void ProcessGeoViewpoint(X3DNode node, X3DEntityHandle parent)
        {
            // TODO: Implement geo coordinate conversion
            // For now, just use standard viewpoint processing
            ProcessViewpoint(node, parent);
        }

        private void ProcessNavigationInfo(X3DNode node)
        {
            float speed = 1f;
            string[] type = new string[] { "EXAMINE" };

            if (node.Attributes.TryGetValue("speed", out string speedStr) && float.TryParse(speedStr, out float s))
                speed = s;

            if (node.Attributes.TryGetValue("type", out string typeStr))
                type = ParseStringArray(typeStr);

            // Apply navigation settings through adapter
            // adapter.SetNavigationInfo(speed, type); // TODO: Add to interface if needed
        }

        private void ProcessRoutes(X3DNode rootNode)
        {
            // Collect all ROUTE nodes from the tree
            var routes = new List<X3DNode>();
            CollectRoutes(rootNode, routes);

            foreach (var route in routes)
            {
                // TODO: Wire up animation routes
                string fromNode = null;
                string fromField = null;
                string toNode = null;
                string toField = null;

                route.Attributes.TryGetValue("fromNode", out fromNode);
                route.Attributes.TryGetValue("fromField", out fromField);
                route.Attributes.TryGetValue("toNode", out toNode);
                route.Attributes.TryGetValue("toField", out toField);

                if (!string.IsNullOrEmpty(fromNode) && !string.IsNullOrEmpty(toNode))
                {
                    Debug.Log($"[X3DWorldBuilder] ROUTE: {fromNode}.{fromField} -> {toNode}.{toField}");
                    // TODO: Create animation connections
                }
            }
        }

        private void CollectRoutes(X3DNode node, List<X3DNode> routes)
        {
            if (node.Name == "ROUTE")
            {
                routes.Add(node);
            }

            foreach (var child in node.Children)
            {
                CollectRoutes(child, routes);
            }
        }

        #endregion

        #region Helper Methods

        private Material CreateMaterialFromX3D(X3DMaterialComponent matComp)
        {
            // Try URP shader first, fall back to Standard
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            
            var material = new Material(shader);
            
            // Set base color (diffuse)
            if (matComp.diffuseColor != Color.clear)
            {
                material.color = matComp.diffuseColor;
                // URP uses _BaseColor
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", matComp.diffuseColor);
            }
            
            // Set emission
            if (matComp.emissiveColor != Color.clear && 
                (matComp.emissiveColor.r > 0.001f || matComp.emissiveColor.g > 0.001f || matComp.emissiveColor.b > 0.001f))
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                    material.SetColor("_EmissionColor", matComp.emissiveColor);
            }
            
            // Set smoothness/shininess
            if (matComp.shininess > 0)
            {
                if (material.HasProperty("_Smoothness"))
                    material.SetFloat("_Smoothness", matComp.shininess);
                if (material.HasProperty("_Glossiness"))
                    material.SetFloat("_Glossiness", matComp.shininess);
            }
            
            // Handle transparency
            if (matComp.transparency > 0.001f)
            {
                Color c = matComp.diffuseColor != Color.clear ? matComp.diffuseColor : Color.white;
                c.a = 1f - matComp.transparency;
                material.color = c;
                
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", c);
                
                // URP transparency settings
                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat("_Surface", 1); // Transparent
                    material.SetFloat("_Blend", 0); // Alpha blend
                    material.SetFloat("_AlphaClip", 0);
                }
                else
                {
                    // Standard shader transparency
                    material.SetFloat("_Mode", 3); // Transparent mode
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                }
                
                material.renderQueue = 3000;
            }

            return material;
        }

        private static Vector3 ParseVector3(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            return Vector3.zero;
        }

        private static Quaternion ParseRotation(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z) &&
                float.TryParse(parts[3], out float angle))
            {
                return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, new Vector3(x, y, z));
            }
            return Quaternion.identity;
        }

        private static Color ParseColor(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], out float r) &&
                float.TryParse(parts[1], out float g) &&
                float.TryParse(parts[2], out float b))
            {
                return new Color(r, g, b);
            }
            return Color.white;
        }

        private static Color[] ParseColorArray(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int count = parts.Length / 3;
            Color[] colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                float.TryParse(parts[i * 3], out float r);
                float.TryParse(parts[i * 3 + 1], out float g);
                float.TryParse(parts[i * 3 + 2], out float b);
                colors[i] = new Color(r, g, b);
            }
            return colors;
        }

        private static float[] ParseFloatArray(string str)
        {
            var parts = str.Split(new char[] { ' ', '\t', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<float>();
            foreach (var part in parts)
            {
                if (float.TryParse(part, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float val))
                {
                    result.Add(val);
                }
            }
            return result.ToArray();
        }

        private static string[] ParseStringArray(string str)
        {
            // X3D string arrays can be quoted or space-separated
            return str.Split(new char[] { ' ', '\t', '\n', '\r', '"', '\'' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
