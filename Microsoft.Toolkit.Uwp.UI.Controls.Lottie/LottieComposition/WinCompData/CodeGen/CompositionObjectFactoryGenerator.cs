using System;
using System.Collections.Generic;
using System.Linq;
using WinCompData.Sn;
using WinCompData.Tools;
using WinCompData.Wui;

namespace WinCompData.CodeGen
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CompositionObjectFactoryGenerator
    {
        readonly List<string> _codeLines = new List<string>();
        readonly bool _setCommentProperties;
        readonly ObjectGraph<ObjectData> _objectGraph;
        // The subset of the object graph for which nodes will be generated.
        readonly ObjectData[] _canonicalNodes;

        CompositionObjectFactoryGenerator(CompositionObject graphRoot, bool setCommentProperties)
        {
            _setCommentProperties = setCommentProperties;
            // Build the object graph.
            _objectGraph = ObjectGraph<ObjectData>.FromCompositionObject(graphRoot, includeVertices: false);

            // Atomize the nodes.
            Atomize(_objectGraph);

            // Get the canonical nodes. These are the nodes for which code will be generated.
            var canonicals = _objectGraph.Select(node => node.Canonical).Distinct().ToArray();

            // Give names to each canonical node.
            SetCanonicalMethodNames(canonicals);

            // Save the canonical nodes, ordered by the name that was just set.
            _canonicalNodes = canonicals.OrderBy(node => node.Name).ToArray();
        }

        /// <summary>
        /// Returns the C# code for a factory that will instantiate the given <see cref="Visual"/> as a
        /// Windows.UI.Composition Visual.
        /// </summary>
        public static string CreateFactoryCode(
            string className,
            Visual rootVisual,
            float width,
            float height,
            CompositionPropertySet progressPropertySet,
            string progressPropertyName,
            TimeSpan duration)
        {
            var generator = new CompositionObjectFactoryGenerator(rootVisual, false);
            var codeBuilder = new CodeBuilder();
            generator.GenerateCode(codeBuilder, className, rootVisual, width, height, progressPropertySet, progressPropertyName, duration);
            return codeBuilder.ToString();
        }

        // Returns the canonical node for the given object.
        ObjectData NodeFor(object obj) => _objectGraph[obj].Canonical;

        void GenerateCode(
            CodeBuilder builder,
            string className,
            Visual rootVisual,
            float width,
            float height,
            CompositionPropertySet progressPropertySet,
            string progressPropertyName,
            TimeSpan duration)
        {
            // Generate methods for each node. Initially assume that none of them
            // need storage to hold their results.
            foreach (var node in _canonicalNodes)
            {
                GenerateCodeForNode(node);
            }

            // Generate the code for the root method. This is done here to ensure
            // the GetCreateMethodName() call is done to register the reference
            // on the root visual's node. If there's more than one reference to
            // the method it will be regenerated with storage below.
            var rootMethodBuilder = new CodeBuilder();
            rootMethodBuilder.WriteLine("internal static Visual InstantiateComposition(Compositor compositor)");
            rootMethodBuilder.Indent();
            rootMethodBuilder.WriteLine($"=> new Instantiator(compositor).{NodeFor(rootVisual).GetCreateMethodName()}();");
            rootMethodBuilder.UnIndent();

            // Any node that is referenced from more than one place needs to have storage
            // unless the method takes a parameter. Regenerate the code for all of these nodes
            // to include the parameter, and generate the storage field.
            var nodesThatNeedStorage =
                (from node in _canonicalNodes
                 where node.CreateMethodNameCallCounter > 1
                    && !node.NeedsParameter
                 select node).ToArray();

            var fieldsBuilder = new CodeBuilder();
            foreach (var node in nodesThatNeedStorage)
            {
                // Force the node to use storage.
                node.ForceStorage();

                // Regenerate the code.
                GenerateCodeForNode(node);

                // Generate a field for the storage.
                switch (node.Type)
                {
                    case Graph.NodeType.CompositionObject:
                        var compObject = (CompositionObject)node.Object;
                        fieldsBuilder.WriteLine($"{compObject.Type.ToString()} {node.FieldName};");
                        break;
                    case Graph.NodeType.CompositionPath:
                        fieldsBuilder.WriteLine($"CompositionPath {node.FieldName};");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            // Write everything out.
            builder.WriteLine("using Lottie;");

            // Include the Win2D namespaces if the composition needs it.
            var requiresWin2D = _canonicalNodes.Where(n => n.RequireWin2D).Any();
            if (requiresWin2D)
            {
                builder.WriteLine("using Microsoft.Graphics.Canvas;");
                builder.WriteLine("using Microsoft.Graphics.Canvas.Geometry;");
            }

            builder.WriteLine("using System;");
            builder.WriteLine("using System.Numerics;");
            builder.WriteLine("using Windows.UI;");
            builder.WriteLine("using Windows.UI.Composition;");
            // Needed for Windows.Compositor.Current.
            builder.WriteLine("using Windows.UI.Xaml;");
            builder.WriteLine();
            builder.WriteLine("namespace Compositions");
            builder.OpenScope();
            builder.WriteLine($"sealed class {className} : Lottie.ICompositionSource");
            builder.OpenScope();

            // Generate the method that creates an instance of the composition.
            builder.WriteLine("public void CreateInstance(");
            builder.Indent();
            builder.WriteLine("Compositor compositor,");
            builder.WriteLine("out Visual rootVisual,");
            builder.WriteLine("out Vector2 size,");
            builder.WriteLine("out CompositionPropertySet progressPropertySet,");
            builder.WriteLine("out string progressPropertyName,");
            builder.WriteLine("out TimeSpan duration)");
            builder.UnIndent();
            builder.OpenScope();
            builder.WriteLine("rootVisual = Instantiator.InstantiateComposition(compositor);");
            builder.WriteLine($"size = new Vector2({Float(width)}, {Float(height)});");
            builder.WriteLine("progressPropertySet = rootVisual.Properties;");
            builder.WriteLine($"progressPropertyName = {String(progressPropertyName)};");
            builder.WriteLine($"duration = {TimeSpan(duration)};");
            builder.CloseScope();
            builder.WriteLine();

            // Generate the ICompositionSource interface.
            builder.WriteLine("void ICompositionSource.ConnectSink(ICompositionSink sink)");
            builder.OpenScope();
            builder.WriteLine("CreateInstance(");
            builder.Indent();
            builder.WriteLine("Window.Current.Compositor,");
            builder.WriteLine("out var rootVisual,");
            builder.WriteLine("out var size,");
            builder.WriteLine("out var progressPropertySet,");
            builder.WriteLine("out var progressPropertyName,");
            builder.WriteLine("out var duration);");
            builder.UnIndent();
            builder.WriteLine();
            builder.WriteLine("sink.SetContent(");
            builder.Indent();
            builder.WriteLine("rootVisual,");
            builder.WriteLine("size,");
            builder.WriteLine("progressPropertySet,");
            builder.WriteLine("progressPropertyName,");
            builder.WriteLine("duration,");
            builder.WriteLine("null);");
            builder.UnIndent();
            builder.CloseScope();
            builder.WriteLine();
            builder.WriteLine("void ICompositionSource.DisconnectSink(ICompositionSink sink) { }");
            builder.WriteLine();

            // Write the instantiator.
            builder.WriteLine("sealed class Instantiator");
            builder.OpenScope();
            builder.WriteLine("readonly Compositor _c;");

            // Write the fields.
            builder.WriteCodeBuilder(fieldsBuilder);

            // Write the method that instantiates everything.
            builder.WriteLine();
            builder.WriteCodeBuilder(rootMethodBuilder);

            // Write the constructor for the instantiator.
            builder.WriteLine();
            builder.WriteLine("Instantiator(Compositor compositor)");
            builder.OpenScope();
            builder.WriteLine($"_c = compositor;");
            builder.CloseScope();
            builder.WriteLine();


            // Write the methods for each node.
            foreach (var node in _canonicalNodes)
            {
                if (!node.CodeBuilder.IsEmpty)
                {
                    builder.WriteCodeBuilder(node.CodeBuilder);
                }
            }

            builder.CloseScope();
            builder.CloseScope();
            builder.CloseScope();
        }

        // Generates code for the given node. The code is written into the CodeBuilder on the node.
        void GenerateCodeForNode(ObjectData node)
        {
            // Remove any content that we previously created.
            node.CodeBuilder.Clear();
            switch (node.Type)
            {
                case Graph.NodeType.CompositionObject:
                    GenerateObjectFactory(node.CodeBuilder, (CompositionObject)node.Object, node);
                    break;
                case Graph.NodeType.CompositionPath:
                    GenerateCompositionPathFactory(node.CodeBuilder, (CompositionPath)node.Object, node);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        bool GenerateObjectFactory(CodeBuilder builder, CompositionObject obj, ObjectData node)
        {
            switch (obj.Type)
            {
                case CompositionObjectType.AnimationController:
                    // Do not generate code for animation controllers. It is done inline in the CompositionObject initialization.
                    return true;
                case CompositionObjectType.ColorKeyFrameAnimation:
                    return GenerateColorKeyFrameAnimationFactory(builder, (ColorKeyFrameAnimation)obj, node);
                case CompositionObjectType.CompositionColorBrush:
                    return GenerateCompositionColorBrushFactory(builder, (CompositionColorBrush)obj, node);
                case CompositionObjectType.CompositionContainerShape:
                    return GenerateContainerShapeFactory(builder, (CompositionContainerShape)obj, node);
                case CompositionObjectType.CompositionEllipseGeometry:
                    return GenerateCompositionEllipseGeometryFactory(builder, (CompositionEllipseGeometry)obj, node);
                case CompositionObjectType.CompositionPathGeometry:
                    return GenerateCompositionPathGeometryFactory(builder, (CompositionPathGeometry)obj, node);
                case CompositionObjectType.CompositionPropertySet:
                    // Do not generate code for property sets. It is done inline in the CompositionObject initialization.
                    return true;
                case CompositionObjectType.CompositionRectangleGeometry:
                    return GenerateCompositionRectangleGeometryFactory(builder, (CompositionRectangleGeometry)obj, node);
                case CompositionObjectType.CompositionRoundedRectangleGeometry:
                    return GenerateCompositionRoundedRectangleGeometryFactory(builder, (CompositionRoundedRectangleGeometry)obj, node);
                case CompositionObjectType.CompositionSpriteShape:
                    return GenerateSpriteShapeFactory(builder, (CompositionSpriteShape)obj, node);
                case CompositionObjectType.CompositionViewBox:
                    return GenerateCompositionViewBoxFactory(builder, (CompositionViewBox)obj, node);
                case CompositionObjectType.ContainerVisual:
                    return GenerateContainerVisualFactory(builder, (ContainerVisual)obj, node);
                case CompositionObjectType.CubicBezierEasingFunction:
                    return GenerateCubicBezierEasingFunctionFactory(builder, (CubicBezierEasingFunction)obj, node);
                case CompositionObjectType.ExpressionAnimation:
                    return GenerateExpressionAnimationFactory(builder, (ExpressionAnimation)obj, node);
                case CompositionObjectType.InsetClip:
                    return GenerateInsetClipFactory(builder, (InsetClip)obj, node);
                case CompositionObjectType.LinearEasingFunction:
                    return GenerateLinearEasingFunctionFactory(builder, (LinearEasingFunction)obj, node);
                case CompositionObjectType.PathKeyFrameAnimation:
                    return GeneratePathKeyFrameAnimationFactory(builder, (PathKeyFrameAnimation)obj, node);
                case CompositionObjectType.ScalarKeyFrameAnimation:
                    return GenerateScalarKeyFrameAnimationFactory(builder, (ScalarKeyFrameAnimation)obj, node);
                case CompositionObjectType.ShapeVisual:
                    return GenerateShapeVisualFactory(builder, (ShapeVisual)obj, node);
                case CompositionObjectType.StepEasingFunction:
                    return GenerateStepEasingFunctionFactory(builder, (StepEasingFunction)obj, node);
                case CompositionObjectType.Vector2KeyFrameAnimation:
                    return GenerateVector2KeyFrameAnimationFactory(builder, (Vector2KeyFrameAnimation)obj, node);
                case CompositionObjectType.Vector3KeyFrameAnimation:
                    return GenerateVector3KeyFrameAnimationFactory(builder, (Vector3KeyFrameAnimation)obj, node);
                default:
                    throw new InvalidOperationException();
            }
        }

        bool GenerateInsetClipFactory(CodeBuilder builder, InsetClip obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateInsetClip()");
            InitializeCompositionClip(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateLinearEasingFunctionFactory(CodeBuilder builder, LinearEasingFunction obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateLinearEasingFunction()");
            InitializeCompositionObject(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCubicBezierEasingFunctionFactory(CodeBuilder builder, CubicBezierEasingFunction obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c.CreateCubicBezierEasingFunction({Vector2(obj.ControlPoint1)}, {Vector2(obj.ControlPoint2)})");
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateStepEasingFunctionFactory(CodeBuilder builder, StepEasingFunction obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c.CreateStepEasingFunction()");
            if (obj.FinalStep != 1)
            {
                builder.WriteLine($"result.FinalStep = {Int(obj.FinalStep)};");
            }
            if (obj.InitialStep != 0)
            {
                builder.WriteLine($"result.InitialStep = {Int(obj.InitialStep)};");
            }
            if (obj.IsFinalStepSingleFrame)
            {
                builder.WriteLine($"result.IsFinalStepSingleFrame  = {Bool(obj.IsFinalStepSingleFrame)};");
            }
            if (obj.IsInitialStepSingleFrame)
            {
                builder.WriteLine($"result.IsInitialStepSingleFrame  = {Bool(obj.IsInitialStepSingleFrame)};");
            }
            if (obj.StepCount != 1)
            {
                builder.WriteLine($"result.StepCount = {Int(obj.StepCount)};");
            }
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateContainerVisualFactory(CodeBuilder builder, ContainerVisual obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateContainerVisual()");
            InitializeContainerVisual(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        void StartAnimations(CodeBuilder builder, CompositionObject obj, string localName = "result", string animationNamePrefix = "")
        {
            var propertySet = obj.Properties;
            if (propertySet.Animators.Any())
            {
                foreach (var animator in propertySet.Animators)
                {
                    builder.OpenScope();
                    var anim = NodeFor(animator.Animation);
                    if (anim.NeedsParameter)
                    {
                        builder.WriteLine($"var {animationNamePrefix}animation = {anim.GetCreateMethodName()}({localName});");
                    }
                    else
                    {
                        builder.WriteLine($"var {animationNamePrefix}animation = {anim.GetCreateMethodName()}();");
                    }
                    builder.WriteLine($"{localName}.StartAnimation({String(animator.Target)}, {animationNamePrefix}animation);");
                    if (animator.Controller != null)
                    {
                        builder.OpenScope();
                        builder.WriteLine($"var controller = {localName}.TryGetAnimationController({String(animator.Target)});");
                        InitializeCompositionObject(builder, animator.Controller, "controller", "controller");
                        // TODO - we always pause here, but really should only pause it if WinCompData does it. 
                        builder.WriteLine("controller.Pause();");
                        StartAnimations(builder, animator.Controller, "controller", "controller");
                        builder.CloseScope();
                    }
                    builder.CloseScope();
                }
            }

            if (obj.Animators.Any())
            {
                foreach (var animator in obj.Animators)
                {
                    builder.OpenScope();
                    var anim = NodeFor(animator.Animation);
                    if (anim.NeedsParameter)
                    {
                        builder.WriteLine($"var {animationNamePrefix}animation = {anim.GetCreateMethodName()}({localName});");
                    }
                    else
                    {
                        builder.WriteLine($"var {animationNamePrefix}animation = {anim.GetCreateMethodName()}();");
                    }
                    builder.WriteLine($"{localName}.StartAnimation({String(animator.Target)}, {animationNamePrefix}animation);");
                    if (animator.Controller != null)
                    {
                        builder.OpenScope();
                        builder.WriteLine($"var controller = {localName}.TryGetAnimationController({String(animator.Target)});");
                        InitializeCompositionObject(builder, animator.Controller, "controller", "controller");
                        // TODO - we always pause here, but really should only pause it if WinCompData does it. 
                        builder.WriteLine("controller.Pause();");
                        StartAnimations(builder, animator.Controller, "controller", "controller");
                        builder.CloseScope();
                    }
                    builder.CloseScope();
                }
            }
        }

        void InitializeCompositionObject(CodeBuilder builder, CompositionObject obj, string localName = "result", string animationNamePrefix = "")
        {
            if (_setCommentProperties)
            {
                if (!string.IsNullOrWhiteSpace(obj.Comment))
                {
                    builder.WriteLine($"{localName}.Comment = {String(obj.Comment)};");
                }
            }

            var propertySet = obj.Properties;
            if (propertySet.PropertyNames.Any())
            {
                builder.WriteLine($"var propertySet = {localName}.Properties;");
                foreach (var prop in propertySet.ScalarProperties)
                {
                    builder.WriteLine($"propertySet.InsertScalar({String(prop.Key)}, {Float(prop.Value)});");
                }

                foreach (var prop in propertySet.Vector2Properties)
                {
                    builder.WriteLine($"propertySet.InsertVector2({String(prop.Key)}, {Vector2(prop.Value)});");
                }

            }
        }

        void InitializeCompositionBrush(CodeBuilder builder, CompositionBrush obj)
        {
            InitializeCompositionObject(builder, obj);
        }

        void InitializeVisual(CodeBuilder builder, Visual obj)
        {
            InitializeCompositionObject(builder, obj);
            if (obj.CenterPoint.HasValue)
            {
                builder.WriteLine($"result.CenterPoint = {Vector3(obj.CenterPoint.Value)};");
            }
            if (obj.Clip != null)
            {
                builder.WriteLine($"result.Clip = {NodeFor(obj.Clip).GetCreateMethodName()}();");
            }
            if (obj.Offset.HasValue)
            {
                builder.WriteLine($"result.Offset = {Vector3(obj.Offset.Value)};");
            }
            if (obj.RotationAngleInDegrees.HasValue)
            {
                builder.WriteLine($"result.RotationAngleInDegrees = {Float(obj.RotationAngleInDegrees.Value)};");
            }
            if (obj.Scale.HasValue)
            {
                builder.WriteLine($"result.Scale = {Vector3(obj.Scale.Value)};");
            }
            if (obj.Size.HasValue)
            {
                builder.WriteLine($"result.Size = {Vector2(obj.Size.Value)};");
            }
        }

        void InitializeCompositionClip(CodeBuilder builder, CompositionClip obj)
        {
            InitializeCompositionObject(builder, obj);

            if (obj.CenterPoint.HasValue)
            {
                builder.WriteLine($"result.CenterPoint = {Vector2(obj.CenterPoint.Value)};");
            }
            if (obj.Scale.HasValue)
            {
                builder.WriteLine($"result.Scale = {Vector2(obj.Scale.Value)};");
            }
        }

        void InitializeCompositionShape(CodeBuilder builder, CompositionShape obj)
        {
            InitializeCompositionObject(builder, obj);

            if (obj.CenterPoint.HasValue)
            {
                builder.WriteLine($"result.CenterPoint = {Vector2(obj.CenterPoint.Value)};");
            }
            if (obj.Offset != null)
            {
                builder.WriteLine($"result.Offset = {Vector2(obj.Offset.Value)};");
            }
            if (obj.RotationAngleInDegrees.HasValue)
            {
                builder.WriteLine($"result.RotationAngleInDegrees = {Float(obj.RotationAngleInDegrees.Value)};");
            }
            if (obj.Scale.HasValue)
            {
                builder.WriteLine($"result.Scale = {Vector2(obj.Scale.Value)};");
            }
        }

        void InitializeContainerVisual(CodeBuilder builder, ContainerVisual obj)
        {
            InitializeVisual(builder, obj);

            if (obj.Children.Any())
            {
                builder.WriteLine("var children = result.Children;");
                foreach (var child in obj.Children)
                {
                    builder.WriteLine($"children.InsertAtTop({NodeFor(child).GetCreateMethodName()}());");
                }
            }
        }

        void InitializeCompositionGeometry(CodeBuilder builder, CompositionGeometry obj)
        {
            InitializeCompositionObject(builder, obj);
            if (obj.TrimEnd.HasValue)
            {
                builder.WriteLine($"result.TrimEnd = {Float(obj.TrimEnd.Value)};");
            }
            if (obj.TrimOffset.HasValue)
            {
                builder.WriteLine($"result.TrimOffset = {Float(obj.TrimOffset.Value)};");
            }
            if (obj.TrimStart.HasValue)
            {
                builder.WriteLine($"result.TrimStart = {Float(obj.TrimStart.Value)};");
            }
        }

        void InitializeCompositionAnimation(CodeBuilder builder, CompositionAnimation obj)
        {
            InitializeCompositionAnimationWithParameters(
                builder,
                obj,
                obj.ReferenceParameters.Select(p => new KeyValuePair<string, string>(p.Key, $"{NodeFor(p.Value).GetCreateMethodName()}()")));
        }

        void InitializeCompositionAnimationWithParameters(CodeBuilder builder, CompositionAnimation obj, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            InitializeCompositionObject(builder, obj);
            if (!string.IsNullOrWhiteSpace(obj.Target))
            {
                builder.WriteLine($"result.Target = {String(obj.Target)};");
            }
            foreach (var parameter in parameters)
            {
                builder.WriteLine($"result.SetReferenceParameter({String(parameter.Key)}, {parameter.Value});");
            }
        }

        bool GenerateExpressionAnimationFactory(CodeBuilder builder, ExpressionAnimation obj, ObjectData node)
        {
            if (node.NeedsParameter)
            {
                // The expression needs to be generated with a parameter.
                var parameter = obj.ReferenceParameters.Single();
                WriteObjectFactoryStart(builder, node, new[] { "CompositionObject arg" });
                WriteCreateAssignment(builder, node, $"_c.CreateExpressionAnimation({String(obj.Expression)})");
                InitializeCompositionAnimationWithParameters(builder, obj, new[] { new KeyValuePair<string, string>(parameter.Key, "arg") });
            }
            else
            {
                WriteObjectFactoryStart(builder, node);
                WriteCreateAssignment(builder, node, $"_c.CreateExpressionAnimation({String(obj.Expression)})");
                InitializeCompositionAnimation(builder, obj);
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        void InitializeKeyFrameAnimation(CodeBuilder builder, KeyFrameAnimation_ obj)
        {
            InitializeCompositionAnimation(builder, obj);
            builder.WriteLine($"result.Duration = {TimeSpan(obj.Duration)};");
        }

        bool GenerateColorKeyFrameAnimationFactory(CodeBuilder builder, ColorKeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateColorKeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                builder.WriteLine($"result.InsertKeyFrame({Float(kf.Progress)}, {Color(kf.Value)}, {NodeFor(kf.Easing).GetCreateMethodName()}());");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateVector2KeyFrameAnimationFactory(CodeBuilder builder, Vector2KeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateVector2KeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                builder.WriteLine($"result.InsertKeyFrame({Float(kf.Progress)}, {Vector2(kf.Value)}, {NodeFor(kf.Easing).GetCreateMethodName()}());");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateVector3KeyFrameAnimationFactory(CodeBuilder builder, Vector3KeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateVector3KeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                builder.WriteLine($"result.InsertKeyFrame({Float(kf.Progress)}, {Vector3(kf.Value)}, {NodeFor(kf.Easing).GetCreateMethodName()}());");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GeneratePathKeyFrameAnimationFactory(CodeBuilder builder, PathKeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreatePathKeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                var path = NodeFor(kf.Value);
                builder.WriteLine($"result.InsertKeyFrame({Float(kf.Progress)}, {path.GetCreateMethodName()}(), {NodeFor(kf.Easing).GetCreateMethodName()}());");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }


        bool GenerateScalarKeyFrameAnimationFactory(CodeBuilder builder, ScalarKeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateScalarKeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                builder.WriteLine($"result.InsertKeyFrame({Float(kf.Progress)}, {Float(kf.Value)}, {NodeFor(kf.Easing).GetCreateMethodName()}());");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionRectangleGeometryFactory(CodeBuilder builder, CompositionRectangleGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateRectangleGeometry()");
            InitializeCompositionGeometry(builder, obj);
            builder.WriteLine($"result.Size = {Vector2(obj.Size)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionRoundedRectangleGeometryFactory(CodeBuilder builder, CompositionRoundedRectangleGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateRoundedRectangleGeometry()");
            InitializeCompositionGeometry(builder, obj);
            builder.WriteLine($"result.CornerRadius = {Vector2(obj.CornerRadius)};");
            builder.WriteLine($"result.Size = {Vector2(obj.Size)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionEllipseGeometryFactory(CodeBuilder builder, CompositionEllipseGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateEllipseGeometry()");
            InitializeCompositionGeometry(builder, obj);
            builder.WriteLine($"result.Center = {Vector2(obj.Center)};");
            builder.WriteLine($"result.Radius = {Vector2(obj.Radius)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionPathGeometryFactory(CodeBuilder builder, CompositionPathGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            var path = NodeFor(obj.Path);
            WriteCreateAssignment(builder, node, $"_c.CreatePathGeometry({path.GetCreateMethodName()}())");
            InitializeCompositionGeometry(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionColorBrushFactory(CodeBuilder builder, CompositionColorBrush obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c.CreateColorBrush({Color(obj.Color)})");
            InitializeCompositionBrush(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateShapeVisualFactory(CodeBuilder builder, ShapeVisual obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateShapeVisual()");
            InitializeContainerVisual(builder, obj);

            if (obj.Shapes.Any())
            {
                builder.WriteLine("var shapes = result.Shapes;");
                foreach (var shape in obj.Shapes)
                {
                    builder.WriteLine($"shapes.Add({NodeFor(shape).GetCreateMethodName()}());");
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateContainerShapeFactory(CodeBuilder builder, CompositionContainerShape obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateContainerShape()");
            InitializeCompositionShape(builder, obj);
            if (obj.Shapes.Any())
            {
                builder.WriteLine("var shapes = result.Shapes;");
                foreach (var shape in obj.Shapes)
                {
                    builder.WriteLine($"shapes.Add({NodeFor(shape).GetCreateMethodName()}());");
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateSpriteShapeFactory(CodeBuilder builder, CompositionSpriteShape obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, "_c.CreateSpriteShape()");
            InitializeCompositionShape(builder, obj);

            if (obj.FillBrush != null)
            {
                builder.WriteLine($"result.FillBrush = {NodeFor(obj.FillBrush).GetCreateMethodName()}();");
            }
            if (obj.Geometry != null)
            {
                builder.WriteLine($"result.Geometry = {NodeFor(obj.Geometry).GetCreateMethodName()}();");
            }
            if (obj.IsStrokeNonScaling)
            {
                builder.WriteLine("result.IsStrokeNonScaling = true;");
            }
            if (obj.StrokeBrush != null)
            {
                builder.WriteLine($"result.StrokeBrush = {NodeFor(obj.StrokeBrush).GetCreateMethodName()}();");
            }
            if (obj.StrokeDashCap != CompositionStrokeCap.Flat)
            {
                builder.WriteLine($"result.StrokeDashCap = {StrokeCap(obj.StrokeDashCap)};");
            }
            if (obj.StrokeDashOffset != 0)
            {
                builder.WriteLine($"result.DashOffset = {Float(obj.StrokeDashOffset)};");
            }
            if (obj.StrokeDashArray.Count > 0)
            {
                builder.WriteLine($"var strokeDashArray = obj.StrokeDashArray;");
                foreach (var strokeDash in obj.StrokeDashArray)
                {
                    builder.WriteLine($"strokeDashArray.Add({Float(strokeDash)});");
                }
            }
            if (obj.StrokeEndCap != CompositionStrokeCap.Flat)
            {
                builder.WriteLine($"result.StrokeEndCap = {StrokeCap(obj.StrokeEndCap)};");
            }
            if (obj.StrokeLineJoin != CompositionStrokeLineJoin.Miter)
            {
                builder.WriteLine($"result.StrokeLineJoin = {StrokeLineJoin(obj.StrokeLineJoin)};");
            }
            if (obj.StrokeStartCap != CompositionStrokeCap.Flat)
            {
                builder.WriteLine($"result.StrokeStartCap = {StrokeCap(obj.StrokeStartCap)};");
            }
            if (obj.StrokeMiterLimit != 1)
            {
                builder.WriteLine($"result.StrokeMiterLimit = {Float(obj.StrokeMiterLimit)};");
            }
            if (obj.StrokeThickness != 1)
            {
                builder.WriteLine($"result.StrokeThickness = {Float(obj.StrokeThickness)};");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionViewBoxFactory(CodeBuilder builder, CompositionViewBox obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c.CreateViewBox()");
            InitializeCompositionObject(builder, obj);
            builder.WriteLine($"result.Size = {Vector2(obj.Size)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionPathFactory(CodeBuilder builder, CompositionPath obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            var canvasGeometry = (Mgcg.CanvasGeometry)obj.Source;
            node.RequireWin2D = true;
            builder.WriteLine("CanvasGeometry geometry;");
            switch (canvasGeometry.Type)
            {
                case Mgcg.CanvasGeometry.GeometryType.Combination:
                    WriteCanvasGeometry_Combination(builder, (Mgcg.CanvasGeometry.Combination)canvasGeometry.Content);
                    break;
                case Mgcg.CanvasGeometry.GeometryType.Ellipse:
                    WriteCanvasGeometry_Ellipse(builder, (Mgcg.CanvasGeometry.Ellipse)canvasGeometry.Content);
                    break;
                case Mgcg.CanvasGeometry.GeometryType.Path:
                    WriteCanvasGeometry_Path(builder, (Mgcg.CanvasPathBuilder)canvasGeometry.Content);
                    break;
                case Mgcg.CanvasGeometry.GeometryType.RoundedRectangle:
                    WriteCanvasGeometry_RoundedRectangle(builder, (Mgcg.CanvasGeometry.RoundedRectangle)canvasGeometry.Content);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            builder.WriteLine($"var result = new CompositionPath(geometry);");
            WriteObjectFactoryEnd(builder);
            return true;
        }

        void WriteCanvasGeometry_Combination(CodeBuilder builder, Mgcg.CanvasGeometry.Combination obj)
        {
            var a = obj.A;
            var b = obj.B;
            var combineMode = obj.CombineMode;
            var matrix = obj.Matrix;
            // TODO - Need to create the geomtries, and give them separate names, and
            //        handle recursive combinations.
            throw new NotImplementedException();
        }

        void WriteCanvasGeometry_Ellipse(CodeBuilder builder, Mgcg.CanvasGeometry.Ellipse obj)
        {
            builder.WriteLine("geometry = CanvasGeometry.CreateEllipse(");
            builder.Indent();
            builder.WriteLine("CanvasDevice.GetSharedDevice(),");
            builder.WriteLine($"{Float(obj.X)},");
            builder.WriteLine($"{Float(obj.Y)},");
            builder.WriteLine($"{Float(obj.RadiusX)},");
            builder.WriteLine($"{Float(obj.RadiusY)});");
            builder.UnIndent();
        }

        void WriteCanvasGeometry_RoundedRectangle(CodeBuilder builder, Mgcg.CanvasGeometry.RoundedRectangle obj)
        {
            builder.WriteLine("geometry = CanvasGeometry.CreateRoundedRectnagle(");
            builder.Indent();
            builder.WriteLine("CanvasDevice.GetSharedDevice(),");
            builder.WriteLine($"{Float(obj.X)},");
            builder.WriteLine($"{Float(obj.Y)},");
            builder.WriteLine($"{Float(obj.W)},");
            builder.WriteLine($"{Float(obj.H)},");
            builder.WriteLine($"{Float(obj.RadiusX)},");
            builder.WriteLine($"{Float(obj.RadiusY)});");
            builder.UnIndent();
        }


        void WriteCanvasGeometry_Path(CodeBuilder builder, Mgcg.CanvasPathBuilder obj)
        {
            builder.WriteLine("using (var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice()))");
            builder.OpenScope();
            foreach (var command in obj.Commands)
            {
                switch (command.Type)
                {
                    case Mgcg.CanvasPathBuilder.CommandType.BeginFigure:
                        builder.WriteLine($"builder.BeginFigure({Vector2((Sn.Vector2)command.Args)});");
                        break;
                    case Mgcg.CanvasPathBuilder.CommandType.EndFigure:
                        builder.WriteLine($"builder.EndFigure({CanvasFigureLoop((Mgcg.CanvasFigureLoop)command.Args)});");
                        break;
                    case Mgcg.CanvasPathBuilder.CommandType.AddCubicBezier:
                        var vectors = (Sn.Vector2[])command.Args;
                        builder.WriteLine($"builder.AddCubicBezier({Vector2(vectors[0])}, {Vector2(vectors[1])}, {Vector2(vectors[2])});");
                        break;
                    case Mgcg.CanvasPathBuilder.CommandType.SetFilledRegionDetermination:
                        builder.WriteLine($"builder.SetFilledRegionDetermination({FilledRegionDetermination((Mgcg.CanvasFilledRegionDetermination)command.Args)});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            builder.WriteLine("geometry = CanvasGeometry.CreatePath(builder);");
            builder.CloseScope();
        }

        void WriteCreateAssignment(CodeBuilder builder, ObjectData node, string createCallText)
        {
            if (node.RequiresStorage)
            {
                var fieldName = node.FieldName;
                builder.WriteLine($"if ({fieldName} != null)");
                builder.OpenScope();
                builder.WriteLine($"return {fieldName};");
                builder.CloseScope();
                builder.WriteLine($"var result = {fieldName} = {createCallText};");
            }
            else
            {
                builder.WriteLine($"var result = {createCallText};");
            }
        }

        void WriteObjectFactoryExpressionBody(CodeBuilder builder, ObjectData node, string createCallText)
        {
            builder.WriteLine($"{node.TypeName} {node.GetCreateMethodName()}() => {createCallText};");
        }

        void WriteObjectFactoryStart(CodeBuilder builder, ObjectData node, IEnumerable<string> parameters = null)
        {
            builder.WriteLine($"{node.TypeName} {node.Name}({(parameters == null ? "" : string.Join(", ", parameters))})");
            builder.OpenScope();
        }

        void WriteObjectFactoryCreateAndEnd(CodeBuilder builder, string createCallText)
        {
            builder.WriteLine($"return {createCallText};");
            builder.CloseScope();
            builder.WriteLine();
        }

        void WriteObjectFactoryEnd(CodeBuilder builder)
        {
            builder.WriteLine("return result;");
            builder.CloseScope();
            builder.WriteLine();
        }

        // Find the nodes that are equivalent and point them all to a single canonical representation.
        static void Atomize(ObjectGraph<ObjectData> graph)
        {
            // Find all the ExpressionAnimation nodes.
            var expressionAnimations =
                from node in graph
                where node.Type == Graph.NodeType.CompositionObject
                let obj = (CompositionObject)node.Object
                where obj.Type == CompositionObjectType.ExpressionAnimation
                select node;

            // Group equivalent expression animations. The common case is expression animations
            // with a single reference parameter, so for simplicity only handle those.
            var groups =
                from exprAnim in expressionAnimations
                let ea = (ExpressionAnimation)exprAnim.Object
                where ea.ReferenceParameters.Count() == 1
                let expression = ea.Expression
                let target = ea.Target
                let parameter = ea.ReferenceParameters.First()
                let parameterName = parameter.Key
                group exprAnim by new { expression, target, parameterName } into groupedExpressions
                select groupedExpressions;

            // Point all of the equivalent expressions to the first expression in each group.
            foreach (var groupOfExpressions in groups)
            {
                var canonical = groupOfExpressions.First();

                // If the expression has different parameter values, it needs to be
                // turned into a method with a parameter. Discover that here.
                var parameterValueCount =
                    (from exprAnim in groupOfExpressions
                     let ea = (ExpressionAnimation)exprAnim.Object
                     from parameterValue in ea.ReferenceParameters.Select(a => a.Value)
                     select graph[parameterValue].Canonical).Distinct().Count();

                var needsMethodParameter = parameterValueCount > 1;
                if (!needsMethodParameter && groupOfExpressions.Count() > 1)
                {
                    // It doesn't take a parameter and it is referenced more than once. Ensure
                    // it is allocated storage.
                    canonical.ForceStorage();
                }
                foreach (var node in groupOfExpressions)
                {
                    node.Canonical = canonical;
                    node.NeedsParameter = needsMethodParameter;
                }
            }
        }

        static void SetCanonicalMethodNames(IEnumerable<ObjectData> canonicals)
        {
            var countersByType = new Dictionary<CompositionObjectType, int>();
            var pathCounter = 0;
            foreach (var node in canonicals)
            {
                switch (node.Type)
                {
                    case Graph.NodeType.CompositionObject:
                        var compObject = (CompositionObject)node.Object;
                        var compObjectType = compObject.Type;
                        if (!countersByType.TryGetValue(compObjectType, out var count))
                        {
                            countersByType.Add(compObjectType, count);
                        }
                        else
                        {
                            count++;
                            countersByType[compObjectType] = count;
                        }

                        node.SetName($"{compObject.Type.ToString()}_{count.ToString("0000")}");
                        break;
                    case Graph.NodeType.CompositionPath:
                        node.SetName($"CompositionPath_{pathCounter.ToString("0000")}");
                        pathCounter++;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        static string TimeSpan(TimeSpan value)
        {
            return $"TimeSpan.FromTicks({value.Ticks})";
        }

        static string Color(Color value) =>
            $"Color.FromArgb({Hex(value.A)}, {Hex(value.R)}, {Hex(value.G)}, {Hex(value.B)})";

        static string Hex(int value) => $"0x{value.ToString("X2")}";

        static string Float(float value)
        {
            if (Math.Floor(value) == value)
            {
                // Round numbers don't need decimal places or the F suffix.
                return value.ToString("0");
            }
            else
            {
                return value == 0 ? "0" : (value.ToString("0.0#########") + "F");
            }
        }

        static string String(string value) => $"\"{value}\"";

        static string Int(int value) => value.ToString();

        static string Bool(bool value) => value ? "true" : "false";

        static string CanvasFigureLoop(Mgcg.CanvasFigureLoop value)
        {
            switch (value)
            {
                case Mgcg.CanvasFigureLoop.Open:
                    return "CanvasFigureLoop.Open";
                case Mgcg.CanvasFigureLoop.Closed:
                    return "CanvasFigureLoop.Closed";
                default:
                    throw new InvalidOperationException();
            }
        }

        static string FilledRegionDetermination(
                Mgcg.CanvasFilledRegionDetermination value)
        {
            switch (value)
            {
                case Mgcg.CanvasFilledRegionDetermination.Alternate:
                    return "CanvasFilledRegionDetermination.Alternate";
                case Mgcg.CanvasFilledRegionDetermination.Winding:
                    return "CanvasFilledRegionDetermination.Winding";
                default:
                    throw new InvalidOperationException();
            }
        }

        static string StrokeLineJoin(CompositionStrokeLineJoin value)
        {
            switch (value)
            {
                case CompositionStrokeLineJoin.Miter:
                    return "CompositionStrokeLineJoin.Miter";
                case CompositionStrokeLineJoin.Bevel:
                    return "CompositionStrokeLineJoin.Bevel";
                case CompositionStrokeLineJoin.Round:
                    return "CompositionStrokeLineJoin.Round";
                case CompositionStrokeLineJoin.MiterOrBevel:
                    return "CompositionStrokeLineJoin.MiterOrBevel";
                default:
                    throw new InvalidOperationException();
            }
        }

        static string StrokeCap(CompositionStrokeCap value)
        {
            switch (value)
            {
                case CompositionStrokeCap.Flat:
                    return "CompositionStrokeCap.Flat";
                case CompositionStrokeCap.Square:
                    return "CompositionStrokeCap.Square";
                case CompositionStrokeCap.Round:
                    return "CompositionStrokeCap.Round";
                case CompositionStrokeCap.Triangle:
                    return "CompositionStrokeCap.Triangle";
                default:
                    throw new InvalidOperationException();
            }
        }

        static string Vector2(Vector2 value)
        {
            return $"new Vector2({ Float(value.X) }, { Float(value.Y)})";
        }

        static string Vector3(Vector3 value)
        {
            return $"new Vector3({ Float(value.X) }, { Float(value.Y)}, {Float(value.Z)})";
        }

        // A node in the object graph, annotated with extra stuff to assist in code generation.
        sealed class ObjectData : Graph.Node
        {
            string _name;
            string _fieldName;
            CodeBuilder _codeBuilder;
            bool _mustHaveStorage;

            public ObjectData()
            {
                Canonical = this;
            }

            public string Name => _name;

            public string FieldName => _fieldName;

            // Returns the number of times that GetCreateMethodName() was called.
            public int CreateMethodNameCallCounter { get; private set; }

            // Returns the name of the method that creates the object described by this node.
            // Also counts the number of calls. This is used to determine whether the
            // node is referenced by more than one other method.
            internal string GetCreateMethodName()
            {
                CreateMethodNameCallCounter++;
                return _name;
            }

            // True if the object is referenced from more than one method and
            // therefore must be stored after it is created.
            internal bool RequiresStorage => _mustHaveStorage;

            // Sets the name to be used for the node.
            internal void SetName(string name)
            {
                _name = name;
                // Generate a field name. Camel case.
                _fieldName = $"_{char.ToLowerInvariant(name[0])}{name.Substring(1)}";

            }

            // Set to indicate that the node relies on Win2D.
            internal bool RequireWin2D { get; set; }

            // Override the heuristic used to determine whether the object needs storage.
            internal void ForceStorage()
            {
                _mustHaveStorage = true;
            }

            // Gets a CodeBuilder for writing code associated with this node.
            internal CodeBuilder CodeBuilder
            {
                get
                {
                    // Lazy create because many nodes don't create code.
                    if (_codeBuilder == null)
                    {
                        _codeBuilder = new CodeBuilder();
                    }
                    return _codeBuilder;
                }
            }

            // An equivalent node to this. May be this.
            internal ObjectData Canonical { get; set; }

            // Set to true if the method for creating this object needs a parameter.
            internal bool NeedsParameter { get; set; }

            // The name of the type of the object described by this node.
            internal string TypeName
            {
                get
                {
                    switch (Type)
                    {
                        case Graph.NodeType.CompositionObject:
                            return ((CompositionObject)Object).Type.ToString();
                        case Graph.NodeType.CompositionPath:
                            return "CompositionPath";
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            public override string ToString() => _name;
        }
    }
}
