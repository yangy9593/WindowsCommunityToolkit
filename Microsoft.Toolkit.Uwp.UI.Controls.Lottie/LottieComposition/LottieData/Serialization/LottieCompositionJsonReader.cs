// If defined, an issue will be reported for each field that is discovered
// but not parsed. This is used to help test that parsing is complete.
#define CheckForUnparsedFields

using LottieData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Data.Json;
#if CheckForUnparsedFields
using JsonObject = LottieData.Serialization.CheckedJsonObject;
using JsonArray = LottieData.Serialization.CheckedJsonArray;
#endif

namespace LottieData.Serialization
{
    // See: https://github.com/airbnb/lottie-web/tree/master/docs/json for the (sometimes out-of-date)
    // Lottie schema.
#if PUBLIC
    public
#endif
    sealed class LottieCompositionJsonReader
    {
        static readonly AnimatableFloatParser s_animatableFloatParser = new AnimatableFloatParser();
        static readonly AnimatableVector3Parser s_animatableVector3Parser = new AnimatableVector3Parser();
        static readonly AnimatableColorParser s_animatableColorParser = new AnimatableColorParser();
        static readonly AnimatableGeometryParser s_animatableGeometryParser = new AnimatableGeometryParser();

        readonly HashSet<string> m_issues = new HashSet<string>();

        Options _options;

        /// <summary>
        /// Specifies optional behavior for the reader.
        /// </summary>
        public enum Options
        {
            None = 0,
            /// <summary>
            /// Do not read the Name values.
            /// </summary>
            IgnoreNames,
            /// <summary>
            /// Do not read the Match Name values.
            /// </summary>
            IgnoreMatchNames,
        }

        /// <summary>
        /// Parses a Json string to create a <see cref="LottieData.LottieComposition"/>.
        /// </summary>
        public static LottieComposition ReadLottieCompositionFromJsonString(string json, Options options, out string[] issues)
        {
            JsonObject obj;
            try
            {
                obj = JsonObject.Parse(json);
            }
            catch (Exception e)
            {
                issues = new[] { $"Failed to parse JSON. {e.Message}" };
                return null;
            }

            return ReadLottieCompositionFromJson(obj, options, out issues);
        }

        LottieCompositionJsonReader(Options options) { _options = options; }

        static LottieComposition ReadLottieCompositionFromJson(JsonObject obj, Options options, out string[] issues)
        {
            var reader = new LottieCompositionJsonReader(options);
            LottieComposition result = null;
            try
            {
                result = reader.ReadLottieComposition(obj);
            }
            catch (LottieJsonReaderException e)
            {
                reader.ReportIssue(e.Message);
            }
            issues = reader.m_issues.ToArray();
            return result;
        }

        LottieComposition ReadLottieComposition(JsonObject obj)
        {
            int? width = null;
            int? height = null;
            int? inPoint = null;
            int? outPoint = null;
            double? frameRate = null;
            bool is3d = false;
            AssetCollection assets = null;
            LayerCollection layers = null;
            IEnumerable<Marker> markers = null;
            string version = null;
            string name = null;

            int? GetInt(IJsonValue j) => j.ValueType == JsonValueType.Number ? (int?)j.GetNumber() : null;

            foreach (var field in obj)
            {
                switch (field.Key)
                {
                    case "ddd":
                        is3d = field.Value.GetNumber() == 1;
                        break;
                    case "ip":
                        inPoint = GetInt(field.Value);
                        break;
                    case "op":
                        outPoint = GetInt(field.Value);
                        break;
                    case "h":
                        height = GetInt(field.Value);
                        break;
                    case "w":
                        width = GetInt(field.Value);
                        break;
                    case "fr":
                        frameRate = field.Value.ValueType == JsonValueType.Number ? (double?)field.Value.GetNumber() : null;
                        break;
                    case "v":
                        version = field.Value.ValueType == JsonValueType.String ? field.Value.GetString() : null;
                        break;
                    case "assets":
                        assets = field.Value.ValueType == JsonValueType.Array
                            ? new AssetCollection(field.Value.GetArray().Select(a => ReadAsset(a.GetObject())).Where(a => a != null))
                            : null;
                        break;
                    case "markers":
                        markers = field.Value.ValueType == JsonValueType.Array
                            ? field.Value.GetArray().Select(a => ReadMarker(a.GetObject())).ToArray()
                            : null;
                        break;
                    case "layers":
                        layers = field.Value.ValueType == JsonValueType.Array
                            ? new LayerCollection(field.Value.GetArray().Select(a => ReadLayer(a.GetObject())).Where(a => a != null))
                            : null;
                        break;
                    case "nm":
                        name = field.Value.GetString();
                        break;
                    case "chars":
                        ReportIssue("chars: is not supported.");
                        break;
                    case "fonts":
                        ReportIssue("fonts: is not supported.");
                        break;
                    default:
                        throw new LottieJsonReaderException($"Unexpected field: {field.Key}");
                }
            }

            if (version == null)
            {
                throw new LottieJsonReaderException("Version parameter not found.");
            }

            if (!width.HasValue)
            {
                throw new LottieJsonReaderException("Width parameter not found.");
            }

            if (!height.HasValue)
            {
                throw new LottieJsonReaderException("Height parameter not found.");
            }

            if (!inPoint.HasValue)
            {
                throw new LottieJsonReaderException("Start frame parameter not found.");
            }

            if (!outPoint.HasValue)
            {
                throw new LottieJsonReaderException("End frame parameter not found.");
            }

            var versions = version.Split('.');

            if (layers == null)
            {
                throw new LottieJsonReaderException("No layers found.");
            }

            var lottieComposition = new LottieComposition(
                name,
                width.Value,
                height.Value,
                inPoint.Value,
                outPoint.Value,
                frameRate.Value,
                is3d,
                new Version(int.Parse(versions[0]), int.Parse(versions[1]), int.Parse(versions[2])),
                assets,
                layers,
                markers ?? new Marker[0]);

            return lottieComposition;
        }

        Marker ReadMarker(JsonObject obj)
        {
            var tm = obj.GetNamedNumber("tm");
            var cm = obj.GetNamedString("cm");
            var dr = obj.GetNamedNumber("dr");
            AssertAllFieldsRead(obj);
            return new Marker(tm, cm, dr);
        }

