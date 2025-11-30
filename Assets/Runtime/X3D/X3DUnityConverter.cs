using UnityEngine;
using System;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.Video;
using X3D;

namespace X3D
{
    public static class X3DUnityConverter
    {
        // Converts an X3DNode to a Unity GameObject (scaffold)
        public static GameObject X3DNodeToGameObject(X3DNode node)
        {
            GameObject go = null;
            // UI nodes should be parented to a Canvas
            bool isUINode = node.Name == "Text" || node.Name == "Button" || node.Name == "Slider" || node.Name == "Panel" || node.Name == "Label" || node.Name == "Checkbox" || node.Name == "TextField" || node.Name == "Group" || node.Name == "Layout";
            Canvas parentCanvas = null;
            if (isUINode)
            {
                parentCanvas = EnsureCanvas();
            }
            switch (node.Name)
            {
                case "Coordinate":
                    go = new GameObject("Coordinate");
                    var coord = go.AddComponent<X3DCoordinate>();
                    if (node.Attributes.TryGetValue("point", out string pointStr))
                        coord.points = ParseVector3Array(pointStr);
                    if (node.Attributes.TryGetValue("DEF", out string defCoord)) coord.DEF = defCoord;
                    if (node.Attributes.TryGetValue("USE", out string useCoord)) coord.USE = useCoord;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldCoord)) coord.containerField = containerFieldCoord;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            coord.metadata = metaStr;
                    }
                    break;
                case "TextField":
                    go = new GameObject("TextField");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
#if TMP_PRESENT
                    go.AddComponent<TMPro.TMP_InputField>();
#else
                    go.AddComponent<UnityEngine.UI.InputField>();
#endif
                    break;
                case "Group":
                    go = new GameObject("Group");
                    var groupStub = go.AddComponent<X3DGroup>();
                    if (node.Attributes.TryGetValue("bboxCenter", out string bboxCenter))
                        groupStub.bboxCenter = ParseVector3(bboxCenter);
                    if (node.Attributes.TryGetValue("bboxSize", out string bboxSize))
                        groupStub.bboxSize = ParseVector3(bboxSize);
                    if (node.Attributes.TryGetValue("DEF", out string defG)) groupStub.DEF = defG;
                    if (node.Attributes.TryGetValue("USE", out string useG)) groupStub.USE = useG;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldG)) groupStub.containerField = containerFieldG;
                    foreach (var child in node.Children)
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            groupStub.metadata = metaStr;
                    break;
                case "Layout":
                    go = new GameObject("Layout");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
                    go.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                    break;
                case "Text":
                    go = new GameObject("Text");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
#if TMP_PRESENT
                    var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
                    tmp.text = node.InnerText ?? "";
#else
                    var uiText = go.AddComponent<UnityEngine.UI.Text>();
                    uiText.text = node.InnerText ?? "";
#endif
                    break;
                case "Button":
                    go = new GameObject("Button");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
                    var button = go.AddComponent<UnityEngine.UI.Button>();
                    // Add a child Text for the button label
                    var btnTextGO = new GameObject("Text");
                    btnTextGO.AddComponent<RectTransform>();
                    btnTextGO.transform.SetParent(go.transform, false);
#if TMP_PRESENT
                    var btnTmp = btnTextGO.AddComponent<TMPro.TextMeshProUGUI>();
                    btnTmp.text = node.InnerText ?? "Button";
#else
                    var btnText = btnTextGO.AddComponent<UnityEngine.UI.Text>();
                    btnText.text = node.InnerText ?? "Button";
#endif
                    break;
                case "Slider":
                    go = new GameObject("Slider");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
                    go.AddComponent<UnityEngine.UI.Slider>();
                    break;
                case "Panel":
                    go = new GameObject("Panel");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
                    go.AddComponent<UnityEngine.UI.Image>();
                    break;
                case "Label":
                    go = new GameObject("Label");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
#if TMP_PRESENT
                    var labelTmp = go.AddComponent<TMPro.TextMeshProUGUI>();
                    labelTmp.text = node.InnerText ?? "Label";
#else
                    var labelText = go.AddComponent<UnityEngine.UI.Text>();
                    labelText.text = node.InnerText ?? "Label";
