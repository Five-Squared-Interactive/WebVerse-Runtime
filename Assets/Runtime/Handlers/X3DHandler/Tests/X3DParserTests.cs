// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine.TestTools;
using X3D;

/// <summary>
/// Pure unit tests for X3DParser. These are synchronous tests that verify XML-to-X3DNode
/// parsing without requiring any Unity scene or runtime setup.
/// </summary>
public class X3DParserTests
{
    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    #region Valid X3D Structures

    [Test]
    public void Parse_MinimalX3DDocument_ReturnsRootNode()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = "<X3D></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        Assert.IsNotNull(root);
        Assert.AreEqual("X3D", root.Name);
        Assert.AreEqual(0, root.Children.Count);
    }

    [Test]
    public void Parse_X3DWithSceneChild_ReturnsSceneAsChild()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene></Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        Assert.IsNotNull(root);
        Assert.AreEqual(1, root.Children.Count);
        Assert.AreEqual("Scene", root.Children[0].Name);
    }

    [Test]
    public void Parse_ShapeWithBoxGeometry_ParsesBoxNode()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape><Box size=""1 1 1""/></Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode scene = root.Children[0];
        X3DNode shape = scene.Children[0];
        Assert.AreEqual("Shape", shape.Name);
        X3DNode box = shape.Children[0];
        Assert.AreEqual("Box", box.Name);
        Assert.AreEqual("1 1 1", box.Attributes["size"]);
    }

    [Test]
    public void Parse_ShapeWithSphereGeometry_ParsesSphereNode()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape><Sphere radius=""0.5""/></Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode sphere = root.Children[0].Children[0].Children[0];
        Assert.AreEqual("Sphere", sphere.Name);
        Assert.AreEqual("0.5", sphere.Attributes["radius"]);
    }

    [Test]
    public void Parse_ShapeWithCylinderGeometry_ParsesCylinderNode()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape><Cylinder radius=""0.3"" height=""2""/></Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode cylinder = root.Children[0].Children[0].Children[0];
        Assert.AreEqual("Cylinder", cylinder.Name);
        Assert.AreEqual("0.3", cylinder.Attributes["radius"]);
        Assert.AreEqual("2", cylinder.Attributes["height"]);
    }

    [Test]
    public void Parse_ShapeWithConeGeometry_ParsesConeNode()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape><Cone bottomRadius=""0.5"" height=""1""/></Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode cone = root.Children[0].Children[0].Children[0];
        Assert.AreEqual("Cone", cone.Name);
        Assert.AreEqual("0.5", cone.Attributes["bottomRadius"]);
        Assert.AreEqual("1", cone.Attributes["height"]);
    }

    [Test]
    public void Parse_ShapeWithIndexedFaceSetGeometry_ParsesIFSNode()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape>
                <IndexedFaceSet coordIndex=""0 1 2 -1 2 3 0 -1"">
                    <Coordinate point=""0 0 0, 1 0 0, 1 1 0, 0 1 0""/>
                </IndexedFaceSet>
            </Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode ifs = root.Children[0].Children[0].Children[0];
        Assert.AreEqual("IndexedFaceSet", ifs.Name);
        Assert.AreEqual("0 1 2 -1 2 3 0 -1", ifs.Attributes["coordIndex"]);
        Assert.AreEqual(1, ifs.Children.Count);
        Assert.AreEqual("Coordinate", ifs.Children[0].Name);
    }

    [Test]
    public void Parse_TransformWithTranslation_ParsesTranslationAttribute()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform translation=""1 2 3""></Transform>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode transform = root.Children[0].Children[0];
        Assert.AreEqual("Transform", transform.Name);
        Assert.AreEqual("1 2 3", transform.Attributes["translation"]);
    }

    [Test]
    public void Parse_TransformWithRotation_ParsesRotationAttribute()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform rotation=""0 1 0 1.57""></Transform>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode transform = root.Children[0].Children[0];
        Assert.AreEqual("0 1 0 1.57", transform.Attributes["rotation"]);
    }

    [Test]
    public void Parse_TransformWithScale_ParsesScaleAttribute()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform scale=""2 2 2""></Transform>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode transform = root.Children[0].Children[0];
        Assert.AreEqual("2 2 2", transform.Attributes["scale"]);
    }

    [Test]
    public void Parse_NestedTransforms_PreservesHierarchy()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform DEF=""Outer"" translation=""1 0 0"">
                <Transform DEF=""Inner"" translation=""0 1 0"">
                    <Shape><Box size=""1 1 1""/></Shape>
                </Transform>
            </Transform>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode outer = root.Children[0].Children[0];
        Assert.AreEqual("Transform", outer.Name);
        Assert.AreEqual("Outer", outer.Attributes["DEF"]);

        X3DNode inner = outer.Children[0];
        Assert.AreEqual("Transform", inner.Name);
        Assert.AreEqual("Inner", inner.Attributes["DEF"]);
        Assert.AreEqual("0 1 0", inner.Attributes["translation"]);

        X3DNode shape = inner.Children[0];
        Assert.AreEqual("Shape", shape.Name);
    }

    [Test]
    public void Parse_AppearanceWithMaterial_ParsesDiffuseAndEmissiveColor()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape>
                <Appearance>
                    <Material diffuseColor=""1 0 0"" emissiveColor=""0.5 0.5 0""/>
                </Appearance>
            </Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode material = root.Children[0].Children[0].Children[0].Children[0];
        Assert.AreEqual("Material", material.Name);
        Assert.AreEqual("1 0 0", material.Attributes["diffuseColor"]);
        Assert.AreEqual("0.5 0.5 0", material.Attributes["emissiveColor"]);
    }

    [Test]
    public void Parse_TimeSensorNode_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <TimeSensor DEF=""Timer"" cycleInterval=""5"" loop=""true""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode timer = root.Children[0].Children[0];
        Assert.AreEqual("TimeSensor", timer.Name);
        Assert.AreEqual("Timer", timer.Attributes["DEF"]);
        Assert.AreEqual("5", timer.Attributes["cycleInterval"]);
        Assert.AreEqual("true", timer.Attributes["loop"]);
    }

    [Test]
    public void Parse_RouteNode_ParsesFromToAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <ROUTE fromNode=""Timer"" fromField=""fraction_changed""
                   toNode=""Interpolator"" toField=""set_fraction""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode route = root.Children[0].Children[0];
        Assert.AreEqual("ROUTE", route.Name);
        Assert.AreEqual("Timer", route.Attributes["fromNode"]);
        Assert.AreEqual("fraction_changed", route.Attributes["fromField"]);
        Assert.AreEqual("Interpolator", route.Attributes["toNode"]);
        Assert.AreEqual("set_fraction", route.Attributes["toField"]);
    }

    [Test]
    public void Parse_ViewpointNode_ParsesPositionOrientationFOV()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Viewpoint position=""0 1.6 5"" orientation=""0 1 0 0"" fieldOfView=""0.785""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode viewpoint = root.Children[0].Children[0];
        Assert.AreEqual("Viewpoint", viewpoint.Name);
        Assert.AreEqual("0 1.6 5", viewpoint.Attributes["position"]);
        Assert.AreEqual("0 1 0 0", viewpoint.Attributes["orientation"]);
        Assert.AreEqual("0.785", viewpoint.Attributes["fieldOfView"]);
    }

    [Test]
    public void Parse_InlineNode_ParsesUrlAttribute()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Inline url=""model.x3d""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode inline = root.Children[0].Children[0];
        Assert.AreEqual("Inline", inline.Name);
        Assert.AreEqual("model.x3d", inline.Attributes["url"]);
    }

    [Test]
    public void Parse_NodeWithDEFAttribute_ParsesDEF()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform DEF=""MyTransform"" translation=""0 0 0""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode transform = root.Children[0].Children[0];
        Assert.IsTrue(transform.Attributes.ContainsKey("DEF"));
        Assert.AreEqual("MyTransform", transform.Attributes["DEF"]);
    }

    [Test]
    public void Parse_NodeWithUSEAttribute_ParsesUSE()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape USE=""MyShape""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode shape = root.Children[0].Children[0];
        Assert.AreEqual("Shape", shape.Name);
        Assert.IsTrue(shape.Attributes.ContainsKey("USE"));
        Assert.AreEqual("MyShape", shape.Attributes["USE"]);
    }

    [Test]
    public void Parse_MultipleChildrenUnderParent_AllChildrenPresent()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform translation=""1 0 0""/>
            <Transform translation=""2 0 0""/>
            <Transform translation=""3 0 0""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode scene = root.Children[0];
        Assert.AreEqual(3, scene.Children.Count);
        Assert.AreEqual("1 0 0", scene.Children[0].Attributes["translation"]);
        Assert.AreEqual("2 0 0", scene.Children[1].Attributes["translation"]);
        Assert.AreEqual("3 0 0", scene.Children[2].Attributes["translation"]);
    }

    [Test]
    public void Parse_AttributeWithMultipleFloatValues_PreservesFullString()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Material diffuseColor=""0.2 0.4 0.6""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode material = root.Children[0].Children[0];
        Assert.AreEqual("0.2 0.4 0.6", material.Attributes["diffuseColor"]);
    }

    [Test]
    public void Parse_FullSceneWithHeadAndMeta_ParsesHeadSection()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
        <X3D profile=""Immersive"" version=""3.3"">
            <head>
                <meta name=""title"" content=""Test Scene""/>
            </head>
            <Scene>
                <Shape><Box size=""1 1 1""/></Shape>
            </Scene>
        </X3D>";

        X3DNode root = X3DParser.Parse(xml);

        Assert.AreEqual("X3D", root.Name);
        Assert.AreEqual("Immersive", root.Attributes["profile"]);
        Assert.AreEqual("3.3", root.Attributes["version"]);

        // head and Scene
        Assert.AreEqual(2, root.Children.Count);
        Assert.AreEqual("head", root.Children[0].Name);
        Assert.AreEqual("Scene", root.Children[1].Name);

        // meta inside head
        X3DNode meta = root.Children[0].Children[0];
        Assert.AreEqual("meta", meta.Name);
        Assert.AreEqual("title", meta.Attributes["name"]);
        Assert.AreEqual("Test Scene", meta.Attributes["content"]);
    }

    [Test]
    public void Parse_CommentsInDocument_CapturedInCommentsList()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><!-- This is a comment --><Scene/></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        Assert.IsNotNull(root);
        Assert.AreEqual(1, root.Comments.Count);
        Assert.AreEqual(" This is a comment ", root.Comments[0]);
    }

    [Test]
    public void Parse_TextContent_CapturedInInnerText()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene><Script>function initialize() {}</Script></Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode script = root.Children[0].Children[0];
        Assert.AreEqual("Script", script.Name);
        Assert.AreEqual("function initialize() {}", script.InnerText);
    }

    [Test]
    public void Parse_SelfClosingElements_ParsedCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Background skyColor=""0.2 0.4 0.6""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode bg = root.Children[0].Children[0];
        Assert.AreEqual("Background", bg.Name);
        Assert.AreEqual("0.2 0.4 0.6", bg.Attributes["skyColor"]);
        Assert.AreEqual(0, bg.Children.Count);
    }

    [Test]
    public void Parse_DirectionalLightNode_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <DirectionalLight direction=""0 -1 -1"" intensity=""0.8"" color=""1 1 1"" ambientIntensity=""0.2""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode light = root.Children[0].Children[0];
        Assert.AreEqual("DirectionalLight", light.Name);
        Assert.AreEqual("0 -1 -1", light.Attributes["direction"]);
        Assert.AreEqual("0.8", light.Attributes["intensity"]);
        Assert.AreEqual("1 1 1", light.Attributes["color"]);
        Assert.AreEqual("0.2", light.Attributes["ambientIntensity"]);
    }

    [Test]
    public void Parse_PointLightNode_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <PointLight location=""0 2 0"" intensity=""1"" color=""1 0.9 0.8"" radius=""10""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode light = root.Children[0].Children[0];
        Assert.AreEqual("PointLight", light.Name);
        Assert.AreEqual("0 2 0", light.Attributes["location"]);
        Assert.AreEqual("1", light.Attributes["intensity"]);
        Assert.AreEqual("10", light.Attributes["radius"]);
    }

    [Test]
    public void Parse_SpotLightNode_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <SpotLight location=""0 3 0"" direction=""0 -1 0"" cutOffAngle=""0.785"" beamWidth=""0.6"" intensity=""1""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode light = root.Children[0].Children[0];
        Assert.AreEqual("SpotLight", light.Name);
        Assert.AreEqual("0 3 0", light.Attributes["location"]);
        Assert.AreEqual("0 -1 0", light.Attributes["direction"]);
        Assert.AreEqual("0.785", light.Attributes["cutOffAngle"]);
        Assert.AreEqual("0.6", light.Attributes["beamWidth"]);
    }

    [Test]
    public void Parse_BackgroundNode_ParsesMultipleColors()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Background skyColor=""0.2 0.4 0.6, 0.1 0.2 0.3"" groundColor=""0.3 0.2 0.1""
                         skyAngle=""1.57"" groundAngle=""1.57""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode bg = root.Children[0].Children[0];
        Assert.AreEqual("Background", bg.Name);
        Assert.AreEqual("0.2 0.4 0.6, 0.1 0.2 0.3", bg.Attributes["skyColor"]);
        Assert.AreEqual("0.3 0.2 0.1", bg.Attributes["groundColor"]);
        Assert.AreEqual("1.57", bg.Attributes["skyAngle"]);
    }

    [Test]
    public void Parse_FogNode_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Fog color=""0.8 0.8 0.8"" visibilityRange=""100"" fogType=""LINEAR""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode fog = root.Children[0].Children[0];
        Assert.AreEqual("Fog", fog.Name);
        Assert.AreEqual("0.8 0.8 0.8", fog.Attributes["color"]);
        Assert.AreEqual("100", fog.Attributes["visibilityRange"]);
        Assert.AreEqual("LINEAR", fog.Attributes["fogType"]);
    }

    [Test]
    public void Parse_NavigationInfoNode_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <NavigationInfo type='""WALK"" ""ANY""' speed=""1.5"" headlight=""true""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode nav = root.Children[0].Children[0];
        Assert.AreEqual("NavigationInfo", nav.Name);
        Assert.AreEqual("1.5", nav.Attributes["speed"]);
        Assert.AreEqual("true", nav.Attributes["headlight"]);
    }

    [Test]
    public void Parse_ImageTextureNode_ParsesUrl()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape>
                <Appearance>
                    <ImageTexture url=""texture.png"" repeatS=""true"" repeatT=""false""/>
                </Appearance>
            </Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode texture = root.Children[0].Children[0].Children[0].Children[0];
        Assert.AreEqual("ImageTexture", texture.Name);
        Assert.AreEqual("texture.png", texture.Attributes["url"]);
        Assert.AreEqual("true", texture.Attributes["repeatS"]);
        Assert.AreEqual("false", texture.Attributes["repeatT"]);
    }

    [Test]
    public void Parse_PositionInterpolator_ParsesKeysAndValues()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <PositionInterpolator DEF=""PosInterp"" key=""0 0.5 1"" keyValue=""0 0 0, 1 2 0, 2 0 0""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode interp = root.Children[0].Children[0];
        Assert.AreEqual("PositionInterpolator", interp.Name);
        Assert.AreEqual("PosInterp", interp.Attributes["DEF"]);
        Assert.AreEqual("0 0.5 1", interp.Attributes["key"]);
        Assert.AreEqual("0 0 0, 1 2 0, 2 0 0", interp.Attributes["keyValue"]);
    }

    [Test]
    public void Parse_OrientationInterpolator_ParsesKeysAndValues()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <OrientationInterpolator DEF=""RotInterp"" key=""0 1"" keyValue=""0 1 0 0, 0 1 0 3.14""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode interp = root.Children[0].Children[0];
        Assert.AreEqual("OrientationInterpolator", interp.Name);
        Assert.AreEqual("0 1", interp.Attributes["key"]);
        Assert.AreEqual("0 1 0 0, 0 1 0 3.14", interp.Attributes["keyValue"]);
    }

    [Test]
    public void Parse_ColorInterpolator_ParsesKeysAndValues()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <ColorInterpolator DEF=""ColorInterp"" key=""0 0.5 1"" keyValue=""1 0 0, 0 1 0, 0 0 1""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode interp = root.Children[0].Children[0];
        Assert.AreEqual("ColorInterpolator", interp.Name);
        Assert.AreEqual("0 0.5 1", interp.Attributes["key"]);
    }

    [Test]
    public void Parse_GroupNode_ParsesChildren()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Group DEF=""MyGroup"">
                <Shape><Box size=""1 1 1""/></Shape>
                <Shape><Sphere radius=""0.5""/></Shape>
            </Group>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode group = root.Children[0].Children[0];
        Assert.AreEqual("Group", group.Name);
        Assert.AreEqual("MyGroup", group.Attributes["DEF"]);
        Assert.AreEqual(2, group.Children.Count);
        Assert.AreEqual("Shape", group.Children[0].Name);
        Assert.AreEqual("Shape", group.Children[1].Name);
    }

    [Test]
    public void Parse_SwitchNode_ParsesWhichChoice()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Switch whichChoice=""0"">
                <Shape><Box size=""1 1 1""/></Shape>
                <Shape><Sphere radius=""0.5""/></Shape>
            </Switch>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode sw = root.Children[0].Children[0];
        Assert.AreEqual("Switch", sw.Name);
        Assert.AreEqual("0", sw.Attributes["whichChoice"]);
        Assert.AreEqual(2, sw.Children.Count);
    }

    [Test]
    public void Parse_LODNode_ParsesRangeAttribute()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <LOD range=""25 50"">
                <Shape><Box size=""1 1 1""/></Shape>
                <Shape><Box size=""2 2 2""/></Shape>
            </LOD>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode lod = root.Children[0].Children[0];
        Assert.AreEqual("LOD", lod.Name);
        Assert.AreEqual("25 50", lod.Attributes["range"]);
        Assert.AreEqual(2, lod.Children.Count);
    }

    [Test]
    public void Parse_AnchorNode_ParsesUrlAndDescription()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Anchor url=""scene2.x3d"" description=""Go to scene 2"">
                <Shape><Box size=""1 1 1""/></Shape>
            </Anchor>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode anchor = root.Children[0].Children[0];
        Assert.AreEqual("Anchor", anchor.Name);
        Assert.AreEqual("scene2.x3d", anchor.Attributes["url"]);
        Assert.AreEqual("Go to scene 2", anchor.Attributes["description"]);
        Assert.AreEqual(1, anchor.Children.Count);
    }

    [Test]
    public void Parse_SoundAndAudioClip_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Sound location=""0 1 0"" maxBack=""10"" maxFront=""10"">
                <AudioClip url=""sound.mp3"" loop=""true"" pitch=""1.0""/>
            </Sound>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode sound = root.Children[0].Children[0];
        Assert.AreEqual("Sound", sound.Name);
        Assert.AreEqual("0 1 0", sound.Attributes["location"]);

        X3DNode clip = sound.Children[0];
        Assert.AreEqual("AudioClip", clip.Name);
        Assert.AreEqual("sound.mp3", clip.Attributes["url"]);
        Assert.AreEqual("true", clip.Attributes["loop"]);
    }

    [Test]
    public void Parse_TouchSensor_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Group>
                <TouchSensor DEF=""Touch"" description=""Click me""/>
                <Shape><Box size=""1 1 1""/></Shape>
            </Group>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode sensor = root.Children[0].Children[0].Children[0];
        Assert.AreEqual("TouchSensor", sensor.Name);
        Assert.AreEqual("Touch", sensor.Attributes["DEF"]);
        Assert.AreEqual("Click me", sensor.Attributes["description"]);
    }

    [Test]
    public void Parse_DeeplyNestedHierarchy_PreservesAllLevels()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Transform DEF=""L1"">
                <Transform DEF=""L2"">
                    <Transform DEF=""L3"">
                        <Transform DEF=""L4"">
                            <Shape><Box size=""1 1 1""/></Shape>
                        </Transform>
                    </Transform>
                </Transform>
            </Transform>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode l1 = root.Children[0].Children[0];
        Assert.AreEqual("L1", l1.Attributes["DEF"]);
        X3DNode l2 = l1.Children[0];
        Assert.AreEqual("L2", l2.Attributes["DEF"]);
        X3DNode l3 = l2.Children[0];
        Assert.AreEqual("L3", l3.Attributes["DEF"]);
        X3DNode l4 = l3.Children[0];
        Assert.AreEqual("L4", l4.Attributes["DEF"]);
        Assert.AreEqual("Shape", l4.Children[0].Name);
    }

    [Test]
    public void Parse_MultipleComments_AllCaptured()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><!-- First --><!-- Second --><Scene><!-- Third --></Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        Assert.AreEqual(2, root.Comments.Count);
        Assert.AreEqual(" First ", root.Comments[0]);
        Assert.AreEqual(" Second ", root.Comments[1]);

        X3DNode scene = root.Children[0];
        Assert.AreEqual(1, scene.Comments.Count);
        Assert.AreEqual(" Third ", scene.Comments[0]);
    }

    [Test]
    public void Parse_CDATAInScript_CapturedAsInnerText()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Script><![CDATA[function init() { return 42; }]]></Script>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode script = root.Children[0].Children[0];
        Assert.AreEqual("Script", script.Name);
        Assert.AreEqual("function init() { return 42; }", script.InnerText);
    }

    [Test]
    public void Parse_MaterialWithAllProperties_ParsesAllAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <Shape>
                <Appearance>
                    <Material diffuseColor=""1 0 0"" specularColor=""1 1 1""
                              emissiveColor=""0.1 0.1 0.1"" shininess=""0.8""
                              transparency=""0.5"" ambientIntensity=""0.3""/>
                </Appearance>
            </Shape>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode material = root.Children[0].Children[0].Children[0].Children[0];
        Assert.AreEqual("Material", material.Name);
        Assert.AreEqual("1 0 0", material.Attributes["diffuseColor"]);
        Assert.AreEqual("1 1 1", material.Attributes["specularColor"]);
        Assert.AreEqual("0.1 0.1 0.1", material.Attributes["emissiveColor"]);
        Assert.AreEqual("0.8", material.Attributes["shininess"]);
        Assert.AreEqual("0.5", material.Attributes["transparency"]);
        Assert.AreEqual("0.3", material.Attributes["ambientIntensity"]);
    }

    [Test]
    public void Parse_MultipleROUTES_AllParsed()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <TimeSensor DEF=""Timer"" cycleInterval=""5"" loop=""true""/>
            <PositionInterpolator DEF=""PosInterp"" key=""0 1"" keyValue=""0 0 0, 5 0 0""/>
            <ROUTE fromNode=""Timer"" fromField=""fraction_changed"" toNode=""PosInterp"" toField=""set_fraction""/>
            <ROUTE fromNode=""PosInterp"" fromField=""value_changed"" toNode=""Ball"" toField=""set_translation""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode scene = root.Children[0];
        Assert.AreEqual(4, scene.Children.Count);
        Assert.AreEqual("TimeSensor", scene.Children[0].Name);
        Assert.AreEqual("PositionInterpolator", scene.Children[1].Name);
        Assert.AreEqual("ROUTE", scene.Children[2].Name);
        Assert.AreEqual("ROUTE", scene.Children[3].Name);
        Assert.AreEqual("PosInterp", scene.Children[3].Attributes["fromNode"]);
        Assert.AreEqual("Ball", scene.Children[3].Attributes["toNode"]);
    }

    [Test]
    public void Parse_GeoElevationGrid_ParsesAttributes()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<X3D><Scene>
            <GeoElevationGrid xDimension=""3"" zDimension=""3"" xSpacing=""1.0"" zSpacing=""1.0""
                              height=""0 1 0 1 2 1 0 1 0""/>
        </Scene></X3D>";

        X3DNode root = X3DParser.Parse(xml);

        X3DNode grid = root.Children[0].Children[0];
        Assert.AreEqual("GeoElevationGrid", grid.Name);
        Assert.AreEqual("3", grid.Attributes["xDimension"]);
        Assert.AreEqual("3", grid.Attributes["zDimension"]);
        Assert.AreEqual("0 1 0 1 2 1 0 1 0", grid.Attributes["height"]);
    }

    [Test]
    public void Parse_WhitespaceOnlyContent_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        X3DNode result = X3DParser.Parse("   \n\t  ");

        Assert.IsNull(result);
    }

    #endregion

    #region Error Handling

    [Test]
    public void Parse_NullString_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        X3DNode result = X3DParser.Parse(null);

        Assert.IsNull(result);
    }

    [Test]
    public void Parse_EmptyString_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        X3DNode result = X3DParser.Parse("");

        Assert.IsNull(result);
    }

    [Test]
    public void Parse_InvalidXml_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        X3DNode result = X3DParser.Parse("<invalid><not closed");

        Assert.IsNull(result);
    }

    [Test]
    public void Parse_ValidXmlButNotX3D_ReturnsNodeWithNonX3DName()
    {
        LogAssert.ignoreFailingMessages = true;
        string xml = @"<html><body><p>Not X3D</p></body></html>";

        X3DNode result = X3DParser.Parse(xml);

        // Parser does not validate root tag name, so it returns a node
        Assert.IsNotNull(result);
        Assert.AreEqual("html", result.Name);
    }

    [Test]
    public void ParseFromFile_NonExistentPath_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        X3DNode result = X3DParser.ParseFromFile("/nonexistent/path/file.x3d");

        Assert.IsNull(result);
    }

    #endregion
}