        Asset ReadAsset(JsonObject obj)
        {
            // Older Lottie's use a string for the id. Newer Lotties use a number.
            // Convert either to a string.
            var idObj = obj.GetNamedValue("id");
            var id = idObj.ValueType == JsonValueType.Number 
                ? idObj.GetNumber().ToString() 
                : idObj.GetString();

            // Try to parse as a layers asset.
            var layersArray = obj.GetNamedArray("layers", null);
            if (layersArray != null)
            {
                var layers = from val in layersArray
                             let layer = ReadLayer(val.GetObject())
                             where layer != null
                             select layer;

                AssertAllFieldsRead(obj);
                return new LayerCollectionAsset(id, new LayerCollection(layers));
            }
            else
            {
                // Try to parse as an image asset.
                var w = obj.GetNamedNumber("w", double.NaN);
                var h = obj.GetNamedNumber("h", double.NaN);
                var u = obj.GetNamedString("u");
                var p = obj.GetNamedString("p");

                if (double.IsNaN(w) || double.IsNaN(h) || u == null || p == null)
                {
                    ReportIssue("Unsupported asset type.");
                    AssertAllFieldsRead(obj);
                    return null;
                }

                return new ImageAsset(id, w, h, u, p);
            }
        }

        // May return null if there was a problem reading the layer.
        Layer ReadLayer(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "bounds");
            IgnoreFieldThatIsNotYetSupported(obj, "masksProperties");
            IgnoreFieldThatIsNotYetSupported(obj, "sy");
            IgnoreFieldThatIsNotYetSupported(obj, "t");
            IgnoreFieldThatIsNotYetSupported(obj, "td");

            var name = obj.GetNamedString("nm");
            var layerIndex = ReadInt(obj, "ind").Value;
            var parentIndex = ReadInt(obj, "parent");
            var is3d = ReadBool(obj, "ddd") == true;
            var autoOrient = ReadBool(obj, "ao") == true;
            var blendMode = BmToBlendMode(obj.GetNamedNumber("bm", 0));
            var isHidden = ReadBool(obj, "hd") == true;
            var render = ReadBool(obj, "render") != false;

            if (!render)
            {
                ReportIssue("Layer with render:false");
                return null;
            }
            
            // Warnings
            if (name.EndsWith(".ai") || obj.GetNamedString("cl", "") == "ai")
            {
                ReportIssue("Illustrator layers must be converted to shape layers.");
            }

            if (obj.ContainsKey("ef"))
            {
                ReportIssue("Layer effects are not supported. If you are using them for " +
                                "fills, strokes, trim paths etc. then try adding them directly as contents " +
                                "in your shape layer.");
            }

            if (obj.ContainsKey("tt"))
            {
                ReportIssue("Mattes are not supported.");
            }

            if (obj.ContainsKey("maskProperties") || obj.ContainsKey("hasMask"))
            {
                ReportIssue("Masks are not supported.");
            }

            // ----------------------
            // Layer Transform
            // ----------------------

            var transform = ReadTransform(obj.GetNamedObject("ks"));

            // ------------------------------
            // Layer Animation
            // ------------------------------
            var timeStretch = obj.GetNamedNumber("sr", 1.0);
            // Time when the layer starts
            var startFrame = obj.GetNamedNumber("st");

            // Time when the layer becomes visible.
            var inFrame = obj.GetNamedNumber("ip");
            var outFrame = obj.GetNamedNumber("op");