#endif
                    break;
                case "Checkbox":
                    go = new GameObject("Checkbox");
                    go.AddComponent<RectTransform>();
                    if (parentCanvas != null) go.transform.SetParent(parentCanvas.transform, false);
                    go.AddComponent<UnityEngine.UI.Toggle>();
                    break;
                // Event handling stubs for interactivity
                case "TouchSensor":
                    go = new GameObject("TouchSensor");
                    // Add a collider and event stub
                    var collider = go.AddComponent<BoxCollider>();
                    var touchSensor = go.AddComponent<X3DTouchSensor>();
                    if (node.Attributes.TryGetValue("enabled", out string enabledStr))
                        touchSensor.x3dEnabled = enabledStr == "true";
                    if (node.Attributes.TryGetValue("description", out string descStr))
                        touchSensor.description = descStr;
                    if (node.Attributes.TryGetValue("DEF", out string defTouchSensor)) touchSensor.DEF = defTouchSensor;
                    if (node.Attributes.TryGetValue("USE", out string useTouchSensor)) touchSensor.USE = useTouchSensor;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldTouchSensor)) touchSensor.containerField = containerFieldTouchSensor;
                    foreach (var child in node.Children)
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            touchSensor.metadata = metaStr;
                    break;
                case "Anchor":
                    go = new GameObject("Anchor");
                    var anchor = go.AddComponent<X3DAnchor>();
                    if (node.Attributes.TryGetValue("url", out string anchorUrlStr))
                        anchor.url = anchorUrlStr;
                    if (node.Attributes.TryGetValue("parameter", out string anchorParamStr))
                        anchor.parameter = anchorParamStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("description", out string anchorDescStr))
                        anchor.description = anchorDescStr;
                    if (node.Attributes.TryGetValue("DEF", out string defAnchor)) anchor.DEF = defAnchor;
                    if (node.Attributes.TryGetValue("USE", out string useAnchor)) anchor.USE = useAnchor;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldAnchor)) anchor.containerField = containerFieldAnchor;
                    foreach (var child in node.Children)
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            anchor.metadata = metaStr;
                    break;
                case "Transform":
                    go = new GameObject("Transform");
                    var transform = go.AddComponent<X3DTransform>();
                    if (node.Attributes.TryGetValue("translation", out string translation))
                        transform.translation = ParseVector3(translation);
                    if (node.Attributes.TryGetValue("rotation", out string rotation))
                        transform.rotation = ParseRotation(rotation);
                    if (node.Attributes.TryGetValue("scale", out string scale))
                        transform.scale = ParseVector3(scale);
                    if (node.Attributes.TryGetValue("center", out string center))
                        transform.center = ParseVector3(center);
                    if (node.Attributes.TryGetValue("scaleOrientation", out string scaleOrientation))
                        transform.scaleOrientation = ParseRotation(scaleOrientation);
                    if (node.Attributes.TryGetValue("DEF", out string defT)) transform.DEF = defT;
                    if (node.Attributes.TryGetValue("USE", out string useT)) transform.USE = useT;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldT)) transform.containerField = containerFieldT;
                    foreach (var child in node.Children)
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            transform.metadata = metaStr;
                    break;
                case "GeoLocation":
                    go = new GameObject("GeoLocation");
                    try
                    {
                        // Use the X3DGeographicHandler to parse and convert geoCoords to Unity position
                        if (node.Attributes.TryGetValue("geoCoords", out string geoCoords))
                        {
                            // Fake an XmlNode for compatibility with handler
                            var xmlDoc = new XmlDocument();
                            var geoNode = xmlDoc.CreateElement("GeoLocation");
                            geoNode.SetAttribute("geoCoords", geoCoords);
                            go.transform.position = X3D.X3DGeographicHandler.ParseGeoLocation(geoNode);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"GeoLocation parse error: {ex.Message}");
                    }
                    break;
                case "GeoViewpoint":
                    go = new GameObject("GeoViewpoint");
                    var geoViewpoint = go.AddComponent<X3DGeoViewpoint>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defGeoViewpoint)) geoViewpoint.DEF = defGeoViewpoint;
                    if (node.Attributes.TryGetValue("USE", out string useGeoViewpoint)) geoViewpoint.USE = useGeoViewpoint;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldGeoViewpoint)) geoViewpoint.containerField = containerFieldGeoViewpoint;

                    // GeoViewpoint fields
                    if (node.Attributes.TryGetValue("geoSystem", out string geoSystemStrViewpoint)) geoViewpoint.geoSystem = geoSystemStrViewpoint.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("geoCoords", out string geoCoordsStrViewpoint)) geoViewpoint.geoCoords = geoCoordsStrViewpoint;
                    if (node.Attributes.TryGetValue("orientation", out string orientationStr)) {
                        var vals = orientationStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (vals.Length == 4 && float.TryParse(vals[0], out float x) && float.TryParse(vals[1], out float y) && float.TryParse(vals[2], out float z) && float.TryParse(vals[3], out float angle))
                            geoViewpoint.orientation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, new Vector3(x, y, z));
                    }
                    if (node.Attributes.TryGetValue("fieldOfView", out string fovStr) && float.TryParse(fovStr, out float fov)) geoViewpoint.fieldOfView = fov;
                    if (node.Attributes.TryGetValue("description", out string descStrGeoViewpoint)) geoViewpoint.description = descStrGeoViewpoint;
                    if (node.Attributes.TryGetValue("jump", out string jumpStr)) geoViewpoint.jump = jumpStr == "true";
                    if (node.Attributes.TryGetValue("retainUserOffsets", out string retainStr)) geoViewpoint.retainUserOffsets = retainStr == "true";
                    if (node.Attributes.TryGetValue("centerOfRotation", out string centerStr)) {
                        var vals = centerStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (vals.Length == 3 && float.TryParse(vals[0], out float x) && float.TryParse(vals[1], out float y) && float.TryParse(vals[2], out float z))
                            geoViewpoint.centerOfRotation = new Vector3(x, y, z);
                    }

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataGeoViewpoint)) geoViewpoint.metadata = metadataGeoViewpoint;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            geoViewpoint.metadata = metaStr;
                    }
                    break;
                case "GeoOrigin":
                    go = new GameObject("GeoOrigin");
                    var geoOrigin = go.AddComponent<X3DGeoOrigin>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defGeoOrigin)) geoOrigin.DEF = defGeoOrigin;
                    if (node.Attributes.TryGetValue("USE", out string useGeoOrigin)) geoOrigin.USE = useGeoOrigin;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldGeoOrigin)) geoOrigin.containerField = containerFieldGeoOrigin;

                    // GeoOrigin fields
                    if (node.Attributes.TryGetValue("geoCoords", out string geoCoordsStrOrigin)) geoOrigin.geoCoords = geoCoordsStrOrigin;
                    if (node.Attributes.TryGetValue("geoSystem", out string geoSystemStrOrigin)) geoOrigin.geoSystem = geoSystemStrOrigin.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataGeoOrigin)) geoOrigin.metadata = metadataGeoOrigin;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            geoOrigin.metadata = metaStr;
                    }
                    break;
                case "GeoCoordinate":
                    go = new GameObject("GeoCoordinate");
                    var geoCoord = go.AddComponent<X3DGeoCoordinate>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defGeoCoord)) geoCoord.DEF = defGeoCoord;
                    if (node.Attributes.TryGetValue("USE", out string useGeoCoord)) geoCoord.USE = useGeoCoord;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldGeoCoord)) geoCoord.containerField = containerFieldGeoCoord;

                    // GeoCoordinate fields
                    if (node.Attributes.TryGetValue("point", out string geoPointStr))
                        geoCoord.point = Array.ConvertAll(geoPointStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries), double.Parse);
                    if (node.Attributes.TryGetValue("geoSystem", out string geoSystemStr))
                        geoCoord.geoSystem = geoSystemStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("geoCoords", out string geoCoordsStr))
                        geoCoord.geoCoords = geoCoordsStr;

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataGeoCoord)) geoCoord.metadata = metadataGeoCoord;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            geoCoord.metadata = metaStr;
                    }
                    break;
                case "GeoElevationGrid":
                    go = new GameObject("GeoElevationGrid");
                    var geoElev = go.AddComponent<X3DGeoElevationGrid>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defGeoElev)) geoElev.DEF = defGeoElev;
                    if (node.Attributes.TryGetValue("USE", out string useGeoElev)) geoElev.USE = useGeoElev;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldGeoElev)) geoElev.containerField = containerFieldGeoElev;

                    // GeoElevationGrid fields
                    if (node.Attributes.TryGetValue("height", out string heightStr))
                        geoElev.height = Array.ConvertAll(heightStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries), float.Parse);
                    if (node.Attributes.TryGetValue("xDimension", out string xDimStr) && int.TryParse(xDimStr, out int xDim))
                        geoElev.xDimension = xDim;
                    if (node.Attributes.TryGetValue("zDimension", out string zDimStr) && int.TryParse(zDimStr, out int zDim))
                        geoElev.zDimension = zDim;
                    if (node.Attributes.TryGetValue("xSpacing", out string xSpacingStr) && float.TryParse(xSpacingStr, out float xSpacing))
                        geoElev.xSpacing = xSpacing;
                    if (node.Attributes.TryGetValue("zSpacing", out string zSpacingStr) && float.TryParse(zSpacingStr, out float zSpacing))
                        geoElev.zSpacing = zSpacing;
                    if (node.Attributes.TryGetValue("geoSystem", out string geoSystemStrElev))
                        geoElev.geoSystem = geoSystemStrElev.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "GeoOrigin")
                        {
                            geoElev.geoOrigin = X3DNodeToGameObject(child);
                            if (geoElev.geoOrigin != null) geoElev.geoOrigin.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "Color")
                        {
                            geoElev.color = X3DNodeToGameObject(child);
                            if (geoElev.color != null) geoElev.color.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "Normal")
                        {
                            geoElev.normal = X3DNodeToGameObject(child);
                            if (geoElev.normal != null) geoElev.normal.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "TextureCoordinate")
                        {
                            geoElev.texCoord = X3DNodeToGameObject(child);
                            if (geoElev.texCoord != null) geoElev.texCoord.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            geoElev.metadata = metaStr;
                    }
                    if (node.Attributes.TryGetValue("colorPerVertex", out string colorPerVertexStr))
                        geoElev.colorPerVertex = colorPerVertexStr == "true";
                    if (node.Attributes.TryGetValue("normalPerVertex", out string normalPerVertexStr))
                        geoElev.normalPerVertex = normalPerVertexStr == "true";
                    if (node.Attributes.TryGetValue("ccw", out string ccwStr))
                        geoElev.ccw = ccwStr == "true";
                    if (node.Attributes.TryGetValue("solid", out string solidStr))
                        geoElev.solid = solidStr == "true";
                    if (node.Attributes.TryGetValue("creaseAngle", out string creaseAngleStr) && float.TryParse(creaseAngleStr, out float creaseAngle))
                        geoElev.creaseAngle = creaseAngle;
                    break;
                case "Shape":
                    go = new GameObject("Shape");
                    var shape = go.AddComponent<X3DShape>();
                    if (node.Attributes.TryGetValue("DEF", out string def)) shape.DEF = def;
                    if (node.Attributes.TryGetValue("USE", out string use)) shape.USE = use;
                    if (node.Attributes.TryGetValue("containerField", out string containerField)) shape.containerField = containerField;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "Appearance")
                        {
                            shape.appearance = X3DNodeToGameObject(child);
                            if (shape.appearance != null) shape.appearance.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "Box" || child.Name == "Cone" || child.Name == "Cylinder" || child.Name == "Sphere" || child.Name == "IndexedFaceSet" || child.Name == "IndexedLineSet" || child.Name == "PointSet" || child.Name == "LineSet" || child.Name == "TriangleSet" || child.Name == "QuadSet")
                        {
                            shape.geometry = X3DNodeToGameObject(child);
                            if (shape.geometry != null) shape.geometry.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            shape.metadata = metaStr;
                    }
                    break;
                case "Box":
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (node.Attributes.TryGetValue("size", out string boxSize))
                    {
                        go.transform.localScale = ParseVector3(boxSize);
                    }
                    break;
                case "Cone":
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    // X3D Cone defaults: height=2, bottomRadius=1
                    // Unity Cylinder: height=2, radius=0.5
                    float coneHeight = 2f; // X3D default
                    float coneBottomRadius = 1f; // X3D default
                    if (node.Attributes.TryGetValue("height", out string coneHeightStr))
                    {
                        float.TryParse(coneHeightStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out coneHeight);
                    }
                    if (node.Attributes.TryGetValue("bottomRadius", out string coneRadiusStr))
                    {
                        float.TryParse(coneRadiusStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out coneBottomRadius);
                    }
                    // Scale: Y = height/2, X/Z = bottomRadius * 2 (Unity cylinder radius is 0.5)
                    go.transform.localScale = new Vector3(coneBottomRadius * 2f, coneHeight / 2f, coneBottomRadius * 2f);
                    // Adjust mesh to approximate a cone (set top radius to 0)
                    var meshFilter = go.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        Mesh mesh = meshFilter.mesh;
                        Vector3[] vertices = mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            if (vertices[i].y > 0) // top cap
                                vertices[i].x = vertices[i].z = 0;
                        }
                        mesh.vertices = vertices;
                        mesh.RecalculateNormals();
                    }
                    break;
                case "Cylinder":
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    // X3D Cylinder defaults: height=2, radius=1
                    // Unity Cylinder: height=2, radius=0.5
                    // So: Y scale = X3D height / 2, X/Z scale = X3D radius / 0.5 = X3D radius * 2
                    float cylHeight = 2f; // X3D default
                    float cylRadius = 1f; // X3D default
                    if (node.Attributes.TryGetValue("height", out string cylHeightStr))
                    {
                        float.TryParse(cylHeightStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out cylHeight);
                    }
                    if (node.Attributes.TryGetValue("radius", out string cylRadiusStr))
                    {
                        float.TryParse(cylRadiusStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out cylRadius);
                    }
                    go.transform.localScale = new Vector3(cylRadius * 2f, cylHeight / 2f, cylRadius * 2f);
                    break;
                case "PointSet":
                    go = new GameObject("PointSet");
                    var pointSet = go.AddComponent<X3DPointSet>();
                    // Optional fields: color, coord, metadata
                    if (node.Attributes.TryGetValue("DEF", out string defPointSet)) pointSet.DEF = defPointSet;
                    if (node.Attributes.TryGetValue("USE", out string usePointSet)) pointSet.USE = usePointSet;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldPointSet)) pointSet.containerField = containerFieldPointSet;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string colorStr))
                        {
                            // Parse color array (space/comma separated)
                            pointSet.colors = ParseColorArray(colorStr);
                        }
                        if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string pointStr2))
                        {
                            // Parse point array (space/comma separated)
                            pointSet.points = ParseVector3Array(pointStr2);
                        }
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                        {
                            pointSet.metadata = metaStr;
                        }
                    }
                    break;
                case "LineSet":
                    go = new GameObject("LineSet");
                    var lineSet = go.AddComponent<X3DLineSet>();
                    // Optional fields: color, coordinate, colorIndex, coordIndex, metadata
                    if (node.Attributes.TryGetValue("DEF", out string defLineSet)) lineSet.DEF = defLineSet;
                    if (node.Attributes.TryGetValue("USE", out string useLineSet)) lineSet.USE = useLineSet;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldLineSet)) lineSet.containerField = containerFieldLineSet;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string colorStr))
                        {
                            lineSet.colors = ParseColorArray(colorStr);
                        }
                        if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string pointStr3))
                        {
                            lineSet.points = ParseVector3Array(pointStr3);
                        }
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                        {
                            lineSet.metadata = metaStr;
                        }
                    }
                    if (node.Attributes.TryGetValue("colorIndex", out string colorIndexStr))
                        lineSet.colorIndex = ParseIntArray(colorIndexStr);
                    if (node.Attributes.TryGetValue("coordIndex", out string coordIndexStr))
                        lineSet.coordIndex = ParseIntArray(coordIndexStr);
                    break;
                case "Rectangle2D":
                    go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    // X3D Rectangle2D defaults: size="2 2" (width, height)
                    // Unity Quad: 1x1 unit
                    if (node.Attributes.TryGetValue("size", out string rect2DSize))
                    {
                        Vector2 rectSize = ParseVector2(rect2DSize);
                        go.transform.localScale = new Vector3(rectSize.x, rectSize.y, 1f);
                    }
                    else
                    {
                        // X3D default size is 2x2
                        go.transform.localScale = new Vector3(2f, 2f, 1f);
                    }
                    break;
                case "TriangleSet":
                    go = new GameObject("TriangleSet");
                    var triSet = go.AddComponent<X3DTriangleSet>();
                    if (node.Attributes.TryGetValue("DEF", out var defTriangleSetAttr)) triSet.DEF = defTriangleSetAttr;
                    if (node.Attributes.TryGetValue("USE", out var useTriangleSetAttr)) triSet.USE = useTriangleSetAttr;
                    if (node.Attributes.TryGetValue("containerField", out var containerFieldTriangleSetAttr)) triSet.containerField = containerFieldTriangleSetAttr;
                    // Optional fields: color, coordinate, normal, texCoord, colorIndex, coordIndex, normalIndex, texCoordIndex, metadata
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string triColorStr))
                            triSet.colors = ParseColorArray(triColorStr);
                        if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string triPointStr))
                            triSet.points = ParseVector3Array(triPointStr);
                        if (child.Name == "Normal" && child.Attributes.TryGetValue("vector", out string triNormalStr))
                            triSet.normals = ParseVector3Array(triNormalStr);
                        if (child.Name == "TextureCoordinate" && child.Attributes.TryGetValue("point", out string triTexStr))
                            triSet.texCoords = ParseVector2Array(triTexStr);
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string triMetaStr))
                            triSet.metadata = triMetaStr;
                    }
                    if (node.Attributes.TryGetValue("colorIndex", out string triColorIndexStr))
                        triSet.colorIndex = ParseIntArray(triColorIndexStr);
                    if (node.Attributes.TryGetValue("coordIndex", out string triCoordIndexStr))
                        triSet.coordIndex = ParseIntArray(triCoordIndexStr);
                    if (node.Attributes.TryGetValue("normalIndex", out string triNormalIndexStr))
                        triSet.normalIndex = ParseIntArray(triNormalIndexStr);
                    if (node.Attributes.TryGetValue("texCoordIndex", out string triTexCoordIndexStr))
                        triSet.texCoordIndex = ParseIntArray(triTexCoordIndexStr);
                    break;
                case "QuadSet":
                    go = new GameObject("QuadSet");
                    var quadSet = go.AddComponent<X3DQuadSet>();
                    if (node.Attributes.TryGetValue("DEF", out var defQuadSetAttr)) quadSet.DEF = defQuadSetAttr;
                    if (node.Attributes.TryGetValue("USE", out var useQuadSetAttr)) quadSet.USE = useQuadSetAttr;
                    if (node.Attributes.TryGetValue("containerField", out var containerFieldQuadSetAttr)) quadSet.containerField = containerFieldQuadSetAttr;
                    // Optional fields: color, coordinate, normal, texCoord, colorIndex, coordIndex, normalIndex, texCoordIndex, metadata
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string quadColorStr))
                            quadSet.colors = ParseColorArray(quadColorStr);
                        if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string quadPointStr))
                            quadSet.points = ParseVector3Array(quadPointStr);
                        if (child.Name == "Normal" && child.Attributes.TryGetValue("vector", out string quadNormalStr))
                            quadSet.normals = ParseVector3Array(quadNormalStr);
                        if (child.Name == "TextureCoordinate" && child.Attributes.TryGetValue("point", out string quadTexStr))
                            quadSet.texCoords = ParseVector2Array(quadTexStr);
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string quadMetaStr))
                            quadSet.metadata = quadMetaStr;
                    }
                    if (node.Attributes.TryGetValue("colorIndex", out string quadColorIndexStr))
                        quadSet.colorIndex = ParseIntArray(quadColorIndexStr);
                    if (node.Attributes.TryGetValue("coordIndex", out string quadCoordIndexStr))
                        quadSet.coordIndex = ParseIntArray(quadCoordIndexStr);
                    if (node.Attributes.TryGetValue("normalIndex", out string quadNormalIndexStr))
                        quadSet.normalIndex = ParseIntArray(quadNormalIndexStr);
                    if (node.Attributes.TryGetValue("texCoordIndex", out string quadTexCoordIndexStr))
                        quadSet.texCoordIndex = ParseIntArray(quadTexCoordIndexStr);
                    break;
                case "IndexedFaceSet":
                    go = CreateIndexedFaceSetMesh(node);
                    break;
                case "IndexedLineSet":
                    go = CreateIndexedLineSetMesh(node);
                    break;
                case "IndexedTriangleSet":
                    go = CreateIndexedTriangleSetMesh(node);
                    break;
                case "IndexedTriangleFanSet":
                case "IndexedTriangleStripSet":
                    // Fall back to IndexedTriangleSet logic
                    go = CreateIndexedTriangleSetMesh(node);
                    break;
                case "ElevationGrid":
                    go = CreateElevationGridMesh(node);
                    break;
                case "Extrusion":
                    go = CreateExtrusionMesh(node);
                    break;
                case "Sphere":
                    go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    // X3D Sphere default: radius=1
                    // Unity Sphere: diameter=1 (radius=0.5)
                    // So: scale = X3D radius * 2
                    float sphereRadius = 1f; // X3D default
                    if (node.Attributes.TryGetValue("radius", out string radiusStr))
                    {
                        float.TryParse(radiusStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out sphereRadius);
                    }
                    go.transform.localScale = Vector3.one * sphereRadius * 2f;
                    break;
                case "Appearance":
                    go = new GameObject("Appearance");
                    var app = go.AddComponent<X3DAppearance>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defApp)) app.DEF = defApp;
                    if (node.Attributes.TryGetValue("USE", out string useApp)) app.USE = useApp;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldApp)) app.containerField = containerFieldApp;

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadata)) app.metadata = metadata;

                    // Appearance children
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "Material")
                        {
                            app.material = X3DNodeToGameObject(child);
                            if (app.material != null) app.material.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "ImageTexture" || child.Name == "MovieTexture" || child.Name == "PixelTexture")
                        {
                            app.texture = X3DNodeToGameObject(child);
                            if (app.texture != null) app.texture.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "TextureTransform")
                        {
                            app.textureTransform = X3DNodeToGameObject(child);
                            if (app.textureTransform != null) app.textureTransform.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "FillProperties")
                        {
                            app.fillProperties = X3DNodeToGameObject(child);
                            if (app.fillProperties != null) app.fillProperties.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "LineProperties")
                        {
                            app.lineProperties = X3DNodeToGameObject(child);
                            if (app.lineProperties != null) app.lineProperties.transform.SetParent(go.transform, false);
                        }
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            app.metadata = metaStr;
                    }
                    break;
                case "Material":
                    go = new GameObject("Material");
                    var matComp = go.AddComponent<X3DMaterialComponent>();
                    if (node.Attributes.TryGetValue("diffuseColor", out string diffuse))
                        matComp.diffuseColor = ParseColor(diffuse);
                    if (node.Attributes.TryGetValue("emissiveColor", out string emissive))
                        matComp.emissiveColor = ParseColor(emissive);
                    if (node.Attributes.TryGetValue("specularColor", out string specular))
                        matComp.specularColor = ParseColor(specular);
                    if (node.Attributes.TryGetValue("ambientIntensity", out string ambientIntensity) && float.TryParse(ambientIntensity, out float ambInt))
                        matComp.ambientIntensity = ambInt;
                    if (node.Attributes.TryGetValue("shininess", out string shininess) && float.TryParse(shininess, out float shin))
                        matComp.shininess = shin;
                    if (node.Attributes.TryGetValue("transparency", out string transparency) && float.TryParse(transparency, out float transp))
                        matComp.transparency = transp;
                    if (node.Attributes.TryGetValue("isSmooth", out string isSmooth))
                        matComp.isSmooth = isSmooth == "true";
                    if (node.Attributes.TryGetValue("isLit", out string isLit))
                        matComp.isLit = isLit == "true";
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            matComp.metadata = metaStr;
                    }
                    break;
                case "ImageTexture":
                    go = new GameObject("ImageTexture");
                    var texComp2 = go.AddComponent<X3DImageTextureComponent>();
                    if (node.Attributes.TryGetValue("url", out string texUrl))
                        texComp2.url = texUrl;
                    if (node.Attributes.TryGetValue("repeatS", out string repeatS))
                        texComp2.repeatS = repeatS == "true";
                    if (node.Attributes.TryGetValue("repeatT", out string repeatT))
                        texComp2.repeatT = repeatT == "true";
                    if (node.Attributes.TryGetValue("urls", out string urls))
                        texComp2.urls = urls.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            texComp2.metadata = metaStr;
                    }
                    break;
                case "MovieTexture":
                    go = new GameObject("MovieTexture");
                    var movComp2 = go.AddComponent<X3DMovieTextureComponent>();
                    if (node.Attributes.TryGetValue("url", out string movUrl))
                        movComp2.url = movUrl;
                    if (node.Attributes.TryGetValue("loop", out string loop))
                        movComp2.loop = loop == "true";
                    if (node.Attributes.TryGetValue("speed", out string speed) && float.TryParse(speed, out float spd))
                        movComp2.speed = spd;
                    if (node.Attributes.TryGetValue("startTime", out string startTime) && float.TryParse(startTime, out float st))
                        movComp2.startTime = st;
                    if (node.Attributes.TryGetValue("stopTime", out string stopTime) && float.TryParse(stopTime, out float spt))
                        movComp2.stopTime = spt;
                    if (node.Attributes.TryGetValue("repeatS", out string repeatS2))
                        movComp2.repeatS = repeatS2 == "true";
                    if (node.Attributes.TryGetValue("repeatT", out string repeatT2))
                        movComp2.repeatT = repeatT2 == "true";
                    if (node.Attributes.TryGetValue("urls", out string urls2))
                        movComp2.urls = urls2.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            movComp2.metadata = metaStr;
                    }
                    break;
                case "Sound":
                    go = new GameObject("Sound");
                    var audioSource = go.AddComponent<AudioSource>();
                    var sound = go.AddComponent<X3DSound>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defSound)) sound.DEF = defSound;
                    if (node.Attributes.TryGetValue("USE", out string useSound)) sound.USE = useSound;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldSound)) sound.containerField = containerFieldSound;

                    // Sound fields
                    if (node.Attributes.TryGetValue("direction", out string dirStr)) sound.direction = ParseVector3(dirStr);
                    if (node.Attributes.TryGetValue("intensity", out string intensityStr) && float.TryParse(intensityStr, out float intenVal)) {
                        sound.intensity = intenVal;
                        audioSource.volume = intenVal;
                    }
                    if (node.Attributes.TryGetValue("location", out string locStr)) sound.location = ParseVector3(locStr);
                    if (node.Attributes.TryGetValue("maxBack", out string maxBackStr) && float.TryParse(maxBackStr, out float mbVal)) sound.maxBack = mbVal;
                    if (node.Attributes.TryGetValue("maxFront", out string maxFrontStr) && float.TryParse(maxFrontStr, out float mfVal)) sound.maxFront = mfVal;
                    if (node.Attributes.TryGetValue("minBack", out string minBackStr) && float.TryParse(minBackStr, out float minbVal)) sound.minBack = minbVal;
                    if (node.Attributes.TryGetValue("minFront", out string minFrontStr) && float.TryParse(minFrontStr, out float minfVal)) sound.minFront = minfVal;
                    if (node.Attributes.TryGetValue("priority", out string priorityStr) && float.TryParse(priorityStr, out float prioVal)) sound.priority = prioVal;
                    if (node.Attributes.TryGetValue("spatialize", out string spatializeStr)) sound.spatialize = spatializeStr == "true";

                    // Attach AudioClip if AudioClip child exists
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "AudioClip" && child.Attributes.TryGetValue("url", out string audioUrl))
                        {
                            string path = audioUrl.Split(' ', '\t', '\n', '\r')[0];
                            var clip = Resources.Load<AudioClip>(path);
                            if (clip != null) {
                                audioSource.clip = clip;
                                sound.source = go;
                            }
                        }
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            sound.metadata = metaStr;
                    }
                    break;
                case "AudioClip":
                    go = new GameObject("AudioClip");
                    var audioComp2 = go.AddComponent<X3DAudioClipComponent>();
                    if (node.Attributes.TryGetValue("url", out string audioUrl2))
                        audioComp2.url = audioUrl2;
                    if (node.Attributes.TryGetValue("description", out string description))
                        audioComp2.description = description;
                    if (node.Attributes.TryGetValue("loop", out string loop2))
                        audioComp2.loop = loop2 == "true";
                    if (node.Attributes.TryGetValue("pitch", out string pitch) && float.TryParse(pitch, out float ptch))
                        audioComp2.pitch = ptch;
                    if (node.Attributes.TryGetValue("startTime", out string startTime2) && float.TryParse(startTime2, out float st2))
                        audioComp2.startTime = st2;
                    if (node.Attributes.TryGetValue("stopTime", out string stopTime2) && float.TryParse(stopTime2, out float spt2))
                        audioComp2.stopTime = spt2;
                    if (node.Attributes.TryGetValue("urls", out string urls3))
                        audioComp2.urls = urls3.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            audioComp2.metadata = metaStr;
                    }
                    break;
                case "Script":
                    go = new GameObject("Script");
                    var scriptComp = go.AddComponent<X3DScriptComponent>();
                    // Store script source from inner text (inline script)
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        scriptComp.scriptSource = node.InnerText;
                    // Store URLs if present
                    if (node.Attributes.TryGetValue("url", out string urlStr))
                        scriptComp.urls = urlStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            scriptComp.metadata = metaStr;
                    }
                    break;
                case "Inline":
                    go = new GameObject("InlineModel");
                    var inlineLoader = go.AddComponent<X3DInlineModelLoader>();
                    if (node.Attributes.TryGetValue("url", out string inlineUrlStr))
                        inlineLoader.urls = inlineUrlStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            inlineLoader.metadata = metaStr;
                    }
                    // Optionally, call inlineLoader.LoadModel() here or from another script after instantiation
                    break;
                case "Billboard":
                    go = new GameObject("Billboard");
                    var billboard = go.AddComponent<X3DBillboard>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defBillboard)) billboard.DEF = defBillboard;
                    if (node.Attributes.TryGetValue("USE", out string useBillboard)) billboard.USE = useBillboard;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldBillboard)) billboard.containerField = containerFieldBillboard;

                    // Billboard fields
                    if (node.Attributes.TryGetValue("axisOfRotation", out string axisStr)) {
                        var axisVals = axisStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (axisVals.Length == 3 && float.TryParse(axisVals[0], out float x) && float.TryParse(axisVals[1], out float y) && float.TryParse(axisVals[2], out float z))
                            billboard.axisOfRotation = new UnityEngine.Vector3(x, y, z);
                    }

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataBillboard)) billboard.metadata = metadataBillboard;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            billboard.metadata = metaStr;
                    }
                    // Optionally, add a Billboard script to always face camera
                    break;
                case "ParticleSystem":
                    go = new GameObject("ParticleSystem");
                    var psys = go.AddComponent<ParticleSystem>();
                    var x3dpsys = go.AddComponent<X3DParticleSystem>();
                    var psysMain = psys.main;
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defPS)) x3dpsys.DEF = defPS;
                    if (node.Attributes.TryGetValue("USE", out string usePS)) x3dpsys.USE = usePS;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldPS)) x3dpsys.containerField = containerFieldPS;

                    // ParticleSystem fields
                    if (node.Attributes.TryGetValue("color", out string psColorStr)) {
                        x3dpsys.color = ParseColor(psColorStr);
                        psysMain.startColor = x3dpsys.color;
                    }
                    if (node.Attributes.TryGetValue("size", out string psSizeStr) && float.TryParse(psSizeStr, out float psSize)) {
                        x3dpsys.size = psSize;
                        psysMain.startSize = psSize;
                    }
                    if (node.Attributes.TryGetValue("lifetime", out string psLifetimeStr) && float.TryParse(psLifetimeStr, out float psLifetime)) {
                        x3dpsys.lifetime = psLifetime;
                        psysMain.startLifetime = psLifetime;
                    }
                    if (node.Attributes.TryGetValue("rate", out string psRateStr) && float.TryParse(psRateStr, out float psRate)) {
                        x3dpsys.rate = psRate;
                        var emission = psys.emission;
                        emission.rateOverTime = psRate;
                    }
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            x3dpsys.metadata = metaStr;
                    }
                    // You can add more mappings for X3D particle fields as needed
                    break;
                case "DirectionalLight":
                    go = new GameObject("DirectionalLight");
                    var dirLight = go.AddComponent<Light>();
                    dirLight.type = LightType.Directional;
                    var dir = go.AddComponent<X3DDirectionalLight>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defDir)) dir.DEF = defDir;
                    if (node.Attributes.TryGetValue("USE", out string useDir)) dir.USE = useDir;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldDir)) dir.containerField = containerFieldDir;

                    // DirectionalLight fields
                    if (node.Attributes.TryGetValue("color", out string dirColor)) {
                        dir.color = ParseColor(dirColor);
                        dirLight.color = dir.color;
                    }
                    if (node.Attributes.TryGetValue("intensity", out string dirIntensity) && float.TryParse(dirIntensity, out float dInt)) {
                        dir.intensity = dInt;
                        dirLight.intensity = dInt;
                    }
                    if (node.Attributes.TryGetValue("direction", out string dirVecStr)) {
                        var dirVals = dirVecStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (dirVals.Length == 3 && float.TryParse(dirVals[0], out float x) && float.TryParse(dirVals[1], out float y) && float.TryParse(dirVals[2], out float z))
                            dir.direction = new UnityEngine.Vector3(x, y, z);
                    }
                    if (node.Attributes.TryGetValue("ambientIntensity", out string ambientStr) && float.TryParse(ambientStr, out float ambientVal))
                        dir.ambientIntensity = ambientVal;
                    if (node.Attributes.TryGetValue("on", out string onStr))
                        dir.on = onStr == "true";
                    if (node.Attributes.TryGetValue("shadowIntensity", out string shadowStr) && float.TryParse(shadowStr, out float shadowVal))
                        dir.shadowIntensity = shadowVal;
                    if (node.Attributes.TryGetValue("global", out string globalStr))
                        dir.global = globalStr == "true";

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataDir)) dir.metadata = metadataDir;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            dir.metadata = metaStr;
                    }
                    break;
                case "PointLight":
                    go = new GameObject("PointLight");
                    var pointLight = go.AddComponent<Light>();
                    pointLight.type = LightType.Point;
                    var pt = go.AddComponent<X3DPointLight>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defPt)) pt.DEF = defPt;
                    if (node.Attributes.TryGetValue("USE", out string usePt)) pt.USE = usePt;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldPt)) pt.containerField = containerFieldPt;

                    // PointLight fields
                    if (node.Attributes.TryGetValue("color", out string ptColor)) {
                        pt.color = ParseColor(ptColor);
                        pointLight.color = pt.color;
                    }
                    if (node.Attributes.TryGetValue("intensity", out string ptIntensity) && float.TryParse(ptIntensity, out float pInt)) {
                        pt.intensity = pInt;
                        pointLight.intensity = pInt;
                    }
                    if (node.Attributes.TryGetValue("location", out string ptLoc))
                        pt.location = ParseVector3(ptLoc);
                    if (node.Attributes.TryGetValue("radius", out string ptRange) && float.TryParse(ptRange, out float pRange)) {
                        pt.radius = pRange;
                        pointLight.range = pRange;
                    }
                    if (node.Attributes.TryGetValue("ambientIntensity", out string ambientStrPt) && float.TryParse(ambientStrPt, out float ambientValPt))
                        pt.ambientIntensity = ambientValPt;
                    if (node.Attributes.TryGetValue("attenuation", out string attenStr)) {
                        var vals = attenStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (vals.Length == 3 && float.TryParse(vals[0], out float x) && float.TryParse(vals[1], out float y) && float.TryParse(vals[2], out float z))
                            pt.attenuation = new Vector3(x, y, z);
                    }
                    if (node.Attributes.TryGetValue("on", out string onStrPt))
                        pt.on = onStrPt == "true";
                    if (node.Attributes.TryGetValue("shadowIntensity", out string shadowStrPt) && float.TryParse(shadowStrPt, out float shadowValPt))
                        pt.shadowIntensity = shadowValPt;
                    if (node.Attributes.TryGetValue("global", out string globalStrPt))
                        pt.global = globalStrPt == "true";

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataPt)) pt.metadata = metadataPt;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            pt.metadata = metaStr;
                    }
                    break;
                case "SpotLight":
                    go = new GameObject("SpotLight");
                    var spotLight = go.AddComponent<Light>();
                    spotLight.type = LightType.Spot;
                    var sp = go.AddComponent<X3DSpotLight>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defSp)) sp.DEF = defSp;
                    if (node.Attributes.TryGetValue("USE", out string useSp)) sp.USE = useSp;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldSp)) sp.containerField = containerFieldSp;

                    // SpotLight fields
                    if (node.Attributes.TryGetValue("color", out string spColor)) {
                        sp.color = ParseColor(spColor);
                        spotLight.color = sp.color;
                    }
                    if (node.Attributes.TryGetValue("intensity", out string spIntensity) && float.TryParse(spIntensity, out float sInt)) {
                        sp.intensity = sInt;
                        spotLight.intensity = sInt;
                    }
                    if (node.Attributes.TryGetValue("location", out string spLoc)) sp.location = ParseVector3(spLoc);
                    if (node.Attributes.TryGetValue("radius", out string spRange) && float.TryParse(spRange, out float sRange)) {
                        sp.radius = sRange;
                        spotLight.range = sRange;
                    }
                    if (node.Attributes.TryGetValue("ambientIntensity", out string ambientStrSp) && float.TryParse(ambientStrSp, out float ambientValSp))
                        sp.ambientIntensity = ambientValSp;
                    if (node.Attributes.TryGetValue("attenuation", out string attenStrSp)) {
                        var vals = attenStrSp.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (vals.Length == 3 && float.TryParse(vals[0], out float x) && float.TryParse(vals[1], out float y) && float.TryParse(vals[2], out float z))
                            sp.attenuation = new Vector3(x, y, z);
                    }
                    if (node.Attributes.TryGetValue("beamWidth", out string beamWidthStrSp) && float.TryParse(beamWidthStrSp, out float beamWidthValSp)) {
                        sp.beamWidth = beamWidthValSp;
                        spotLight.spotAngle = beamWidthValSp * Mathf.Rad2Deg;
                    }
                    if (node.Attributes.TryGetValue("cutOffAngle", out string cutOffStrSp) && float.TryParse(cutOffStrSp, out float cutOffValSp))
                        sp.cutOffAngle = cutOffValSp;
                    if (node.Attributes.TryGetValue("direction", out string dirStrSp)) sp.direction = ParseVector3(dirStrSp);
                    if (node.Attributes.TryGetValue("on", out string onStrSp)) sp.on = onStrSp == "true";
                    if (node.Attributes.TryGetValue("shadowIntensity", out string shadowStrSp) && float.TryParse(shadowStrSp, out float shadowValSp)) sp.shadowIntensity = shadowValSp;
                    if (node.Attributes.TryGetValue("global", out string globalStrSp)) sp.global = globalStrSp == "true";

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataSp)) sp.metadata = metadataSp;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            sp.metadata = metaStr;
                    }
                    break;
                case "Background":
                    go = new GameObject("Background");
                    var bg = go.AddComponent<X3DBackground>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defBg)) bg.DEF = defBg;
                    if (node.Attributes.TryGetValue("USE", out string useBg)) bg.USE = useBg;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldBg)) bg.containerField = containerFieldBg;

                    // Background fields
                    if (node.Attributes.TryGetValue("skyColor", out string skyColorStr))
                        bg.skyColor = skyColorStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("groundColor", out string groundColorStr))
                        bg.groundColor = groundColorStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("skyAngle", out string skyAngleStr))
                        bg.skyAngle = Array.ConvertAll(skyAngleStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries), float.Parse);
                    if (node.Attributes.TryGetValue("groundAngle", out string groundAngleStr))
                        bg.groundAngle = Array.ConvertAll(groundAngleStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries), float.Parse);
                    if (node.Attributes.TryGetValue("transparency", out string transparencyStr) && float.TryParse(transparencyStr, out float transparencyVal))
                        bg.transparency = transparencyVal;

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataBg)) bg.metadata = metadataBg;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            bg.metadata = metaStr;
                    }
                    break;
                case "Fog":
                    go = new GameObject("Fog");
                    var fog = go.AddComponent<X3DFog>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defFog)) fog.DEF = defFog;
                    if (node.Attributes.TryGetValue("USE", out string useFog)) fog.USE = useFog;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldFog)) fog.containerField = containerFieldFog;

                    // Fog fields
                    if (node.Attributes.TryGetValue("color", out string fogColor)) {
                        fog.color = ParseColor(fogColor);
                        RenderSettings.fogColor = fog.color;
                    }
                    if (node.Attributes.TryGetValue("fogType", out string fogTypeStr))
                        fog.fogType = fogTypeStr;
                    if (node.Attributes.TryGetValue("visibilityRange", out string fogRange) && float.TryParse(fogRange, out float fRange)) {
                        fog.visibilityRange = fRange;
                        RenderSettings.fogEndDistance = fRange;
                    }
                    RenderSettings.fog = true;

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataFog)) fog.metadata = metadataFog;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            fog.metadata = metaStr;
                    }
                    break;
                case "NavigationInfo":
                    go = new GameObject("NavigationInfo");
                    var navInfo = go.AddComponent<X3DNavigationInfo>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defNavInfo)) navInfo.DEF = defNavInfo;
                    if (node.Attributes.TryGetValue("USE", out string useNavInfo)) navInfo.USE = useNavInfo;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldNavInfo)) navInfo.containerField = containerFieldNavInfo;

                    // NavigationInfo fields
                    if (node.Attributes.TryGetValue("type", out string typeStr))
                        navInfo.type = typeStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("avatarSize", out string avatarSizeStr))
                        navInfo.avatarSize = Array.ConvertAll(avatarSizeStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries), float.Parse);
                    if (node.Attributes.TryGetValue("headlight", out string headlightStr))
                        navInfo.headlight = headlightStr == "true";
                    if (node.Attributes.TryGetValue("speed", out string speedStr) && float.TryParse(speedStr, out float speedVal))
                        navInfo.speed = speedVal;
                    if (node.Attributes.TryGetValue("visibilityLimit", out string visLimitStr) && float.TryParse(visLimitStr, out float visLimitVal))
                        navInfo.visibilityLimit = visLimitVal;
                    if (node.Attributes.TryGetValue("transitionType", out string transTypeStr))
                        navInfo.transitionType = transTypeStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (node.Attributes.TryGetValue("transitionTime", out string transTimeStr) && float.TryParse(transTimeStr, out float transTimeVal))
                        navInfo.transitionTime = transTimeVal;

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataNavInfo)) navInfo.metadata = metadataNavInfo;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            navInfo.metadata = metaStr;
                    }
                    // Placeholder: could be used to set camera movement style
                    break;
                case "Viewpoint":
                    go = new GameObject("Viewpoint");
                    var camObj = go.AddComponent<Camera>();
                    var vp = go.AddComponent<X3DViewpoint>();

                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out var defViewpointAttr)) vp.DEF = defViewpointAttr;
                    if (node.Attributes.TryGetValue("USE", out var useViewpointAttr)) vp.USE = useViewpointAttr;
                    if (node.Attributes.TryGetValue("containerField", out var containerFieldViewpointAttr)) vp.containerField = containerFieldViewpointAttr;

                    // X3D 4.0 Viewpoint fields
                    if (node.Attributes.TryGetValue("position", out string vpPos)) {
                        vp.position = ParseVector3(vpPos);
                        go.transform.position = vp.position;
                    }
                    if (node.Attributes.TryGetValue("orientation", out string vpRot)) {
                        vp.orientation = ParseRotation(vpRot);
                        go.transform.rotation = vp.orientation;
                    }
                    if (node.Attributes.TryGetValue("fieldOfView", out string fovStrViewpoint) && float.TryParse(fovStrViewpoint, out float fovValViewpoint)) {
                        vp.fieldOfView = fovValViewpoint;
                        camObj.fieldOfView = fovValViewpoint * Mathf.Rad2Deg;
                    }
                    if (node.Attributes.TryGetValue("description", out string descStrViewpoint)) vp.description = descStrViewpoint;
                    if (node.Attributes.TryGetValue("jump", out string jumpStrViewpoint)) vp.jump = jumpStrViewpoint.ToLower() == "true";
                    if (node.Attributes.TryGetValue("retainUserOffsets", out string retainStrViewpoint)) vp.retainUserOffsets = retainStrViewpoint.ToLower() == "true";
                    if (node.Attributes.TryGetValue("centerOfRotation", out string centerStrViewpoint)) vp.centerOfRotation = ParseVector3(centerStrViewpoint);

                    // Metadata
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            vp.metadata = metaStr;
                    }
                    break;
                case "Switch":
                    go = new GameObject("Switch");
                    var sw = go.AddComponent<X3DSwitch>();
                    if (node.Attributes.TryGetValue("DEF", out var defSwitchAttr)) sw.DEF = defSwitchAttr;
                    if (node.Attributes.TryGetValue("USE", out var useSwitchAttr)) sw.USE = useSwitchAttr;
                    if (node.Attributes.TryGetValue("containerField", out var containerFieldSwitchAttr)) sw.containerField = containerFieldSwitchAttr;
                    if (node.Attributes.TryGetValue("whichChoice", out string whichChoiceStr) && int.TryParse(whichChoiceStr, out int whichChoiceVal))
                        sw.whichChoice = whichChoiceVal;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            sw.metadata = metaStr;
                    }
                    break;
                case "LOD":
                    go = new GameObject("LOD");
                    var lod = go.AddComponent<X3DLOD>();
                    if (node.Attributes.TryGetValue("DEF", out var defLODAttr)) lod.DEF = defLODAttr;
                    if (node.Attributes.TryGetValue("USE", out var useLODAttr)) lod.USE = useLODAttr;
                    if (node.Attributes.TryGetValue("containerField", out var containerFieldLODAttr)) lod.containerField = containerFieldLODAttr;
                    if (node.Attributes.TryGetValue("center", out string centerStr2))
                        lod.center = ParseVector3(centerStr2);
                    if (node.Attributes.TryGetValue("range", out string rangeStr))
                        lod.range = ParseFloatArray(rangeStr);
                    // Assign child nodes to LOD.level array
                    var levels = new System.Collections.Generic.List<GameObject>();
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            lod.metadata = metaStr;
                        else
                        {
                            var levelGO = X3DNodeToGameObject(child);
                            if (levelGO != null)
                            {
                                levelGO.transform.SetParent(go.transform, false);
                                levels.Add(levelGO);
                            }
                        }
                    }
                    lod.level = levels.ToArray();
                    break;
                case "StaticGroup":
                    go = new GameObject("StaticGroup");
                    go.isStatic = true;
                    var staticGroup = go.AddComponent<X3DStaticGroup>();
                    if (node.Attributes.TryGetValue("bboxCenter", out string bboxCenterS))
                        staticGroup.bboxCenter = ParseVector3(bboxCenterS);
                    if (node.Attributes.TryGetValue("bboxSize", out string bboxSizeS))
                        staticGroup.bboxSize = ParseVector3(bboxSizeS);
                    if (node.Attributes.TryGetValue("DEF", out string defS)) staticGroup.DEF = defS;
                    if (node.Attributes.TryGetValue("USE", out string useS)) staticGroup.USE = useS;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldS)) staticGroup.containerField = containerFieldS;
                    foreach (var child in node.Children)
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            staticGroup.metadata = metaStr;
                    break;
                case "Scene":
                    go = new GameObject("Scene");
                    var scene = go.AddComponent<X3DScene>();
                    if (node.Attributes.TryGetValue("bboxCenter", out string bboxCenterScene))
                        scene.bboxCenter = ParseVector3(bboxCenterScene);
                    if (node.Attributes.TryGetValue("bboxSize", out string bboxSizeScene))
                        scene.bboxSize = ParseVector3(bboxSizeScene);
                    if (node.Attributes.TryGetValue("DEF", out string defScene)) scene.DEF = defScene;
                    if (node.Attributes.TryGetValue("USE", out string useScene)) scene.USE = useScene;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldScene)) scene.containerField = containerFieldScene;
                    foreach (var child in node.Children)
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            scene.metadata = metaStr;
                    break;
                case "WorldInfo":
                    go = new GameObject("WorldInfo");
                    var worldInfo = go.AddComponent<X3DWorldInfoComponent>();
                    if (node.Attributes.TryGetValue("DEF", out var defWorldInfoAttr)) worldInfo.DEF = defWorldInfoAttr;
                    if (node.Attributes.TryGetValue("USE", out var useWorldInfoAttr)) worldInfo.USE = useWorldInfoAttr;
                    if (node.Attributes.TryGetValue("containerField", out var containerFieldWorldInfoAttr)) worldInfo.containerField = containerFieldWorldInfoAttr;
                    if (node.Attributes.TryGetValue("title", out string titleStr))
                        worldInfo.title = titleStr;
                    if (node.Attributes.TryGetValue("info", out string infoStr))
                        worldInfo.info = infoStr.Split(new char[] { ' ', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            worldInfo.metadata = metaStr;
                    }
                    break;
                // Animation/Time nodes
                case "TimeSensor":
                    go = new GameObject("TimeSensor");
                    var timeSensor = go.AddComponent<X3DTimeSensor>();
                    if (node.Attributes.TryGetValue("cycleInterval", out string cycleIntervalStr) && float.TryParse(cycleIntervalStr, out float cycleInterval))
                        timeSensor.cycleInterval = cycleInterval;
                    if (node.Attributes.TryGetValue("loop", out string loopStr))
                        timeSensor.loop = loopStr == "true";
                    if (node.Attributes.TryGetValue("startTime", out string startTimeStr1) && float.TryParse(startTimeStr1, out float startTimeVal))
                        timeSensor.startTime = startTimeVal;
                    if (node.Attributes.TryGetValue("stopTime", out string stopTimeStr1) && float.TryParse(stopTimeStr1, out float stopTimeVal))
                        timeSensor.stopTime = stopTimeVal;
                    if (node.Attributes.TryGetValue("enabled", out string enabledStr1))
                        timeSensor.enabledSensor = enabledStr1 == "true";
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            timeSensor.metadata = metaStr;
                    }
                    break;
                case "PositionInterpolator":
                    go = new GameObject("PositionInterpolator");
                    var posInterp = go.AddComponent<X3DPositionInterpolator>();
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            posInterp.metadata = metaStr;
                    }
                    break;
                case "OrientationInterpolator":
                    go = new GameObject("OrientationInterpolator");
                    var oriInterp = go.AddComponent<X3DOrientationInterpolator>();
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            oriInterp.metadata = metaStr;
                    }
                    break;
                case "ColorInterpolator":
                    go = new GameObject("ColorInterpolator");
                    var colInterp = go.AddComponent<X3DColorInterpolator>();
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            colInterp.metadata = metaStr;
                    }
                    break;
                case "ScalarInterpolator":
                    go = new GameObject("ScalarInterpolator");
                    var scalInterp = go.AddComponent<X3DScalarInterpolatorStub>();
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            scalInterp.metadata = metaStr;
                    }
                    break;
                case "ROUTE":
                    go = new GameObject("ROUTE");
                    var route = go.AddComponent<X3DRoute>();
                    // Universal fields
                    if (node.Attributes.TryGetValue("DEF", out string defRoute)) route.DEF = defRoute;
                    if (node.Attributes.TryGetValue("USE", out string useRoute)) route.USE = useRoute;
                    if (node.Attributes.TryGetValue("containerField", out string containerFieldRoute)) route.containerField = containerFieldRoute;

                    // ROUTE fields
                    if (node.Attributes.TryGetValue("fromNode", out string fromNodeStr)) route.fromNode = fromNodeStr;
                    if (node.Attributes.TryGetValue("fromField", out string fromFieldStr)) route.fromField = fromFieldStr;
                    if (node.Attributes.TryGetValue("toNode", out string toNodeStr)) route.toNode = toNodeStr;
                    if (node.Attributes.TryGetValue("toField", out string toFieldStr)) route.toField = toFieldStr;

                    // Metadata
                    if (node.Attributes.TryGetValue("metadata", out string metadataRoute)) route.metadata = metadataRoute;
                    foreach (var child in node.Children)
                    {
                        if (child.Name == "MetadataString" && child.Attributes.TryGetValue("value", out string metaStr))
                            route.metadata = metaStr;
                    }
                    break;
                // Add more X3D node types here as needed
                default:
                    go = new GameObject(node.Name); // Fallback for unknown nodes
                    break;
            }
            // Recursively add children
            foreach (var child in node.Children)
            {
                GameObject childGO = X3DNodeToGameObject(child);
                childGO.transform.parent = go.transform;
            }
            return go;
        }

        // Ensures a Canvas exists in the scene and returns it
        private static Canvas EnsureCanvas()
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("X3DCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            return canvas;
        }

        // Helper: Parse X3D vector string (e.g., "1 2 3") to Vector3
        private static Vector3 ParseVector3(string str)
        {
            var parts = str.Split(' ');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            return Vector3.one;
        }

        // Helper: Parse X3D vector string (e.g., "1 2") to Vector2
        private static Vector2 ParseVector2(string str)
        {
            var parts = str.Split(' ');
            if (parts.Length >= 2 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y))
            {
                return new Vector2(x, y);
            }
            return Vector2.one;
        }

        // Helper: Parse X3D rotation string (axis-angle: "x y z angle") to Quaternion
        private static Quaternion ParseRotation(string str)
        {
            var parts = str.Split(' ');
            if (parts.Length == 4 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z) &&
                float.TryParse(parts[3], out float angle))
            {
                return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, new Vector3(x, y, z));
            }
            return Quaternion.identity;
        }


        // Helper: Parse X3D color string (e.g., "1 0 0") to Color
        private static Color ParseColor(string str)
        {
            var parts = str.Split(' ');
            if (parts.Length >= 3 &&
                float.TryParse(parts[0], out float r) &&
                float.TryParse(parts[1], out float g) &&
                float.TryParse(parts[2], out float b))
            {
                return new Color(r, g, b);
            }
            return Color.white;
        }

        // Helper: Parse X3D color array string (e.g., "1 0 0 0 1 0 0 0 1") to Color[]
        private static Color[] ParseColorArray(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
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

        // Helper: Parse X3D point array string (e.g., "0 0 0 1 1 1 2 2 2") to Vector3[]
        private static Vector3[] ParseVector3Array(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            int count = parts.Length / 3;
            Vector3[] vectors = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float.TryParse(parts[i * 3], out float x);
                float.TryParse(parts[i * 3 + 1], out float y);
                float.TryParse(parts[i * 3 + 2], out float z);
                vectors[i] = new Vector3(x, y, z);
            }
            return vectors;
        }

        // Helper: Parse X3D int array string (e.g., "0 1 2 3") to int[]
        private static int[] ParseIntArray(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            int[] arr = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                int.TryParse(parts[i], out arr[i]);
            }
            return arr;
        }

        // Helper: Parse X3D Vector2 array string (e.g., "0 0 1 0 1 1") to Vector2[]
        private static Vector2[] ParseVector2Array(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            int count = parts.Length / 2;
            Vector2[] vectors = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float.TryParse(parts[i * 2], out float x);
                float.TryParse(parts[i * 2 + 1], out float y);
                vectors[i] = new Vector2(x, y);
            }
            return vectors;
        }

        // Converts a Unity GameObject to an X3DNode (scaffold)
        public static X3DNode GameObjectToX3DNode(GameObject go)
        {
            X3DNode node = new X3DNode(go.name);
            // TODO: Add logic to serialize Unity components to X3D attributes
            foreach (Transform child in go.transform)
            {
                node.Children.Add(GameObjectToX3DNode(child.gameObject));
            }
            return node;
        }

        // Helper: Parse X3D float array string (e.g., "10 20 30") to float[]
        private static float[] ParseFloatArray(string str)
        {
            var parts = str.Split(new char[] { ' ', ',', '\t', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            float[] arr = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                float.TryParse(parts[i], out arr[i]);
            }
            return arr;
        }

        #region Indexed Geometry Builders

        /// <summary>
        /// Creates a mesh from an IndexedFaceSet node.
        /// IndexedFaceSet uses coordIndex to define faces, with -1 as face delimiter.
        /// </summary>
        private static GameObject CreateIndexedFaceSetMesh(X3DNode node)
        {
            GameObject go = new GameObject("IndexedFaceSet");
            
            Vector3[] coordinates = null;
            Vector3[] normals = null;
            Vector2[] texCoords = null;
            Color[] colors = null;
            int[] coordIndex = null;
            int[] normalIndex = null;
            int[] texCoordIndex = null;
            int[] colorIndex = null;
            bool ccw = true;
            bool solid = true;
            bool normalPerVertex = true;
            bool colorPerVertex = true;
            float creaseAngle = 0f;

            // Parse attributes
            if (node.Attributes.TryGetValue("coordIndex", out string coordIndexStr))
                coordIndex = ParseIntArray(coordIndexStr);
            if (node.Attributes.TryGetValue("normalIndex", out string normalIndexStr))
                normalIndex = ParseIntArray(normalIndexStr);
            if (node.Attributes.TryGetValue("texCoordIndex", out string texCoordIndexStr))
                texCoordIndex = ParseIntArray(texCoordIndexStr);
            if (node.Attributes.TryGetValue("colorIndex", out string colorIndexStr))
                colorIndex = ParseIntArray(colorIndexStr);
            if (node.Attributes.TryGetValue("ccw", out string ccwStr))
                ccw = ccwStr != "false";
            if (node.Attributes.TryGetValue("solid", out string solidStr))
                solid = solidStr != "false";
            if (node.Attributes.TryGetValue("normalPerVertex", out string npvStr))
                normalPerVertex = npvStr != "false";
            if (node.Attributes.TryGetValue("colorPerVertex", out string cpvStr))
                colorPerVertex = cpvStr != "false";
            if (node.Attributes.TryGetValue("creaseAngle", out string creaseAngleStr))
                float.TryParse(creaseAngleStr, out creaseAngle);

            // Parse child nodes for coordinate data
            foreach (var child in node.Children)
            {
                if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string pointStr))
                    coordinates = ParseVector3Array(pointStr);
                if (child.Name == "Normal" && child.Attributes.TryGetValue("vector", out string normalStr))
                    normals = ParseVector3Array(normalStr);
                if (child.Name == "TextureCoordinate" && child.Attributes.TryGetValue("point", out string texStr))
                    texCoords = ParseVector2Array(texStr);
                if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string colorStr))
                    colors = ParseColorArray(colorStr);
            }

            if (coordinates == null || coordIndex == null)
            {
                Debug.LogWarning("[X3DUnityConverter] IndexedFaceSet missing coordinate or coordIndex data");
                return go;
            }

            // Build mesh from indexed faces
            Mesh mesh = BuildIndexedFaceSetMesh(coordinates, coordIndex, normals, normalIndex, 
                texCoords, texCoordIndex, colors, colorIndex, ccw, normalPerVertex, colorPerVertex, creaseAngle);
            
            if (mesh != null)
            {
                go.AddComponent<MeshFilter>().mesh = mesh;
                go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }

            return go;
        }

        /// <summary>
        /// Builds a Unity Mesh from IndexedFaceSet data.
        /// </summary>
        private static Mesh BuildIndexedFaceSetMesh(Vector3[] coordinates, int[] coordIndex, 
            Vector3[] normals, int[] normalIndex, Vector2[] texCoords, int[] texCoordIndex,
            Color[] colors, int[] colorIndex, bool ccw, bool normalPerVertex, bool colorPerVertex, float creaseAngle)
        {
            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();
            var meshNormals = new System.Collections.Generic.List<Vector3>();
            var meshUVs = new System.Collections.Generic.List<Vector2>();
            var meshColors = new System.Collections.Generic.List<Color>();

            // Parse faces (delimited by -1)
            var faceIndices = new System.Collections.Generic.List<int>();
            int faceStart = 0;

            for (int i = 0; i <= coordIndex.Length; i++)
            {
                if (i == coordIndex.Length || coordIndex[i] == -1)
                {
                    // End of face
                    if (faceIndices.Count >= 3)
                    {
                        // Triangulate the face (fan triangulation)
                        int baseVertexIndex = vertices.Count;
                        
                        for (int j = 0; j < faceIndices.Count; j++)
                        {
                            int coordIdx = faceIndices[j];
                            vertices.Add(coordinates[coordIdx]);

                            // Handle normals
                            if (normals != null && normalPerVertex)
                            {
                                int nIdx = (normalIndex != null && faceStart + j < normalIndex.Length) 
                                    ? normalIndex[faceStart + j] : coordIdx;
                                if (nIdx >= 0 && nIdx < normals.Length)
                                    meshNormals.Add(normals[nIdx]);
                                else
                                    meshNormals.Add(Vector3.up);
                            }
                            else
                            {
                                meshNormals.Add(Vector3.up); // Will recalculate later
                            }

                            // Handle UVs
                            if (texCoords != null)
                            {
                                int tIdx = (texCoordIndex != null && faceStart + j < texCoordIndex.Length) 
                                    ? texCoordIndex[faceStart + j] : coordIdx;
                                if (tIdx >= 0 && tIdx < texCoords.Length)
                                    meshUVs.Add(texCoords[tIdx]);
                                else
                                    meshUVs.Add(Vector2.zero);
                            }
                            else
                            {
                                meshUVs.Add(Vector2.zero);
                            }

                            // Handle colors
                            if (colors != null && colorPerVertex)
                            {
                                int cIdx = (colorIndex != null && faceStart + j < colorIndex.Length) 
                                    ? colorIndex[faceStart + j] : coordIdx;
                                if (cIdx >= 0 && cIdx < colors.Length)
                                    meshColors.Add(colors[cIdx]);
                                else
                                    meshColors.Add(Color.white);
                            }
                            else
                            {
                                meshColors.Add(Color.white);
                            }
                        }

                        // Fan triangulation
                        for (int j = 1; j < faceIndices.Count - 1; j++)
                        {
                            if (ccw)
                            {
                                triangles.Add(baseVertexIndex);
                                triangles.Add(baseVertexIndex + j);
                                triangles.Add(baseVertexIndex + j + 1);
                            }
                            else
                            {
                                triangles.Add(baseVertexIndex);
                                triangles.Add(baseVertexIndex + j + 1);
                                triangles.Add(baseVertexIndex + j);
                            }
                        }
                    }

                    faceIndices.Clear();
                    faceStart = i + 1;
                }
                else
                {
                    faceIndices.Add(coordIndex[i]);
                }
            }

            if (vertices.Count == 0)
                return null;

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            
            if (meshUVs.Count == vertices.Count)
                mesh.uv = meshUVs.ToArray();
            
            if (meshColors.Count == vertices.Count)
                mesh.colors = meshColors.ToArray();

            // Recalculate normals if not provided or if creaseAngle is specified
            if (normals == null || creaseAngle > 0)
                mesh.RecalculateNormals();
            else if (meshNormals.Count == vertices.Count)
                mesh.normals = meshNormals.ToArray();

            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Creates a line renderer from an IndexedLineSet node.
        /// </summary>
        private static GameObject CreateIndexedLineSetMesh(X3DNode node)
        {
            GameObject go = new GameObject("IndexedLineSet");
            
            Vector3[] coordinates = null;
            Color[] colors = null;
            int[] coordIndex = null;
            int[] colorIndex = null;
            bool colorPerVertex = true;

            // Parse attributes
            if (node.Attributes.TryGetValue("coordIndex", out string coordIndexStr))
                coordIndex = ParseIntArray(coordIndexStr);
            if (node.Attributes.TryGetValue("colorIndex", out string colorIndexStr))
                colorIndex = ParseIntArray(colorIndexStr);
            if (node.Attributes.TryGetValue("colorPerVertex", out string cpvStr))
                colorPerVertex = cpvStr != "false";

            // Parse child nodes
            foreach (var child in node.Children)
            {
                if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string pointStr))
                    coordinates = ParseVector3Array(pointStr);
                if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string colorStr))
                    colors = ParseColorArray(colorStr);
            }

            if (coordinates == null)
            {
                Debug.LogWarning("[X3DUnityConverter] IndexedLineSet missing coordinate data");
                return go;
            }

            // Create LineRenderer for each polyline
            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;

            if (coordIndex != null)
            {
                // Build positions from indexed coordinates (stop at -1)
                var positions = new System.Collections.Generic.List<Vector3>();
                foreach (int idx in coordIndex)
                {
                    if (idx == -1) break; // First polyline only for simple LineRenderer
                    if (idx >= 0 && idx < coordinates.Length)
                        positions.Add(coordinates[idx]);
                }
                lineRenderer.positionCount = positions.Count;
                lineRenderer.SetPositions(positions.ToArray());
            }
            else
            {
                lineRenderer.positionCount = coordinates.Length;
                lineRenderer.SetPositions(coordinates);
            }

            // Apply colors if available
            if (colors != null && colors.Length > 0)
            {
                var gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(colors[0], 0f), new GradientColorKey(colors[Mathf.Min(1, colors.Length - 1)], 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                );
                lineRenderer.colorGradient = gradient;
            }

            return go;
        }

        /// <summary>
        /// Creates a mesh from an IndexedTriangleSet node.
        /// </summary>
        private static GameObject CreateIndexedTriangleSetMesh(X3DNode node)
        {
            GameObject go = new GameObject("IndexedTriangleSet");
            
            Vector3[] coordinates = null;
            Vector3[] normals = null;
            Vector2[] texCoords = null;
            Color[] colors = null;
            int[] index = null;
            bool ccw = true;
            bool solid = true;
            bool normalPerVertex = true;
            bool colorPerVertex = true;

            // Parse attributes
            if (node.Attributes.TryGetValue("index", out string indexStr))
                index = ParseIntArray(indexStr);
            if (node.Attributes.TryGetValue("ccw", out string ccwStr))
                ccw = ccwStr != "false";
            if (node.Attributes.TryGetValue("solid", out string solidStr))
                solid = solidStr != "false";
            if (node.Attributes.TryGetValue("normalPerVertex", out string npvStr))
                normalPerVertex = npvStr != "false";
            if (node.Attributes.TryGetValue("colorPerVertex", out string cpvStr))
                colorPerVertex = cpvStr != "false";

            // Parse child nodes
            foreach (var child in node.Children)
            {
                if (child.Name == "Coordinate" && child.Attributes.TryGetValue("point", out string pointStr))
                    coordinates = ParseVector3Array(pointStr);
                if (child.Name == "Normal" && child.Attributes.TryGetValue("vector", out string normalStr))
                    normals = ParseVector3Array(normalStr);
                if (child.Name == "TextureCoordinate" && child.Attributes.TryGetValue("point", out string texStr))
                    texCoords = ParseVector2Array(texStr);
                if (child.Name == "Color" && child.Attributes.TryGetValue("color", out string colorStr))
                    colors = ParseColorArray(colorStr);
            }

            if (coordinates == null || index == null)
            {
                Debug.LogWarning("[X3DUnityConverter] IndexedTriangleSet missing coordinate or index data");
                return go;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = coordinates;

            // Handle winding order
            int[] triangles = new int[index.Length];
            for (int i = 0; i < index.Length - 2; i += 3)
            {
                if (ccw)
                {
                    triangles[i] = index[i];
                    triangles[i + 1] = index[i + 1];
                    triangles[i + 2] = index[i + 2];
                }
                else
                {
                    triangles[i] = index[i];
                    triangles[i + 1] = index[i + 2];
                    triangles[i + 2] = index[i + 1];
                }
            }
            mesh.triangles = triangles;

            if (normals != null && normals.Length == coordinates.Length)
                mesh.normals = normals;
            else
                mesh.RecalculateNormals();

            if (texCoords != null && texCoords.Length == coordinates.Length)
                mesh.uv = texCoords;

            if (colors != null && colors.Length == coordinates.Length)
                mesh.colors = colors;

            mesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

            return go;
        }

        /// <summary>
        /// Creates a mesh from an ElevationGrid node.
        /// </summary>
        private static GameObject CreateElevationGridMesh(X3DNode node)
        {
            GameObject go = new GameObject("ElevationGrid");

            int xDimension = 0;
            int zDimension = 0;
            float xSpacing = 1f;
            float zSpacing = 1f;
            float[] height = null;
            bool ccw = true;
            bool solid = true;
            float creaseAngle = 0f;

            // Parse attributes
            if (node.Attributes.TryGetValue("xDimension", out string xDimStr))
                int.TryParse(xDimStr, out xDimension);
            if (node.Attributes.TryGetValue("zDimension", out string zDimStr))
                int.TryParse(zDimStr, out zDimension);
            if (node.Attributes.TryGetValue("xSpacing", out string xSpaceStr))
                float.TryParse(xSpaceStr, out xSpacing);
            if (node.Attributes.TryGetValue("zSpacing", out string zSpaceStr))
                float.TryParse(zSpaceStr, out zSpacing);
            if (node.Attributes.TryGetValue("height", out string heightStr))
                height = ParseFloatArray(heightStr);
            if (node.Attributes.TryGetValue("ccw", out string ccwStr))
                ccw = ccwStr != "false";
            if (node.Attributes.TryGetValue("solid", out string solidStr))
                solid = solidStr != "false";
            if (node.Attributes.TryGetValue("creaseAngle", out string creaseAngleStr))
                float.TryParse(creaseAngleStr, out creaseAngle);

            if (xDimension < 2 || zDimension < 2 || height == null)
            {
                Debug.LogWarning("[X3DUnityConverter] ElevationGrid invalid dimensions or missing height data");
                return go;
            }

            // Build mesh
            int vertexCount = xDimension * zDimension;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];

            for (int z = 0; z < zDimension; z++)
            {
                for (int x = 0; x < xDimension; x++)
                {
                    int idx = z * xDimension + x;
                    float h = (idx < height.Length) ? height[idx] : 0f;
                    vertices[idx] = new Vector3(x * xSpacing, h, z * zSpacing);
                    uvs[idx] = new Vector2((float)x / (xDimension - 1), (float)z / (zDimension - 1));
                }
            }

            // Build triangles
            int triCount = (xDimension - 1) * (zDimension - 1) * 6;
            int[] triangles = new int[triCount];
            int triIdx = 0;

            for (int z = 0; z < zDimension - 1; z++)
            {
                for (int x = 0; x < xDimension - 1; x++)
                {
                    int bl = z * xDimension + x;
                    int br = bl + 1;
                    int tl = (z + 1) * xDimension + x;
                    int tr = tl + 1;

                    if (ccw)
                    {
                        triangles[triIdx++] = bl;
                        triangles[triIdx++] = tl;
                        triangles[triIdx++] = br;
                        triangles[triIdx++] = br;
                        triangles[triIdx++] = tl;
                        triangles[triIdx++] = tr;
                    }
                    else
                    {
                        triangles[triIdx++] = bl;
                        triangles[triIdx++] = br;
                        triangles[triIdx++] = tl;
                        triangles[triIdx++] = br;
                        triangles[triIdx++] = tr;
                        triangles[triIdx++] = tl;
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

            return go;
        }

        /// <summary>
        /// Creates a mesh from an Extrusion node.
        /// </summary>
        private static GameObject CreateExtrusionMesh(X3DNode node)
        {
            GameObject go = new GameObject("Extrusion");

            Vector2[] crossSection = new Vector2[] { 
                new Vector2(1, 1), new Vector2(1, -1), 
                new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, 1) 
            };
            Vector3[] spine = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0) };
            Vector2[] scale = new Vector2[] { new Vector2(1, 1) };
            Vector4[] orientation = null;
            bool ccw = true;
            bool solid = true;
            bool beginCap = true;
            bool endCap = true;

            // Parse attributes
            if (node.Attributes.TryGetValue("crossSection", out string csStr))
                crossSection = ParseVector2Array(csStr);
            if (node.Attributes.TryGetValue("spine", out string spineStr))
                spine = ParseVector3Array(spineStr);
            if (node.Attributes.TryGetValue("scale", out string scaleStr))
                scale = ParseVector2Array(scaleStr);
            if (node.Attributes.TryGetValue("ccw", out string ccwStr))
                ccw = ccwStr != "false";
            if (node.Attributes.TryGetValue("solid", out string solidStr))
                solid = solidStr != "false";
            if (node.Attributes.TryGetValue("beginCap", out string beginCapStr))
                beginCap = beginCapStr != "false";
            if (node.Attributes.TryGetValue("endCap", out string endCapStr))
                endCap = endCapStr != "false";

            if (crossSection == null || crossSection.Length < 2 || spine == null || spine.Length < 2)
            {
                Debug.LogWarning("[X3DUnityConverter] Extrusion invalid crossSection or spine");
                return go;
            }

            // Build extruded mesh
            Mesh mesh = BuildExtrusionMesh(crossSection, spine, scale, ccw, beginCap, endCap);
            
            if (mesh != null)
            {
                go.AddComponent<MeshFilter>().mesh = mesh;
                go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            }

            return go;
        }

        /// <summary>
        /// Builds an extrusion mesh from cross-section and spine data.
        /// </summary>
        private static Mesh BuildExtrusionMesh(Vector2[] crossSection, Vector3[] spine, Vector2[] scale, bool ccw, bool beginCap, bool endCap)
        {
            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();
            var uvs = new System.Collections.Generic.List<Vector2>();

            int csCount = crossSection.Length;
            int spineCount = spine.Length;

            // Generate vertices along spine
            for (int s = 0; s < spineCount; s++)
            {
                Vector3 spinePoint = spine[s];
                Vector2 scaleVal = (s < scale.Length) ? scale[s] : scale[scale.Length - 1];

                // Calculate spine direction for orientation
                Vector3 spineDir = Vector3.up;
                if (s < spineCount - 1)
                    spineDir = (spine[s + 1] - spinePoint).normalized;
                else if (s > 0)
                    spineDir = (spinePoint - spine[s - 1]).normalized;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, spineDir);

                for (int c = 0; c < csCount; c++)
                {
                    Vector3 csPoint = new Vector3(
                        crossSection[c].x * scaleVal.x,
                        0,
                        crossSection[c].y * scaleVal.y
                    );
                    csPoint = rotation * csPoint + spinePoint;
                    vertices.Add(csPoint);
                    uvs.Add(new Vector2((float)c / (csCount - 1), (float)s / (spineCount - 1)));
                }
            }

            // Generate side triangles
            for (int s = 0; s < spineCount - 1; s++)
            {
                for (int c = 0; c < csCount - 1; c++)
                {
                    int bl = s * csCount + c;
                    int br = bl + 1;
                    int tl = (s + 1) * csCount + c;
                    int tr = tl + 1;

                    if (ccw)
                    {
                        triangles.Add(bl);
                        triangles.Add(tl);
                        triangles.Add(br);
                        triangles.Add(br);
                        triangles.Add(tl);
                        triangles.Add(tr);
                    }
                    else
                    {
                        triangles.Add(bl);
                        triangles.Add(br);
                        triangles.Add(tl);
                        triangles.Add(br);
                        triangles.Add(tr);
                        triangles.Add(tl);
                    }
                }
            }

            // TODO: Add cap triangles for beginCap and endCap

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        #endregion
    }
}