            switch (TyToLayerType(obj.GetNamedNumber("ty", double.NaN)))
            {
                case Layer.LayerType.PreComp:
                    {
                        var refId = obj.GetNamedString("refId", "");
                        var width = obj.GetNamedNumber("w");
                        var height = obj.GetNamedNumber("h");
                        var tm = obj.GetNamedObject("tm", null);
                        if (tm != null)
                        {
                            ReportIssue("Time remapping of PreComps is not supported.");
                        }

                        AssertAllFieldsRead(obj);
                        return new PreCompLayer(
                            name,
                            layerIndex,
                            parentIndex,
                            isHidden,
                            transform,
                            timeStretch,
                            startFrame,
                            inFrame,
                            outFrame,
                            blendMode,
                            is3d,
                            autoOrient,
                            refId,
                            width,
                            height);
                    }
                case Layer.LayerType.Solid:
                    {
                        var solidWidth = ReadInt(obj, "sw").Value;
                        var solidHeight = ReadInt(obj, "sh").Value;
                        var solidColor = GetSolidColorFromString(obj.GetNamedString("sc"));
                        AssertAllFieldsRead(obj);
                        return new SolidLayer(
                            name,
                            layerIndex,
                            parentIndex,
                            isHidden,
                            transform,
                            solidWidth,
                            solidHeight,
                            solidColor,
                            timeStretch,
                            startFrame,
                            inFrame,
                            outFrame,
                            blendMode,
                            is3d,
                            autoOrient);
                    }
                case Layer.LayerType.Image:
                    {
                        var refId = obj.GetNamedString("refId", "");

                        AssertAllFieldsRead(obj);
                        return new ImageLayer(
                            name,
                            layerIndex,
                            parentIndex,
                            isHidden,
                            transform,
                            timeStretch,
                            startFrame,
                            inFrame,
                            outFrame,
                            blendMode,
                            is3d,
                            autoOrient,
                            refId);
                    }
                case Layer.LayerType.Null:
                    {
                        AssertAllFieldsRead(obj);

                        return new NullLayer(
                            name,
                            layerIndex,
                            parentIndex,
                            isHidden,
                            transform,
                            timeStretch,
                            startFrame,
                            inFrame,
                            outFrame,
                            blendMode,
                            is3d,
                            autoOrient);
                    }
                case Layer.LayerType.Shape:
                    {
                        var shapes = new List<ShapeLayerContent>();
                        var shapesJson = obj.GetNamedArray("shapes", null);
                        shapes.Capacity = shapesJson.Count;
                        if (shapesJson != null)
                        {
                            var shapesJsonCount = shapesJson.Count;
                            for (var i = 0; i < shapesJsonCount; i++)
                            {
                                var item = ReadShapeContent(shapesJson.GetObjectAt((uint)i));
                                if (item != null)
                                {
                                    shapes.Add(item);
                                }
                            }
                        }

                        AssertAllFieldsRead(obj);
                        return new ShapeLayer(
                            name,
                            shapes,
                            layerIndex,
                            parentIndex,
                            isHidden,
                            transform,
                            timeStretch,
                            startFrame,
                            inFrame,
                            outFrame,
                            blendMode,
                            is3d,
                            autoOrient);
                    }
                case Layer.LayerType.Text:
                    {
                        var refId = obj.GetNamedString("refId", "");

                        AssertAllFieldsRead(obj);
                        return new TextLayer(
                            name,
                            layerIndex,
                            parentIndex,
                            isHidden,
                            transform,
                            timeStretch,
                            startFrame,
                            inFrame,
                            outFrame,
                            blendMode,
                            is3d,
                            autoOrient,
                            refId);
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        static Color GetSolidColorFromString(string hex)
        {
            var index = 1; // Skip '#'
                           // '#AARRGGBB'
            byte a = 255;
            if (hex.Length == 9)
            {
                a = Convert.ToByte(hex.Substring(index, 2), 16);
                index += 2;
            }
            var r = Convert.ToByte(hex.Substring(index, 2), 16);
            index += 2;
            var g = Convert.ToByte(hex.Substring(index, 2), 16);
            index += 2;
            var b = Convert.ToByte(hex.Substring(index, 2), 16);
            return Color.FromArgb(a / 255.0, r / 255.0, g / 255.0, b / 255.0);
        }

        ShapeLayerContent ReadShapeContent(JsonObject obj)
        {
            var type = obj.GetNamedString("ty");

            switch (type)
            {
                case "gr":
                    return ReadShapeGroup(obj);
                case "st":
                    return ReadSolidColorStroke(obj);
                case "gs":
                    return ReadGradientStroke(obj);
                case "fl":
                    return ReadSolidColorFill(obj);
                case "gf":
                    return ReadGradientFill(obj);
                case "tr":
                    return ReadTransform(obj);
                case "el":
                    return ReadEllipse(obj);
                case "sr":
                    return ReadPolystar(obj);
                case "rc":
                    return ReadRectangle(obj);
                case "sh":
                    return ReadShape(obj);
                case "tm":
                    return ReadTrimPath(obj);
                case "mm":
                    return ReadMergePaths(obj);
                case "rd":
                    return ReadRoundedCorner(obj);
                case "rp":
                    return ReadReapeter(obj);
                default:
                    break;
            }
            ReportIssue($"Unexpected shape content type: {type}");
            return null;
        }

        ShapeGroup ReadShapeGroup(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "cix");
            IgnoreFieldThatIsNotYetSupported(obj, "cl");
            IgnoreFieldThatIsNotYetSupported(obj, "ix");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var numberOfProperties = ReadInt(obj, "np");
            var itemsJson = obj.GetNamedArray("it");
            var items = new List<ShapeLayerContent>();

            var itemsJsonCount = itemsJson.Count;
            for (var i = 0; i < itemsJsonCount; i++)
            {
                var item = ReadShapeContent(itemsJson.GetObjectAt((uint)i));
                if (item != null)
                {
                    items.Add(item);
                }
            }
            AssertAllFieldsRead(obj);
            return new ShapeGroup(name.Name, name.MatchName, items);
        }

        // "st"
        SolidColorStroke ReadSolidColorStroke(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "fillEnabled");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var color = ReadColor(obj);
            var opacityPercent = ReadOpacityPercent(obj);
            var strokeWidth = ReadAnimatableFloat(obj.GetNamedObject("w"));
            var capType = LcToLineCapType(obj.GetNamedNumber("lc"));
            var joinType = LjToLineJoinType(obj.GetNamedNumber("lj"));
            var miterLimit = obj.GetNamedNumber("ml", 4); // Default miter limit in AfterEffects is 4

            // Get dash pattern to be set as StrokeDashArray
            Animatable<double> offset = null;
            var dashPattern = new List<double>();
            var dashesJson = obj.GetNamedArray("d", null);
            if (dashesJson != null)
            {
                for (uint i = 0; i < dashesJson.Count; i++)
                {
                    var dashObj = dashesJson.GetObjectAt(i);

                    switch (dashObj.GetNamedString("n"))
                    {
                        case "o":
                            offset = ReadAnimatableFloat(dashObj.GetNamedObject("v"));
                            break;
                        case "d":
                        case "g":
                            dashPattern.Add(ReadAnimatableFloat(dashObj.GetNamedObject("v")).InitialValue);
                            break;
                    }
                }
            }

            AssertAllFieldsRead(obj);
            return new SolidColorStroke(
                name.Name,
                name.MatchName,
                offset ?? new Animatable<double>(0, null),
                dashPattern,
                color,
                opacityPercent,
                strokeWidth,
                capType,
                joinType,
                miterLimit);
        }

        // gs
        ShapeLayerContent ReadGradientStroke(JsonObject obj)
        {
            switch (TToGradientType(obj.GetNamedNumber("t")))
            {
                case GradientType.Linear:
                    return ReadLinearGradientStroke(obj);
                case GradientType.Radial:
                    return ReadRadialGradientStroke(obj);
                default:
                    throw new InvalidOperationException();
            }
        }

        LinearGradientStroke ReadLinearGradientStroke(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "hd");
            IgnoreFieldThatIsNotYetSupported(obj, "g");
            IgnoreFieldThatIsNotYetSupported(obj, "t");
            // highlightLength - ReadAnimatableFloat(obj.GetNamedObject("h")) - but is optional
            IgnoreFieldThatIsNotYetSupported(obj, "h");
            // highlightAngle - ReadAnimatableFloat(obj.GetNamedObject("a")) - but is optional
            IgnoreFieldThatIsNotYetSupported(obj, "1");

            var name = ReadName(obj);
            var opacityPercent = ReadOpacityPercent(obj);
            var strokeWidth = ReadAnimatableFloat(obj.GetNamedObject("w"));
            var capType = LcToLineCapType(obj.GetNamedNumber("lc"));
            var joinType = LjToLineJoinType(obj.GetNamedNumber("lj"));
            var miterLimit = obj.GetNamedNumber("ml", 4); // Default miter limit in AfterEffects is 4
            var startPoint = ReadAnimatableVector3(obj.GetNamedObject("s"));
            var endPoint = ReadAnimatableVector3(obj.GetNamedObject("e"));

            ReportIssue("Gradients are not supported.");

            AssertAllFieldsRead(obj);
            return new LinearGradientStroke(
                name.Name,
                name.MatchName,
                opacityPercent,
                strokeWidth,
                capType,
                joinType,
                miterLimit);
        }

        RadialGradientStroke ReadRadialGradientStroke(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "t");
            // highlightLength - ReadAnimatableFloat(obj.GetNamedObject("h")) - but is optional
            IgnoreFieldThatIsNotYetSupported(obj, "h");
            // highlightAngle - ReadAnimatableFloat(obj.GetNamedObject("a")) - but is optional
            IgnoreFieldThatIsNotYetSupported(obj, "1");

            var name = ReadName(obj);
            var opacityPercent = ReadOpacityPercent(obj);
            var strokeWidth = ReadAnimatableFloat(obj.GetNamedObject("w"));
            var capType = LcToLineCapType(obj.GetNamedNumber("lc"));
            var joinType = LjToLineJoinType(obj.GetNamedNumber("lj"));
            var miterLimit = obj.GetNamedNumber("ml", 4); // Default miter limit in AfterEffects is 4
            var startPoint = ReadAnimatableVector3(obj.GetNamedObject("s"));
            var endPoint = ReadAnimatableVector3(obj.GetNamedObject("e"));

            ReportIssue("Gradients are not supported.");

            AssertAllFieldsRead(obj);
            return new RadialGradientStroke(
                name.Name,
                name.MatchName,
                opacityPercent,
                strokeWidth,
                capType,
                joinType,
                miterLimit);
        }

        // "fl"
        SolidColorFill ReadSolidColorFill(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "fillEnabled");
            IgnoreFieldThatIsNotYetSupported(obj, "cl");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var color = ReadColor(obj);
            var opacityPercent = ReadOpacityPercent(obj);
            var isWindingFill = ReadBool(obj, "r") == true;
            var fillType = isWindingFill ? SolidColorFill.PathFillType.Winding : SolidColorFill.PathFillType.EvenOdd;
            AssertAllFieldsRead(obj);
            return new SolidColorFill(name.Name, name.MatchName, fillType, color, opacityPercent);
        }

        // gf
        ShapeLayerContent ReadGradientFill(JsonObject obj)
        {
            switch (TToGradientType(obj.GetNamedNumber("t")))
            {
                case GradientType.Linear:
                    return ReadLinearGradientFill(obj);
                case GradientType.Radial:
                    return ReadRadialGradientFill(obj);
                default:
                    throw new InvalidOperationException();
            }
        }

        RadialGradientFill ReadRadialGradientFill(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "a");
            IgnoreFieldThatIsNotYetSupported(obj, "g");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");
            IgnoreFieldThatIsNotYetSupported(obj, "r");
            // highlightLength - ReadAnimatableFloat(obj.GetNamedObject("h")) - but is optional
            IgnoreFieldThatIsNotYetSupported(obj, "h");
            // highlightAngle - ReadAnimatableFloat(obj.GetNamedObject("a")) - but is optional
            IgnoreFieldThatIsNotYetSupported(obj, "1");

            var name = ReadName(obj);
            var opacityPercent = ReadOpacityPercent(obj);
            var startPoint = ReadAnimatableVector3(obj.GetNamedObject("s"));
            var endPoint = ReadAnimatableVector3(obj.GetNamedObject("e"));
            AssertAllFieldsRead(obj);
            return new RadialGradientFill(name.Name, name.MatchName, opacityPercent, startPoint, endPoint, null, null);
        }

        LinearGradientFill ReadLinearGradientFill(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "g");
            IgnoreFieldThatIsNotYetSupported(obj, "r");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var opacity = ReadOpacityPercent(obj);
            var startPoint = ReadAnimatableVector3(obj.GetNamedObject("s"));
            var endPoint = ReadAnimatableVector3(obj.GetNamedObject("e"));
            AssertAllFieldsRead(obj);
            return new LinearGradientFill(name.Name, name.MatchName, opacity, startPoint, endPoint);
        }

        Ellipse ReadEllipse(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var position = ReadAnimatableVector3(obj.GetNamedObject("p"));
            var diameter = ReadAnimatableVector3(obj.GetNamedObject("s"));
            var direction = ReadBool(obj, "d") == true;
            AssertAllFieldsRead(obj);
            return new Ellipse(name.Name, name.MatchName, direction, position, diameter);
        }

        Polystar ReadPolystar(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "ix");

            var name = ReadName(obj);
            var direction = ReadBool(obj, "d") == true;

            var type = SyToPolystarType(obj.GetNamedNumber("sy", double.NaN));

            var points = ReadAnimatableFloat(obj.GetNamedObject("pt"));
            if (points.IsAnimated)
            {
                ReportIssue("PolyStar.Points animation is not supported.");
            }

            var position = ReadAnimatableVector3(obj.GetNamedObject("p"));
            if (position.IsAnimated)
            {
                ReportIssue("PolyStar.Position animation is not supported.");
            }

            var rotation = ReadAnimatableFloat(obj.GetNamedObject("r"));
            if (rotation.IsAnimated)
            {
                ReportIssue("PolyStar.Rotation animation is not supported.");
            }

            var outerRadius = ReadAnimatableFloat(obj.GetNamedObject("or"));
            if (outerRadius.IsAnimated)
            {
                ReportIssue("PolyStar.OuterRadius animation is not supported.");
            }

            var outerRoundedness = ReadAnimatableFloat(obj.GetNamedObject("os"));
            if (outerRoundedness.IsAnimated)
            {
                ReportIssue("PolyStar.OuterRoundedness animation is not supported.");
            }

            Animatable<double> innerRadius;
            Animatable<double> innerRoundedness;

            if (type == Polystar.PolyStarType.Star)
            {
                innerRadius = ReadAnimatableFloat(obj.GetNamedObject("ir"));
                if (innerRadius.IsAnimated)
                {
                    ReportIssue("PolyStar.InnerRadius animation is not supported.");
                }

                innerRoundedness = ReadAnimatableFloat(obj.GetNamedObject("is"));
                if (innerRoundedness.IsAnimated)
                {
                    ReportIssue("PolyStar.InnerRoundedness animation is not supported.");
                }
            }
            else
            {
                innerRadius = null;
                innerRoundedness = null;
            }

            AssertAllFieldsRead(obj);
            return new Polystar(
                name.Name,
                name.MatchName,
                direction,
                type,
                points,
                position,
                rotation,
                innerRadius,
                outerRadius,
                innerRoundedness,
                outerRoundedness);
        }

        Rectangle ReadRectangle(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var direction = ReadBool(obj, "d") == true;
            var position = ReadAnimatableVector3(obj.GetNamedObject("p"));
            var size = ReadAnimatableVector3(obj.GetNamedObject("s"));
            var cornerRadius = ReadAnimatableFloat(obj.GetNamedObject("r"));

            AssertAllFieldsRead(obj);
            return new Rectangle(name.Name, name.MatchName, direction, position, size, cornerRadius);
        }


        Shape ReadShape(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "ind");
            IgnoreFieldThatIsNotYetSupported(obj, "ix");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");
            IgnoreFieldThatIsNotYetSupported(obj, "cl");
            IgnoreFieldThatIsNotYetSupported(obj, "closed");

            var name = ReadName(obj);
            var geometry = ReadAnimatableGeometry(obj.GetNamedObject("ks"));
            var direction = ReadBool(obj, "d") == true;
            AssertAllFieldsRead(obj);
            return new Shape(name.Name, name.MatchName, direction, geometry);
        }

        TrimPath ReadTrimPath(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "ix");
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var startPercent = ReadAnimatableFloat(obj.GetNamedObject("s"));
            var endPercent = ReadAnimatableFloat(obj.GetNamedObject("e"));
            var offsetDegrees = ReadAnimatableFloat(obj.GetNamedObject("o"));
            var trimType = MToTrimType(obj.GetNamedNumber("m", 1));
            AssertAllFieldsRead(obj);
            return new TrimPath(
                name.Name,
                name.MatchName,
                trimType,
                startPercent,
                endPercent,
                offsetDegrees);
        }

        Repeater ReadReapeter(JsonObject obj)
        {
            var name = ReadName(obj);
            return new Repeater(name.Name, name.MatchName);
        }

        MergePaths ReadMergePaths(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "hd");

            var name = ReadName(obj);
            var mergeMode = MmToMergeMode(obj.GetNamedNumber("mm"));
            AssertAllFieldsRead(obj);
            return new MergePaths(
                name.Name,
                name.MatchName,
                mergeMode);
        }

        RoundedCorner ReadRoundedCorner(JsonObject obj)
        {
            // Not clear whether we need to read these fields.
            IgnoreFieldThatIsNotYetSupported(obj, "hd");
            IgnoreFieldThatIsNotYetSupported(obj, "ix");

            var name = ReadName(obj);
            var radius = ReadAnimatableFloat(obj.GetNamedObject("r"));
            AssertAllFieldsRead(obj);
            return new RoundedCorner(
                name.Name,
                name.MatchName,
                radius);
        }

        Animatable<double> ReadOpacityPercent(JsonObject obj)
        {
            var jsonOpacity = obj.GetNamedObject("o", null);
            var result = jsonOpacity != null
                ? ReadAnimatableFloat(jsonOpacity)
                : new Animatable<double>(100, null);
            return result;
        }

        Animatable<Color> ReadColor(JsonObject obj) =>
            ReadAnimatableColor(obj.GetNamedObject("c", null));

        Animatable<Color> ReadAnimatableColor(JsonObject obj)
        {
            if (obj == null)
            {
                return new Animatable<Color>(Color.Black, null);
            }

            s_animatableColorParser.ParseJson(this, obj, out IEnumerable<KeyFrame<Color>> keyFrames, out Color initialValue);

            var propertyIndex = ReadInt(obj, "ix");

            return new Animatable<Color>(initialValue, keyFrames, propertyIndex);
        }

        Transform ReadTransform(JsonObject obj)
        {
            IAnimatableVector3 anchor = null;
            IAnimatableVector3 position = null;
            IAnimatableVector3 scalePercent = null;
            Animatable<double> rotation = null;

            var anchorJson = obj.GetNamedObject("a", null);
            if (anchorJson != null)
            {
                anchor = ReadAnimatableVector3(anchorJson);
            }
            else
            {
                anchor = new AnimatableVector3(new Vector3(), null);
            }

            var positionJson = obj.GetNamedObject("p", null);
            if (positionJson != null)
            {
                position = ReadAnimatableVector3(positionJson);
            }
            else
            {
                throw new LottieJsonReaderException("Missing transform for position");
            }

            var scaleJson = obj.GetNamedObject("s", null);
            if (scaleJson != null)
            {
                scalePercent = ReadAnimatableVector3(scaleJson);
            }
            else
            {
                // Repeaters have start/end opacity instead of opacity 

            }

            var rotationJson = obj.GetNamedObject("r", null);
            if (rotationJson == null)
            {
                rotationJson = obj.GetNamedObject("rz", null);
            }
            if (rotationJson != null)
            {
                rotation = ReadAnimatableFloat(rotationJson);
            }
            else
            {
                throw new LottieJsonReaderException("Missing transform for rotation");
            }

            var opacity = ReadOpacityPercent(obj);
            var name = ReadName(obj);

            return new Transform(name.Name, anchor, position, scalePercent, rotation, opacity);
        }

        static bool? ReadBool(JsonObject obj, string name)
        {
            if (!obj.ContainsKey(name))
            {
                return null;
            }

            var value = obj.GetNamedValue(name);

            switch (value.ValueType)
            {
                case JsonValueType.Boolean:
                    return obj.GetNamedBoolean(name);
                case JsonValueType.Number:
                    return ReadInt(obj, name)?.Equals(1);
                case JsonValueType.Null:
                case JsonValueType.String:
                case JsonValueType.Array:
                case JsonValueType.Object:
                default:
                    throw new InvalidOperationException();
            }
        }

        static int? ReadInt(JsonObject obj, string name)
        {
            var value = obj.GetNamedNumber(name, double.NaN);
            if (double.IsNaN(value))
            {
                return null;
            }
            var intValue = unchecked((int)value);
            if (value != intValue)
            {
                return null;
            }
            return intValue;
        }

        IAnimatableVector3 ReadAnimatableVector3(JsonObject obj)
        {
            IgnoreFieldThatIsNotYetSupported(obj, "s");
            // Expressions not supported.
            IgnoreFieldThatIsNotYetSupported(obj, "x");

            var propertyIndex = ReadInt(obj, "ix");
            if (obj.ContainsKey("k"))
            {
                s_animatableVector3Parser.ParseJson(this, obj, out IEnumerable<KeyFrame<Vector3>> keyFrames, out Vector3 initialValue);
                AssertAllFieldsRead(obj);
                return new AnimatableVector3(initialValue, keyFrames, propertyIndex);
            }
            else
            {
                // Split X and Y dimensions 
                var x = ReadAnimatableFloat(obj.GetNamedObject("x"));
                var y = ReadAnimatableFloat(obj.GetNamedObject("y"));
                AssertAllFieldsRead(obj);

                return new AnimatableXYZ(x, y, new Animatable<double>(0, propertyIndex));
            }
        }

        Animatable<PathGeometry> ReadAnimatableGeometry(JsonObject obj)
        {
            s_animatableGeometryParser.ParseJson(this, obj, out IEnumerable<KeyFrame<PathGeometry>> keyFrames, out PathGeometry initialValue);
            var propertyIndex = ReadInt(obj, "ix");
            return new Animatable<PathGeometry>(initialValue, keyFrames, propertyIndex);
        }

        Animatable<double> ReadAnimatableFloat(JsonObject obj)
        {
            s_animatableFloatParser.ParseJson(this, obj, out IEnumerable<KeyFrame<double>> keyFrames, out double initialValue);
            var propertyIndex = ReadInt(obj, "ix");
            return new Animatable<double>(initialValue, keyFrames, propertyIndex);
        }

        static Vector3 ReadVector3FromJsonArray(JsonArray array)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            uint i = 0;
            var count = array.Count;
            for (; i < count; i++)
            {
                // NOTE: indexing JsonArray is faster than enumerating it.
                var number = array.GetNumberAt(i);
                switch (i)
                {
                    case 0:
                        x = number;
                        break;
                    case 1:
                        y = number;
                        break;
                    case 2:
                        z = number;
                        break;
                    default:
                        throw new LottieJsonReaderException("Too many values for Vector3.");
                }
            }

            // Allow either 2 or 3 values to be specified. If 2 values, assume z==0.
            if (i < 2)
            {
                throw new LottieJsonReaderException("Not enough values for Vector3.");
            }

            return new Vector3(x, y, z);
        }

        struct AfterEffectsName
        {
            internal string Name;
            internal string MatchName;
        }

        AfterEffectsName ReadName(JsonObject obj)
        {
            var result = new AfterEffectsName();
            if (_options.HasFlag(Options.IgnoreNames))
            {
                IgnoreFieldIntentionally(obj, "nm");
            }
            else
            {
                result.Name = obj.GetNamedString("nm", "");
            }
            if (_options.HasFlag(Options.IgnoreMatchNames))
            {
                IgnoreFieldIntentionally(obj, "mn");
            }
            else
            {
                result.MatchName = obj.GetNamedString("mn", "");
            }

            return result;
        }

        void ReportIssue(string issue)
        {
            m_issues.Add(issue);
        }

        sealed class AnimatableVector3Parser : AnimatableParser<Vector3>
        {
            internal AnimatableVector3Parser() : base((IJsonValue obj) => ReadVector3FromJsonArray(obj.GetArray())) { }
        }

        sealed class AnimatableColorParser : AnimatableParser<Color>
        {
            internal AnimatableColorParser() : base(ColorValueFactory) { }

            static Func<IJsonValue, Color> ColorValueFactory => (IJsonValue obj) =>
            {
                var colorArray = obj.GetArray();
                double a = 0;
                double r = 0;
                double g = 0;
                double b = 0;
                uint i = 0;
                var count = colorArray.Count;
                for (; i < count; i++)
                {
                    // Note: indexing a JsonArray is faster than enumerating.
                    var number = colorArray.GetNumberAt(i);
                    switch (i)
                    {
                        case 0:
                            r = number;
                            break;
                        case 1:
                            g = number;
                            break;
                        case 2:
                            b = number;
                            break;
                        case 3:
                            a = number;
                            break;
                        default:
                            throw new LottieJsonReaderException("Too many values for Color.");
                    }
                }

                if (i != 4)
                {
                    throw new LottieJsonReaderException("Not enough values for Color.");
                }

                // If all the values are <= 1, treat the values as floats, otherwise they're bytes.
                if (r > 1 || g > 1 || b > 1 || a > 1)
                {
                    // Convert byte to float.
                    a /= 255;
                    r /= 255;
                    g /= 255;
                    b /= 255;
                }
                return Color.FromArgb(a, r, g, b);
            };
        }

        sealed class AnimatableGeometryParser : AnimatableParser<PathGeometry>
        {
            internal AnimatableGeometryParser() : base(ReadPathGeometryFromJson) { }

            static PathGeometry ReadPathGeometryFromJson(IJsonValue value)
            {
                JsonObject pointsData = null;
                if (value.ValueType == JsonValueType.Array)
                {
                    var firstObject = value.GetArray().First();
                    if (firstObject.ValueType == JsonValueType.Object && firstObject.GetObject().ContainsKey("v"))
                    {
                        pointsData = firstObject.GetObject();
                    }
                }
                else if (value.ValueType == JsonValueType.Object && value.GetObject().ContainsKey("v"))
                {
                    pointsData = value.GetObject();
                }

                if (pointsData == null)
                {
                    return null;
                }

                var vertices = pointsData.GetNamedArray("v", null);
                var inTangents = pointsData.GetNamedArray("i", null);
                var outTangents = pointsData.GetNamedArray("o", null);
                var closed = pointsData.GetNamedBoolean("c", false);

                if (vertices == null || inTangents == null || outTangents == null || vertices.Count != inTangents.Count || vertices.Count != outTangents.Count)
                {
                    throw new LottieJsonReaderException($"Unable to process points array or tangents. {pointsData}");
                }

                var initialPoint = new Vector3();
                var beziers = new List<BezierSegment>();

                if (vertices.Count == 0)
                {
                    return new PathGeometry(initialPoint, beziers, false);
                }

                var verticesAsVector3 = ReadVector3Array(vertices);
                var inTangentsAsVector3 = ReadVector3Array(inTangents);
                var outTangentsAsVector3 = ReadVector3Array(outTangents);

                var vertex = verticesAsVector3[0];

                initialPoint = vertex; // initial point

                for (var i = 1; i < verticesAsVector3.Length; i++)
                {
                    vertex = verticesAsVector3[i];
                    var previousVertex = verticesAsVector3[i - 1];
                    var relCp1 = outTangentsAsVector3[i - 1];
                    var relCp2 = inTangentsAsVector3[i];
                    var controlPoint1 = previousVertex + relCp1;
                    var controlPoint2 = vertex + relCp2;

                    var bezier = new BezierSegment(
                        cp1: controlPoint1,
                        cp2: controlPoint2,
                        vertex: vertex);
                    beziers.Add(bezier);
                }

                // If the path is closed, add another bezier back to the starting point.
                if (closed)
                {
                    vertex = verticesAsVector3[0];
                    var previousVertex = verticesAsVector3[verticesAsVector3.Length - 1];
                    var relCp1 = outTangentsAsVector3[verticesAsVector3.Length - 1];
                    var relCp2 = inTangentsAsVector3[0];
                    var controlPoint1 = previousVertex + relCp1;
                    var controlPoint2 = vertex + relCp2;

                    var bezier = new BezierSegment(
                        cp1: controlPoint1,
                        cp2: controlPoint2,
                        vertex: vertex);

                    beziers.Add(bezier);
                }

                return new PathGeometry(initialPoint, beziers, closed);
            }

            static Vector3[] ReadVector3Array(JsonArray array)
            {
                IEnumerable<Vector3> ToVector3Enumerable()
                {
                    var count = array.Count;
                    for (uint i = 0; i < count; i++)
                    {
                        yield return ReadVector3FromJsonArray(array.GetArrayAt(i));
                    }
                }

                return ToVector3Enumerable().ToArray();
            }
        }

        sealed class AnimatableFloatParser : AnimatableParser<double>
        {
            internal AnimatableFloatParser() : base((IJsonValue obj) => ReadFloat(obj)) { }
        }

        abstract class AnimatableParser<T> where T : IEquatable<T>
        {
            readonly Func<IJsonValue, T> _valueFactory;
            static readonly KeyFrame<T>[] s_emptyKeyFrames = new KeyFrame<T>[0];

            protected AnimatableParser(Func<IJsonValue, T> valueFactory)
            {
                _valueFactory = valueFactory;
            }

            internal void ParseJson(LottieCompositionJsonReader reader, JsonObject obj, out IEnumerable<KeyFrame<T>> keyFrames, out T initialValue)
            {
                var isAnimated = ReadBool(obj, "a") == true;

                keyFrames = s_emptyKeyFrames;
                initialValue = default(T);

                foreach (var field in obj)
                {
                    switch (field.Key)
                    {
                        case "k":
                            {
                                var k = field.Value;
                                if (k.ValueType == JsonValueType.Array)
                                {
                                    var kArray = k.GetArray();
                                    if (HasKeyframes(kArray))
                                    {
                                        keyFrames = ReadKeyFrames(reader, kArray).ToArray();
                                        initialValue = keyFrames.First().Value;
                                    }
                                }

                                if (keyFrames == s_emptyKeyFrames)
                                {
                                    initialValue = _valueFactory(k);
                                }
                            }
                            break;

                        // Defines if property is animated. 0 or 1. 
                        // Currently ignored because we derive this from the existence of keyframes.
                        case "a":
                            break;

                        // Property index. Used for expressions. Currently ignored because we don't support expressions.
                        case "ix":
                            // Do not report it as an issue - existence of "ix" doesn't mean that an expression is actually used.
                            break;

                        // Property expression. Currently ignored because we don't support expressions.
                        case "x":
                            reader.ReportIssue("Expressions are not supported.");
                            break;
                        default:
                            throw new LottieJsonReaderException($"Unexpected field: {field.Key}");
                    }
                }

                if (isAnimated && keyFrames == s_emptyKeyFrames)
                {
                    throw new LottieJsonReaderException($"Expected keyframes.");
                }
            }

            static bool HasKeyframes(JsonArray array)
            {
                var firstItem = array[0];
                return firstItem.ValueType == JsonValueType.Object && firstItem.GetObject().ContainsKey("t");
            }

            IEnumerable<KeyFrame<T>> ReadKeyFrames(LottieCompositionJsonReader reader, JsonArray jsonArray)
            {
                uint count = (uint)jsonArray.Count;

                if (count == 0)
                {
                    yield break;
                }

                //
                // Keyframes are encoded in Lottie as an array consisting of a sequence
                // of start and end value with start frame and easing function. The final
                // entry in the array is the frame at which the last interpolation ends.
                // [
                //   { startValue_1, endValue_1, startFrame_1 },  # interpolates from startValue_1 to endValue_1 from startFrame_1 to startFrame_2
                //   { startValue_2, endValue_2, startFrame_2 },  # interpolates from startValue_2 to endValue_2 from startFrame_2 to startFrame_3
                //   { startValue_3, endValue_3, startFrame_3 },  # interpolates from startValue_3 to endValue_3 from startFrame_3 to startFrame_4
                //   { startFrame_4 }
                // ]
                // We convert these to keyframes that match the Windows.UI.Composition notion of a keyframe,
                // which is a triple: {endValue, endTime, easingFunction}.
                // An initial keyframe is created to describe the initial value. It has no easing function.
                //

                T endValue = default(T);
                // The initial keyframe has the same value as the initial value. Easing therefore doesn't
                // matter, but might as well use linear as it's the simplest.
                Easing easing = LinearEasing.Instance;
                // Start by holding from the initial value.
                bool isHolding = true;
                // SpatialBeziers.
                var ti = default(Vector3);
                var to = default(Vector3);

                // NOTE: indexing an array with GetObjectAt is faster than enumerating.
                for (uint i = 0; i < count; i++)
                {
                    var lottieKeyFrame = jsonArray.GetObjectAt(i);

                    // "n" is a name on the keyframe. Never seems to be useful.
                    reader.IgnoreFieldIntentionally(lottieKeyFrame, "n");

                    // Read the start frame.
                    var startFrame = lottieKeyFrame.GetNamedNumber("t", 0);

                    if (i == count - 1)
                    {
                        // This is the final frame. Final frames optionally don't have a value.
                        if (!lottieKeyFrame.ContainsKey("s"))
                        {
                            // It has no value associated with it.
                            yield return new KeyFrame<T>(startFrame, endValue, to, ti, easing);
                            break;
                        }
                    }

                    // Read the start value.
                    var startValue = _valueFactory(lottieKeyFrame.GetNamedValue("s"));

                    // The start of the next entry must be the same as the end of the previous entry
                    // unless in a hold.
                    if (!isHolding && !endValue.Equals(startValue))
                    {
                        throw new InvalidOperationException();
                    }

                    yield return new KeyFrame<T>(startFrame, startValue, to, ti, easing);

                    // Spatial control points.
                    if (lottieKeyFrame.ContainsKey("ti"))
                    {
                        ti = ReadVector3FromJsonArray(lottieKeyFrame.GetNamedArray("ti"));
                        to = ReadVector3FromJsonArray(lottieKeyFrame.GetNamedArray("to"));
                    }

                    // Get the easing to the end value, and get the end value.
                    if (ReadBool(lottieKeyFrame, "h") == true)
                    {
                        // Hold the current value. The next value comes from the start
                        // of the next entry.
                        isHolding = true;
                        easing = HoldEasing.Instance;
                        // Synthesize an endValue. This is only used if this is the final frame.
                        endValue = startValue;
                    }
                    else
                    {
                        // Read the easing function parameters. If there are any parameters, it's a CubicBezierEasing.
                        var cp1Json = lottieKeyFrame.GetNamedObject("o", null);
                        var cp2Json = lottieKeyFrame.GetNamedObject("i", null);
                        if (cp1Json != null && cp2Json != null)
                        {
                            var cp1 = new Vector3(ReadFloat(cp1Json.GetNamedValue("x")), ReadFloat(cp1Json.GetNamedValue("y")), 0);
                            var cp2 = new Vector3(ReadFloat(cp2Json.GetNamedValue("x")), ReadFloat(cp2Json.GetNamedValue("y")), 0);
                            easing = new CubicBezierEasing(cp1, cp2);
                        }
                        else
                        {
                            easing = LinearEasing.Instance;
                        }

                        // Read the end value. The end frame number isn't known until 
                        // the next pair is read.
                        endValue = _valueFactory(lottieKeyFrame.GetNamedValue("e"));
                    }

                    reader.AssertAllFieldsRead(lottieKeyFrame);
                }
            }
        }

        static double ReadFloat(IJsonValue jsonValue)
        {
            switch (jsonValue.ValueType)
            {
                case JsonValueType.Number:
                    return jsonValue.GetNumber();
                case JsonValueType.Array:
                    {
                        var array = jsonValue.GetArray();
                        switch (array.Count)
                        {
                            case 0:
                                throw new LottieJsonReaderException("Expecting float but found empty array.");
                            case 1:
                                return array.GetNumberAt(0);
                            default:
                                // Some Lottie files have multiple values in arrays that should only have one. Just
                                // take the first value.
                                return array.GetNumberAt(0);
                        }
                    }
                case JsonValueType.Null:
                case JsonValueType.Boolean:
                case JsonValueType.String:
                case JsonValueType.Object:
                default:
                    throw new LottieJsonReaderException($"Expected float but found {jsonValue.ValueType}.");
            }
        }

        static BlendMode BmToBlendMode(double bm)
        {
            if (bm == (int)bm)
            {
                switch ((int)bm)
                {
                    case 0: return BlendMode.Normal;
                    case 1: return BlendMode.Multiply;
                    case 2: return BlendMode.Screen;
                    case 3: return BlendMode.Overlay;
                    case 4: return BlendMode.Darken;
                    case 5: return BlendMode.Lighten;
                    case 6: return BlendMode.ColorDodge;
                    case 7: return BlendMode.ColorBurn;
                    case 8: return BlendMode.HardLight;
                    case 9: return BlendMode.SoftLight;
                    case 10: return BlendMode.Difference;
                    case 11: return BlendMode.Exclusion;
                    case 12: return BlendMode.Hue;
                    case 13: return BlendMode.Saturation;
                    case 14: return BlendMode.Color;
                    case 15: return BlendMode.Luminosity;
                    default:
                        throw new LottieJsonReaderException($"Unexpected blend mode: {bm}.");
                }
            }
            throw new LottieJsonReaderException($"Unexpected layer type: {bm}.");
        }

        static Layer.LayerType TyToLayerType(double ty)
        {
            if (ty == (int)ty)
            {
                switch ((int)ty)
                {
                    case 0: return Layer.LayerType.PreComp;
                    case 1: return Layer.LayerType.Solid;
                    case 2: return Layer.LayerType.Image;
                    case 3: return Layer.LayerType.Null;
                    case 4: return Layer.LayerType.Shape;
                    case 5: return Layer.LayerType.Text;
                }
            }
            throw new LottieJsonReaderException($"Unexpected layer type: {ty}.");
        }

        static Polystar.PolyStarType SyToPolystarType(double sy)
        {
            if (sy == (int)sy)
            {
                switch ((int)sy)
                {
                    case 1: return Polystar.PolyStarType.Star;
                    case 2: return Polystar.PolyStarType.Polygon;
                }
            }
            throw new LottieJsonReaderException($"Unexpected polystar type: {sy}.");
        }

        static SolidColorStroke.LineCapType LcToLineCapType(double lc)
        {
            if (lc == (int)lc)
            {
                switch ((int)lc)
                {
                    case 1: return SolidColorStroke.LineCapType.Butt;
                    case 2: return SolidColorStroke.LineCapType.Round;
                    case 3: return SolidColorStroke.LineCapType.Projected;
                }
            }
            throw new LottieJsonReaderException($"Unexpected linecap type: {lc}.");
        }

        static SolidColorStroke.LineJoinType LjToLineJoinType(double lj)
        {
            if (lj == (int)lj)
            {
                switch ((int)lj)
                {
                    case 1: return SolidColorStroke.LineJoinType.Miter;
                    case 2: return SolidColorStroke.LineJoinType.Round;
                    case 3: return SolidColorStroke.LineJoinType.Bevel;
                }
            }
            throw new LottieJsonReaderException($"Unexpected linejoin type: {lj}.");
        }

        static TrimPath.TrimType MToTrimType(double m)
        {
            if (m == (int)m)
            {
                switch ((int)m)
                {
                    case 1: return TrimPath.TrimType.Simultaneously;
                    case 2: return TrimPath.TrimType.Individually;
                }
            }
            throw new LottieJsonReaderException($"Unexpected trim type: {m}.");
        }

        static MergePaths.MergeMode MmToMergeMode(double mm)
        {
            if (mm == (int)mm)
            {
                switch ((int)mm)
                {
                    case 1: return MergePaths.MergeMode.Merge;
                    case 2: return MergePaths.MergeMode.Add;
                    case 3: return MergePaths.MergeMode.Subtract;
                    case 4: return MergePaths.MergeMode.Intersect;
                    case 5: return MergePaths.MergeMode.ExcludeIntersections;
                }
            }
            throw new LottieJsonReaderException($"Unexpected merge mode: {mm}.");
        }

        static GradientType TToGradientType(double t)
        {
            if (t == (int)t)
            {
                switch ((int)t)
                {
                    case 1: return GradientType.Linear;
                    case 2: return GradientType.Radial;
                }
            }
            throw new LottieJsonReaderException($"Unexpected gradient type: {t}");
        }

        enum GradientType
        {
            Linear,
            Radial,
        }

        // Indicates that the given field will not be read because we don't yet support it.
        [Conditional("CheckForUnparsedFields")]
        void IgnoreFieldThatIsNotYetSupported(CheckedJsonObject obj, string fieldName)
        {
            obj._readFields.Add(fieldName);
        }

        // Indicates that the given field is not read because we don't need to read it.
        [Conditional("CheckForUnparsedFields")]
        void IgnoreFieldIntentionally(CheckedJsonObject obj, string fieldName)
        {
            obj._readFields.Add(fieldName);
        }


        // Reports an issue if the given JsonObject has fields that were not read.
        [Conditional("CheckForUnparsedFields")]
        void AssertAllFieldsRead(CheckedJsonObject obj, [CallerMemberName]string memberName = "")
        {
            var read = obj._readFields;
            var unread = new List<string>();
            foreach (var pair in obj)
            {
                if (!read.Contains(pair.Key))
                {
                    unread.Add(pair.Key);
                }
            }

            unread.Sort();
            foreach (var unreadField in unread)
            {
                ReportIssue($"{memberName} ignored {unreadField}");
            }
        }
    }
}
