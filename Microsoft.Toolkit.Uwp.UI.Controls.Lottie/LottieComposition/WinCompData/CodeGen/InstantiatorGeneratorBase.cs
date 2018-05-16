using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinCompData.Mgcg;
using WinCompData.Sn;
using WinCompData.Tools;
using WinCompData.Wui;

namespace WinCompData.CodeGen
{
#if !WINDOWS_UWP
    public
#endif
    abstract class InstantiatorGeneratorBase
    {
        // The name of the field holding the singleton ExpressionAnimation.
        protected const string c_singletonExpressionAnimationName = "_expressionAnimation";
        readonly bool _setCommentProperties;
        readonly ObjectGraph<ObjectData> _objectGraph;
        // The subset of the object graph for which nodes will be generated.
        readonly ObjectData[] _canonicalNodes;
        readonly IStringifier _stringifier;

        internal InstantiatorGeneratorBase(CompositionObject graphRoot, bool setCommentProperties, IStringifier stringifier)
        {
            _setCommentProperties = setCommentProperties;
            _stringifier = stringifier;
            // Build the object graph.
            _objectGraph = ObjectGraph<ObjectData>.FromCompositionObject(graphRoot, includeVertices: true);

            // Canonicalize the nodes.
            Canonicalizer.Canonicalize(_objectGraph, !setCommentProperties);

            // Get the canonical nodes. These are the nodes for which code will be generated.

            // Filter out ExpressionAnimations that are unique. They will use a single instance that is reset on each use.
            var canonicals =
                from node in _objectGraph
                where !(node.Object is ExpressionAnimation) ||
                        node.NodesInGroup.Count() > 1
                select node.Canonical;

            // Filter out types for which we won't create objects:
            //  AnimationController is created implicitly.
            //  CompositionPropertySet is created implicitly.
            canonicals =
                (from node in canonicals
                 where node.Type != Graph.NodeType.CompositionObject ||
                     !(((CompositionObject)node.Object).Type == CompositionObjectType.AnimationController ||
                      ((CompositionObject)node.Object).Type == CompositionObjectType.CompositionPropertySet)
                 select node.Canonical).Distinct().ToArray();


            Debug.WriteLine($"Canonicalizer reduced object graph from {_objectGraph.Count()} to {canonicals.Count()}");
#if DEBUG
            // Get a list of the nodes that were elided through canonicalization.
            var canonicalized = _objectGraph.Where(n => !n.IsCanonical).ToArray();
#endif // DEBUG

            // Give names to each canonical node.
            SetCanonicalMethodNames(canonicals);

            // Save the canonical nodes, ordered by the name that was just set.
            _canonicalNodes = canonicals.OrderBy(node => node.Name).ToArray();

            // Force storage to be allocated for nodes that have multiple references to them.
            foreach (var node in canonicals)
            {
                if (FilteredCanonicalInRefs(node).Count() > 1)
                {
                    node.RequiresStorage = true;
                }
            }

            // Force inlining on CompositionPath nodes because they are always very simple.
            foreach (var node in canonicals.Where(node => node.Type == Graph.NodeType.CompositionPath))
            {
                if (node.CanonicalInRefs.Count() <= 1)
                {
                    var pathSourceFactoryCall =
                        NodeFor(((CompositionPath)node.Object).Source).FactoryCall();
                    node.ForceInline($"{New} CompositionPath({pathSourceFactoryCall})");
                }
            }

            // Ensure the root object has storage if it is referenced by anything else in the graph.
            // This is necessary because the root node is referenced from the instantiator entrypoint
            // but that isn't counted in the CanonicalInRefs.
            var rootNode = NodeFor(graphRoot);
            if (rootNode.CanonicalInRefs.Any())
            {
                rootNode.RequiresStorage = true;
            }
        }

        // Returns the canonical node for the given object.
        protected ObjectData NodeFor(object obj) => _objectGraph[obj].Canonical;

        // Gets the CanonicalInRefs for node, ignoring those from ExpressionAnimations
        // that have a single instance because they are treated specially (they are initialized inline).
        IEnumerable<ObjectData> FilteredCanonicalInRefs(ObjectData node)
        {
            // Examine all of the inrefs to the node.
            foreach (var item in node.CanonicalInRefs)
            {
                // If the inref is from an ExpressionAnimation ...
                if (item.Object is ExpressionAnimation exprAnim)
                {
                    // ... is the animation shared?
                    if (item.NodesInGroup.Count() > 1)
                    {
                        yield return item;
                        continue;
                    }

                    // ... is the anmation animating a property on the current node or its property set.
                    bool isExpressionOnThisNode = false;

                    var compObject = node.Object as CompositionObject;
                    // Search the animators to find the animator for this ExpressionAnimation.
                    // It will be found iff the ExpressionAnimation is animating this node.
                    foreach (var animator in compObject.Animators.Concat(compObject.Properties.Animators))
                    {
                        if (animator.Animation is ExpressionAnimation animatorExpression &&
                            animatorExpression.Expression == exprAnim.Expression)
                        {
                            isExpressionOnThisNode = true;
                            break;
                        }
                    }

                    if (!isExpressionOnThisNode)
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }

        // Override to generate the using namespace statements at the top of the file.
        protected abstract void WritePreamble(CodeBuilder builder, bool requiresD2d);

        protected abstract void WriteClassStart(
            CodeBuilder builder,
            string className,
            Vector2 size,
            CompositionPropertySet progressPropertySet,
            TimeSpan duration);

        protected abstract void WriteField(CodeBuilder builder, string typeName, string fieldName);

        protected abstract void WriteClassEnd(CodeBuilder builder, Visual rootVisual);


        /// <summary>
        /// Call this to generate the code. Returns a string containing the generated code.
        /// </summary>
        protected string GenerateCode(
            string className,
            Visual rootVisual,
            float width,
            float height,
            CompositionPropertySet progressPropertySet,
            TimeSpan duration)
        {
            var codeBuilder = new CodeBuilder();

            // Generate #includes and usings for namespaces.
            var requiresWin2D = _canonicalNodes.Where(n => n.RequiresWin2D).Any();

            WritePreamble(codeBuilder, requiresWin2D);

            WriteClassStart(codeBuilder, className, new Vector2(width, height), progressPropertySet, duration);

            // Write fields for each object that needs storage (i.e. objects that are 
            // referenced more than once).
            foreach (var node in _canonicalNodes)
            {
                if (node.RequiresStorage)
                {
                    // Generate a field for the storage.
                    WriteField(codeBuilder, node.TypeName, node.FieldName);
                }
            }
            codeBuilder.WriteLine();

            // Write methods for each node.
            foreach (var node in _canonicalNodes)
            {
                WriteCodeForNode(codeBuilder, node);
            }

            WriteClassEnd(codeBuilder, rootVisual);
            return codeBuilder.ToString();
        }

        // Generates code for the given node. The code is written into the CodeBuilder on the node.
        void WriteCodeForNode(CodeBuilder builder, ObjectData node)
        {
            // Only generate if the node is not inlined into the caller.
            if (!node.Inlined)
            {
                switch (node.Type)
                {
                    case Graph.NodeType.CompositionObject:
                        GenerateObjectFactory(builder, (CompositionObject)node.Object, node);
                        break;
                    case Graph.NodeType.CompositionPath:
                        GenerateCompositionPathFactory(builder, (CompositionPath)node.Object, node);
                        break;
                    case Graph.NodeType.CanvasGeometry:
                        GenerateCanvasGeometryFactory(builder, (CanvasGeometry)node.Object, node);
                        return;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        bool GenerateCanvasGeometryFactory(CodeBuilder builder, CanvasGeometry obj, ObjectData node)
        {
            switch (obj.Type)
            {
                case CanvasGeometry.GeometryType.Combination:
                    return GenerateCanvasGeometryCombinationFactory(builder, (CanvasGeometry.Combination)obj, node);
                case CanvasGeometry.GeometryType.Ellipse:
                    return GenerateCanvasGeometryEllipseFactory(builder, (CanvasGeometry.Ellipse)obj, node);
                case CanvasGeometry.GeometryType.Path:
                    return GenerateCanvasGeometryPathFactory(builder, (CanvasGeometry.Path)obj, node);
                case CanvasGeometry.GeometryType.RoundedRectangle:
                    return GenerateCanvasGeometryRoundedRectangleFactory(builder, (CanvasGeometry.RoundedRectangle)obj, node);
                default:
                    throw new InvalidOperationException();
            }
        }

        bool GenerateCanvasGeometryCombinationFactory(CodeBuilder builder, CanvasGeometry.Combination obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            if (node.RequiresStorage)
            {
                WriteCacheHandler(builder, node);
            }
            var a = NodeFor(obj.A);
            var b = NodeFor(obj.B);
            builder.WriteLine($"{Var} result = {(node.RequiresStorage ? $" {node.FieldName} = " : "")}{a.FactoryCall()}.");
            builder.Indent();
            builder.WriteLine($"CombineWith({b.FactoryCall()},");
            if (obj.Matrix.IsIdentity)
            {
                builder.WriteLine("Matrix3x2.Identity,");
            }
            else
            {
                builder.WriteLine($"{Matrix3x2(obj.Matrix)},");
            }
            builder.WriteLine($"{CanvasGeometryCombine(obj.CombineMode)});");
            builder.UnIndent();
            WriteObjectFactoryEnd(builder);
            return true;
        }

        protected virtual bool GenerateCanvasGeometryPathFactory(CodeBuilder builder, CanvasGeometry.Path obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            if (node.RequiresStorage)
            {
                WriteCacheHandler(builder, node);
            }
            builder.WriteLine($"using (var builder = {New} CanvasPathBuilder({Null}))");
            builder.OpenScope();
            foreach (var command in obj.Commands)
            {
                switch (command.Type)
                {
                    case CanvasPathBuilder.CommandType.BeginFigure:
                        builder.WriteLine($"builder{Deref}BeginFigure({Vector2((Vector2)command.Args)});");
                        break;
                    case CanvasPathBuilder.CommandType.EndFigure:
                        builder.WriteLine($"builder{Deref}EndFigure({CanvasFigureLoop((CanvasFigureLoop)command.Args)});");
                        break;
                    case CanvasPathBuilder.CommandType.AddCubicBezier:
                        var vectors = (Vector2[])command.Args;
                        builder.WriteLine($"builder{Deref}AddCubicBezier({Vector2(vectors[0])}, {Vector2(vectors[1])}, {Vector2(vectors[2])});");
                        break;
                    case CanvasPathBuilder.CommandType.SetFilledRegionDetermination:
                        builder.WriteLine($"builder{Deref}SetFilledRegionDetermination({FilledRegionDetermination((CanvasFilledRegionDetermination)command.Args)});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            builder.WriteLine($"return {(node.RequiresStorage ? $" {node.FieldName} = " : "")}CanvasGeometry{Deref}CreatePath(builder);");
            builder.CloseScope();
            builder.CloseScope();
            builder.WriteLine();
            return true;
        }

        bool GenerateCanvasGeometryEllipseFactory(CodeBuilder builder, CanvasGeometry.Ellipse obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            if (node.RequiresStorage)
            {
                WriteCacheHandler(builder, node);
            }
            builder.WriteLine($"{Var} result = {(node.RequiresStorage ? $" {node.FieldName} " : "")}CanvasGeometry{Deref}CreateEllipse(");
            builder.Indent();
            builder.WriteLine($"{Null},");
            builder.WriteLine($"{Float(obj.X)},");
            builder.WriteLine($"{Float(obj.Y)},");
            builder.WriteLine($"{Float(obj.RadiusX)},");
            builder.WriteLine($"{Float(obj.RadiusY)};");
            builder.UnIndent();
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCanvasGeometryRoundedRectangleFactory(CodeBuilder builder, CanvasGeometry.RoundedRectangle obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            if (node.RequiresStorage)
            {
                WriteCacheHandler(builder, node);
            }
            builder.WriteLine($"{Var} result = {(node.RequiresStorage ? $" {node.FieldName} " : "")}CanvasGeometry{Deref}CreateRoundedRectangle(");
            builder.Indent();
            builder.WriteLine($"{Null},");
            builder.WriteLine($"{Float(obj.X)},");
            builder.WriteLine($"{Float(obj.Y)},");
            builder.WriteLine($"{Float(obj.W)},");
            builder.WriteLine($"{Float(obj.H)},");
            builder.WriteLine($"{Float(obj.RadiusX)},");
            builder.WriteLine($"{Float(obj.RadiusY)};");
            builder.UnIndent();
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateObjectFactory(CodeBuilder builder, CompositionObject obj, ObjectData node)
        {
            switch (obj.Type)
            {
                case CompositionObjectType.AnimationController:
                    // Do not generate code for animation controllers. It is done inline in the CompositionObject initialization.
                    throw new InvalidOperationException();
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
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateInsetClip()");
            InitializeCompositionClip(builder, obj);
            if (obj.LeftInset != 0)
            {
                builder.WriteLine($"result{Deref}LeftInset = {Float(obj.LeftInset)}");
            }
            if (obj.RightInset != 0)
            {
                builder.WriteLine($"result{Deref}RightInset = {Float(obj.RightInset)}");
            }
            if (obj.TopInset != 0)
            {
                builder.WriteLine($"result{Deref}TopInset = {Float(obj.TopInset)}");
            }
            if (obj.BottomInset != 0)
            {
                builder.WriteLine($"result{Deref}BottomInset = {Float(obj.BottomInset)}");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateLinearEasingFunctionFactory(CodeBuilder builder, LinearEasingFunction obj, ObjectData node)
        {
            WriteSimpleObjectFactory(builder, node, $"_c{Deref}CreateLinearEasingFunction()");
            return true;
        }

        bool GenerateCubicBezierEasingFunctionFactory(CodeBuilder builder, CubicBezierEasingFunction obj, ObjectData node)
        {
            WriteSimpleObjectFactory(builder, node, $"_c{Deref}CreateCubicBezierEasingFunction({Vector2(obj.ControlPoint1)}, {Vector2(obj.ControlPoint2)})");
            return true;
        }

        bool GenerateStepEasingFunctionFactory(CodeBuilder builder, StepEasingFunction obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateStepEasingFunction()");
            if (obj.FinalStep != 1)
            {
                builder.WriteLine($"result{Deref}FinalStep = {Int(obj.FinalStep)};");
            }
            if (obj.InitialStep != 0)
            {
                builder.WriteLine($"result{Deref}InitialStep = {Int(obj.InitialStep)};");
            }
            if (obj.IsFinalStepSingleFrame)
            {
                builder.WriteLine($"result{Deref}IsFinalStepSingleFrame  = {Bool(obj.IsFinalStepSingleFrame)};");
            }
            if (obj.IsInitialStepSingleFrame)
            {
                builder.WriteLine($"result{Deref}IsInitialStepSingleFrame  = {Bool(obj.IsInitialStepSingleFrame)};");
            }
            if (obj.StepCount != 1)
            {
                builder.WriteLine($"result{Deref}StepCount = {Int(obj.StepCount)};");
            }
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateContainerVisualFactory(CodeBuilder builder, ContainerVisual obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateContainerVisual()");
            InitializeContainerVisual(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateExpressionAnimationFactory(CodeBuilder builder, ExpressionAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateExpressionAnimation()");
            InitializeCompositionAnimation(builder, obj);
            builder.WriteLine($"result{Deref}Expression = {String(obj.Expression)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        void StartAnimations(CodeBuilder builder, CompositionObject obj, string localName = "result", string animationNamePrefix = "")
        {
            var animators = obj.Properties.Animators.Concat(obj.Animators);
            bool controllerVariableAdded = false;
            foreach (var animator in animators)
            {
                // ExpressionAnimations are treated specially - a singleton
                // ExpressionAnimation is reset before each use, unless the animation
                // is shared.
                var anim = NodeFor(animator.Animation);
                if (anim.NodesInGroup.Count() == 1 && animator.Animation is ExpressionAnimation expressionAnimation)
                {
                    builder.WriteLine($"{c_singletonExpressionAnimationName}{Deref}ClearAllParameters();");
                    builder.WriteLine($"{c_singletonExpressionAnimationName}{Deref}Expression = {String(expressionAnimation.Expression)};");
                    // If there is a Target set it. Note however that the Target isn't used for anything
                    // interesting in this scenario, and there is no way to reset the Target to an
                    // empty string (the Target API disallows empty). In reality, for all our uses
                    // the Target will not be set and it doesn't matter if it was set previously.
                    if (!string.IsNullOrWhiteSpace(expressionAnimation.Target))
                    {
                        builder.WriteLine($"{c_singletonExpressionAnimationName}{Deref}Target = {String(expressionAnimation.Target)};");
                    }
                    foreach (var rp in expressionAnimation.ReferenceParameters)
                    {
                        var referenceParamenterValueName = rp.Value == obj
                            ? localName
                            : $"{NodeFor(rp.Value).FactoryCall()}";
                        builder.WriteLine($"{c_singletonExpressionAnimationName}{Deref}SetReferenceParameter({String(rp.Key)}, {referenceParamenterValueName});");
                    }
                    builder.WriteLine($"{localName}{Deref}StartAnimation({String(animator.AnimatedProperty)}, {c_singletonExpressionAnimationName});");
                }
                else
                {
                    // KeyFrameAnimation or shared animation
                    builder.WriteLine($"{localName}{Deref}StartAnimation({String(animator.AnimatedProperty)}, {anim.FactoryCall()});");
                }

                if (animator.Controller != null)
                {
                    if (!controllerVariableAdded)
                    {
                        // Declare and initialize the controller variable.
                        builder.WriteLine($"{Var} controller = {localName}{Deref}TryGetAnimationController({String(animator.AnimatedProperty)});");
                        controllerVariableAdded = true;
                    }
                    else
                    {
                        // Initialize the controller variable.
                        builder.WriteLine($"controller = {localName}{Deref}TryGetAnimationController({String(animator.AnimatedProperty)});");
                    }
                    // TODO - we always pause here, but really should only pause it if WinCompData does it. 
                    builder.WriteLine($"controller{Deref}Pause();");
                    // Recurse to start animations on the controller.
                    StartAnimations(builder, animator.Controller, "controller", "controller");
                }
            }
        }

        void InitializeCompositionObject(CodeBuilder builder, CompositionObject obj, string localName = "result", string animationNamePrefix = "")
        {
            if (_setCommentProperties)
            {
                if (!string.IsNullOrWhiteSpace(obj.Comment))
                {
                    builder.WriteLine($"{localName}{Deref}Comment = {String(obj.Comment)};");
                }
            }

            var propertySet = obj.Properties;
            if (propertySet.PropertyNames.Any())
            {
                builder.WriteLine($"{Var} propertySet = {localName}{Deref}Properties;");
                foreach (var prop in propertySet.ScalarProperties)
                {
                    builder.WriteLine($"propertySet{Deref}InsertScalar({String(prop.Key)}, {Float(prop.Value)});");
                }

                foreach (var prop in propertySet.Vector2Properties)
                {
                    builder.WriteLine($"propertySet{Deref}InsertVector2({String(prop.Key)}, {Vector2(prop.Value)});");
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
                builder.WriteLine($"result{Deref}CenterPoint = {Vector3(obj.CenterPoint.Value)};");
            }
            if (obj.Clip != null)
            {
                builder.WriteLine($"result{Deref}Clip = {NodeFor(obj.Clip).FactoryCall()};");
            }
            if (obj.Offset.HasValue)
            {
                builder.WriteLine($"result{Deref}Offset = {Vector3(obj.Offset.Value)};");
            }
            if (obj.RotationAngleInDegrees.HasValue)
            {
                builder.WriteLine($"result{Deref}RotationAngleInDegrees = {Float(obj.RotationAngleInDegrees.Value)};");
            }
            if (obj.Scale.HasValue)
            {
                builder.WriteLine($"result{Deref}Scale = {Vector3(obj.Scale.Value)};");
            }
            if (obj.Size.HasValue)
            {
                builder.WriteLine($"result{Deref}Size = {Vector2(obj.Size.Value)};");
            }
        }

        void InitializeCompositionClip(CodeBuilder builder, CompositionClip obj)
        {
            InitializeCompositionObject(builder, obj);

            if (obj.CenterPoint.X != 0 || obj.CenterPoint.Y != 0)
            {
                builder.WriteLine($"result{Deref}CenterPoint = {Vector2(obj.CenterPoint)};");
            }
            if (obj.Scale.X != 1 || obj.Scale.Y != 1)
            {
                builder.WriteLine($"result{Deref}Scale = {Vector2(obj.Scale)};");
            }
        }

        void InitializeCompositionShape(CodeBuilder builder, CompositionShape obj)
        {
            InitializeCompositionObject(builder, obj);

            if (obj.CenterPoint.HasValue)
            {
                builder.WriteLine($"result{Deref}CenterPoint = {Vector2(obj.CenterPoint.Value)};");
            }
            if (obj.Offset != null)
            {
                builder.WriteLine($"result{Deref}Offset = {Vector2(obj.Offset.Value)};");
            }
            if (obj.RotationAngleInDegrees.HasValue)
            {
                builder.WriteLine($"result{Deref}RotationAngleInDegrees = {Float(obj.RotationAngleInDegrees.Value)};");
            }
            if (obj.Scale.HasValue)
            {
                builder.WriteLine($"result{Deref}Scale = {Vector2(obj.Scale.Value)};");
            }
        }

        void InitializeContainerVisual(CodeBuilder builder, ContainerVisual obj)
        {
            InitializeVisual(builder, obj);

            if (obj.Children.Any())
            {
                builder.WriteLine($"{Var} children = result{Deref}Children;");
                foreach (var child in obj.Children)
                {
                    builder.WriteLine($"children{Deref}InsertAtTop({NodeFor(child).FactoryCall()});");
                }
            }
        }

        void InitializeCompositionGeometry(CodeBuilder builder, CompositionGeometry obj)
        {
            InitializeCompositionObject(builder, obj);
            if (obj.TrimEnd != 1)
            {
                builder.WriteLine($"result{Deref}TrimEnd = {Float(obj.TrimEnd)};");
            }
            if (obj.TrimOffset != 0)
            {
                builder.WriteLine($"result{Deref}TrimOffset = {Float(obj.TrimOffset)};");
            }
            if (obj.TrimStart != 0)
            {
                builder.WriteLine($"result{Deref}TrimStart = {Float(obj.TrimStart)};");
            }
        }

        void InitializeCompositionAnimation(CodeBuilder builder, CompositionAnimation obj)
        {
            InitializeCompositionAnimationWithParameters(
                builder,
                obj,
                obj.ReferenceParameters.Select(p => new KeyValuePair<string, string>(p.Key, $"{NodeFor(p.Value).FactoryCall()}")));
        }

        void InitializeCompositionAnimationWithParameters(CodeBuilder builder, CompositionAnimation obj, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            InitializeCompositionObject(builder, obj);
            if (!string.IsNullOrWhiteSpace(obj.Target))
            {
                builder.WriteLine($"result{Deref}Target = {String(obj.Target)};");
            }
            foreach (var parameter in parameters)
            {
                builder.WriteLine($"result{Deref}SetReferenceParameter({String(parameter.Key)}, {parameter.Value});");
            }
        }

        void InitializeKeyFrameAnimation(CodeBuilder builder, KeyFrameAnimation_ obj)
        {
            InitializeCompositionAnimation(builder, obj);
            builder.WriteLine($"result{Deref}Duration = {TimeSpan(obj.Duration)};");
        }

        bool GenerateColorKeyFrameAnimationFactory(CodeBuilder builder, ColorKeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateColorKeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                switch (kf.Type)
                {
                    case KeyFrameAnimation<Color>.KeyFrameType.Expression:
                        var expressionKeyFrame = (KeyFrameAnimation<Color>.ExpressionKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertExpressionKeyFrame({Float(kf.Progress)}, {String(expressionKeyFrame.Expression)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    case KeyFrameAnimation<Color>.KeyFrameType.Value:
                        var valueKeyFrame = (KeyFrameAnimation<Color>.ValueKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertKeyFrame({Float(kf.Progress)}, {Color(valueKeyFrame.Value)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateVector2KeyFrameAnimationFactory(CodeBuilder builder, Vector2KeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateVector2KeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                switch (kf.Type)
                {
                    case KeyFrameAnimation<Vector2>.KeyFrameType.Expression:
                        var expressionKeyFrame = (KeyFrameAnimation<Vector2>.ExpressionKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertExpressionKeyFrame({Float(kf.Progress)}, {String(expressionKeyFrame.Expression)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    case KeyFrameAnimation<Vector2>.KeyFrameType.Value:
                        var valueKeyFrame = (KeyFrameAnimation<Vector2>.ValueKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertKeyFrame({Float(kf.Progress)}, {Vector2(valueKeyFrame.Value)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateVector3KeyFrameAnimationFactory(CodeBuilder builder, Vector3KeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateVector3KeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                switch (kf.Type)
                {
                    case KeyFrameAnimation<Vector3>.KeyFrameType.Expression:
                        var expressionKeyFrame = (KeyFrameAnimation<Vector3>.ExpressionKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertExpressionKeyFrame({Float(kf.Progress)}, {String(expressionKeyFrame.Expression)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    case KeyFrameAnimation<Vector3>.KeyFrameType.Value:
                        var valueKeyFrame = (KeyFrameAnimation<Vector3>.ValueKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertKeyFrame({Float(kf.Progress)}, {Vector3(valueKeyFrame.Value)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GeneratePathKeyFrameAnimationFactory(CodeBuilder builder, PathKeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreatePathKeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                var path = NodeFor(((PathKeyFrameAnimation.ValueKeyFrame)kf).Value);
                builder.WriteLine($"result{Deref}InsertKeyFrame({Float(kf.Progress)}, {path.FactoryCall()}, {NodeFor(kf.Easing).FactoryCall()});");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }


        bool GenerateScalarKeyFrameAnimationFactory(CodeBuilder builder, ScalarKeyFrameAnimation obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateScalarKeyFrameAnimation()");
            InitializeKeyFrameAnimation(builder, obj);

            foreach (var kf in obj.KeyFrames)
            {
                switch (kf.Type)
                {
                    case KeyFrameAnimation<float>.KeyFrameType.Expression:
                        var expressionKeyFrame = (KeyFrameAnimation<float>.ExpressionKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertExpressionKeyFrame({Float(kf.Progress)}, {String(expressionKeyFrame.Expression)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    case KeyFrameAnimation<float>.KeyFrameType.Value:
                        var valueKeyFrame = (KeyFrameAnimation<float>.ValueKeyFrame)kf;
                        builder.WriteLine($"result{Deref}InsertKeyFrame({Float(kf.Progress)}, {Float(valueKeyFrame.Value)}, {NodeFor(kf.Easing).FactoryCall()});");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionRectangleGeometryFactory(CodeBuilder builder, CompositionRectangleGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateRectangleGeometry()");
            InitializeCompositionGeometry(builder, obj);
            builder.WriteLine($"result{Deref}Size = {Vector2(obj.Size)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionRoundedRectangleGeometryFactory(CodeBuilder builder, CompositionRoundedRectangleGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateRoundedRectangleGeometry()");
            InitializeCompositionGeometry(builder, obj);
            builder.WriteLine($"result{Deref}CornerRadius = {Vector2(obj.CornerRadius)};");
            builder.WriteLine($"result{Deref}Size = {Vector2(obj.Size)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionEllipseGeometryFactory(CodeBuilder builder, CompositionEllipseGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateEllipseGeometry()");
            InitializeCompositionGeometry(builder, obj);
            if (obj.Center.X != 0 || obj.Center.Y != 0)
            {
                builder.WriteLine($"result{Deref}Center = {Vector2(obj.Center)};");
            }
            builder.WriteLine($"result{Deref}Radius = {Vector2(obj.Radius)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionPathGeometryFactory(CodeBuilder builder, CompositionPathGeometry obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            var path = NodeFor(obj.Path);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreatePathGeometry({path.FactoryCall()})");
            InitializeCompositionGeometry(builder, obj);
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionColorBrushFactory(CodeBuilder builder, CompositionColorBrush obj, ObjectData node)
        {
            var createCallText = $"_c{Deref}CreateColorBrush({Color(obj.Color)})";
            if (obj.Animators.Any() || node.RequiresStorage)
            {
                WriteObjectFactoryStart(builder, node);
                WriteCreateAssignment(builder, node, $"_c{Deref}CreateColorBrush({Color(obj.Color)})");
                InitializeCompositionBrush(builder, obj);
                StartAnimations(builder, obj);
                WriteObjectFactoryEnd(builder);
            }
            else
            {
                WriteSimpleObjectFactory(builder, node, createCallText);
            }
            return true;
        }

        bool GenerateShapeVisualFactory(CodeBuilder builder, ShapeVisual obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateShapeVisual()");
            InitializeContainerVisual(builder, obj);

            if (obj.Shapes.Any())
            {
                builder.WriteLine($"{Var} shapes = result{Deref}Shapes;");
                foreach (var shape in obj.Shapes)
                {
                    builder.WriteLine($"shapes{Deref}{IListAdd}({NodeFor(shape).FactoryCall()});");
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateContainerShapeFactory(CodeBuilder builder, CompositionContainerShape obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateContainerShape()");
            InitializeCompositionShape(builder, obj);
            if (obj.Shapes.Any())
            {
                builder.WriteLine($"{Var} shapes = result{Deref}Shapes;");
                foreach (var shape in obj.Shapes)
                {
                    builder.WriteLine($"shapes{Deref}{IListAdd}({NodeFor(shape).FactoryCall()});");
                }
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateSpriteShapeFactory(CodeBuilder builder, CompositionSpriteShape obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateSpriteShape()");
            InitializeCompositionShape(builder, obj);

            if (obj.FillBrush != null)
            {
                builder.WriteLine($"result{Deref}FillBrush = {NodeFor(obj.FillBrush).FactoryCall()};");
            }
            if (obj.Geometry != null)
            {
                builder.WriteLine($"result{Deref}Geometry = {NodeFor(obj.Geometry).FactoryCall()};");
            }
            if (obj.IsStrokeNonScaling)
            {
                builder.WriteLine("result{Deref}IsStrokeNonScaling = true;");
            }
            if (obj.StrokeBrush != null)
            {
                builder.WriteLine($"result{Deref}StrokeBrush = {NodeFor(obj.StrokeBrush).FactoryCall()};");
            }
            if (obj.StrokeDashCap != CompositionStrokeCap.Flat)
            {
                builder.WriteLine($"result{Deref}StrokeDashCap = {StrokeCap(obj.StrokeDashCap)};");
            }
            if (obj.StrokeDashOffset != 0)
            {
                builder.WriteLine($"result{Deref}StrokeDashOffset = {Float(obj.StrokeDashOffset)};");
            }
            if (obj.StrokeDashArray.Count > 0)
            {
                builder.WriteLine($"{Var} strokeDashArray = result{Deref}StrokeDashArray;");
                foreach (var strokeDash in obj.StrokeDashArray)
                {
                    builder.WriteLine($"strokeDashArray{Deref}Add({Float(strokeDash)});");
                }
            }
            if (obj.StrokeEndCap != CompositionStrokeCap.Flat)
            {
                builder.WriteLine($"result{Deref}StrokeEndCap = {StrokeCap(obj.StrokeEndCap)};");
            }
            if (obj.StrokeLineJoin != CompositionStrokeLineJoin.Miter)
            {
                builder.WriteLine($"result{Deref}StrokeLineJoin = {StrokeLineJoin(obj.StrokeLineJoin)};");
            }
            if (obj.StrokeStartCap != CompositionStrokeCap.Flat)
            {
                builder.WriteLine($"result{Deref}StrokeStartCap = {StrokeCap(obj.StrokeStartCap)};");
            }
            if (obj.StrokeMiterLimit != 1)
            {
                builder.WriteLine($"result{Deref}StrokeMiterLimit = {Float(obj.StrokeMiterLimit)};");
            }
            if (obj.StrokeThickness != 1)
            {
                builder.WriteLine($"result{Deref}StrokeThickness = {Float(obj.StrokeThickness)};");
            }
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionViewBoxFactory(CodeBuilder builder, CompositionViewBox obj, ObjectData node)
        {
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"_c{Deref}CreateViewBox()");
            InitializeCompositionObject(builder, obj);
            builder.WriteLine($"result.Size = {Vector2(obj.Size)};");
            StartAnimations(builder, obj);
            WriteObjectFactoryEnd(builder);
            return true;
        }

        bool GenerateCompositionPathFactory(CodeBuilder builder, CompositionPath obj, ObjectData node)
        {
            var canvasGeometry = NodeFor((CanvasGeometry)obj.Source);
            WriteObjectFactoryStart(builder, node);
            WriteCreateAssignment(builder, node, $"{New} CompositionPath({canvasGeometry.FactoryCall()})");
            WriteObjectFactoryEnd(builder);
            return true;
        }

        protected void WriteCacheHandler(CodeBuilder builder, ObjectData node)
        {
            var fieldName = node.FieldName;
            builder.WriteLine($"if ({fieldName} != {Null})");
            builder.OpenScope();
            builder.WriteLine($"return {fieldName};");
            builder.CloseScope();
        }

        void WriteCreateAssignment(CodeBuilder builder, ObjectData node, string createCallText)
        {
            if (node.RequiresStorage)
            {
                var fieldName = node.FieldName;
                WriteCacheHandler(builder, node);
                builder.WriteLine($"{Var} result = {fieldName} = {createCallText};");
            }
            else
            {
                builder.WriteLine($"{Var} result = {createCallText};");
            }
        }

        // Handles object factories that are just a create call.
        void WriteSimpleObjectFactory(CodeBuilder builder, ObjectData node, string createCallText)
        {
            WriteObjectFactoryStart(builder, node);
            if (node.RequiresStorage)
            {
                WriteCacheHandler(builder, node);
                builder.WriteLine($"return {node.FieldName} = {createCallText};");
            }
            else
            {
                builder.WriteLine($"return {createCallText};");
            }
            builder.CloseScope();
            builder.WriteLine();
        }

        protected virtual void WriteObjectFactoryStart(CodeBuilder builder, ObjectData node, IEnumerable<string> parameters = null)
        {
            builder.WriteLine($"{node.TypeName} {node.Name}({(parameters == null ? "" : string.Join(", ", parameters))})");
            builder.OpenScope();
        }

        void WriteObjectFactoryEnd(CodeBuilder builder)
        {
            builder.WriteLine("return result;");
            builder.CloseScope();
            builder.WriteLine();
        }

        static void SetCanonicalMethodNames(IEnumerable<ObjectData> canonicals)
        {
            var countersByType = new Dictionary<CompositionObjectType, int>();
            var pathCounter = 0;
            var canvasGeometryCounter = 0;
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

                        node.Name = $"{compObject.Type.ToString()}_{count.ToString("0000")}";
                        break;
                    case Graph.NodeType.CompositionPath:
                        node.Name = $"CompositionPath_{pathCounter.ToString("0000")}";
                        pathCounter++;
                        break;
                    case Graph.NodeType.CanvasGeometry:
                        node.Name = $"CanvasGeometry_{canvasGeometryCounter.ToString("0000")}";
                        canvasGeometryCounter++;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        string Deref => _stringifier.Deref;

        string New => _stringifier.New;

        string Null => _stringifier.Null;

        string ScopeResolve => _stringifier.ScopeResolve;

        string Var => _stringifier.Var;

        string Bool(bool value) => _stringifier.Bool(value);

        string Color(Color value) => _stringifier.Color(value);

        string IListAdd => _stringifier.IListAdd;

        string CanvasFigureLoop(CanvasFigureLoop value) => _stringifier.CanvasFigureLoop(value);

        string CanvasGeometryCombine(CanvasGeometryCombine value) => _stringifier.CanvasGeometryCombine(value);

        string FilledRegionDetermination(CanvasFilledRegionDetermination value) => _stringifier.FilledRegionDetermination(value);

        string Float(float value) => _stringifier.Float(value);

        string Int(int value) => _stringifier.Int(value);

        string Matrix3x2(Matrix3x2 value) => _stringifier.Matrix3x2(value);

        string String(string value) => _stringifier.String(value);

        string StrokeCap(CompositionStrokeCap value)
        {
            switch (value)
            {
                case CompositionStrokeCap.Flat:
                    return $"CompositionStrokeCap{ScopeResolve}Flat";
                case CompositionStrokeCap.Square:
                    return $"CompositionStrokeCap{ScopeResolve}Square";
                case CompositionStrokeCap.Round:
                    return $"CompositionStrokeCap{ScopeResolve}Round";
                case CompositionStrokeCap.Triangle:
                    return $"CompositionStrokeCap{ScopeResolve}Triangle";
                default:
                    throw new InvalidOperationException();
            }
        }

        string StrokeLineJoin(CompositionStrokeLineJoin value)
        {
            switch (value)
            {
                case CompositionStrokeLineJoin.Miter:
                    return $"CompositionStrokeLineJoin{ScopeResolve}Miter";
                case CompositionStrokeLineJoin.Bevel:
                    return $"CompositionStrokeLineJoin{ScopeResolve}Bevel";
                case CompositionStrokeLineJoin.Round:
                    return $"CompositionStrokeLineJoin{ScopeResolve}Round";
                case CompositionStrokeLineJoin.MiterOrBevel:
                    return $"CompositionStrokeLineJoin{ScopeResolve}MiterOrBevel";
                default:
                    throw new InvalidOperationException();
            }
        }

        string TimeSpan(TimeSpan value) => _stringifier.TimeSpan(value);

        string Vector2(Vector2 value) => _stringifier.Vector2(value);

        string Vector3(Vector3 value) => _stringifier.Vector3(value);

        // Provides language-specific string representations of a value.
        protected internal interface IStringifier
        {
            string Bool(bool value);
            string CanvasFigureLoop(CanvasFigureLoop value);
            string CanvasGeometryCombine(CanvasGeometryCombine value);

            string Color(Color value);
            string Deref { get; }
            string FilledRegionDetermination(CanvasFilledRegionDetermination value);
            string Float(float value);
            string IListAdd { get; }
            string Int(int value);
            string MemberSelect { get; }
            string New { get; }
            string Null { get; }
            string ScopeResolve { get; }
            string Matrix3x2(Matrix3x2 value);
            string String(string value);
            string TimeSpan(TimeSpan value);
            string Var { get; }
            string Vector2(Vector2 value);
            string Vector3(Vector3 value);
        }

        // A node in the object graph, annotated with extra stuff to assist in code generation.
        protected sealed class ObjectData : CanonicalizedNode<ObjectData>
        {
            string _overriddenFactoryCall;

            public string Name { get; set; }

            public string FieldName => CamelCase(Name);

            // Returns text for obtaining the value for this node. If the node has
            // been inlined, this can generate the code into the returned string, otherwise
            // it returns code for calling the factory.
            internal string FactoryCall()
            {
                if (Inlined)
                {
                    return _overriddenFactoryCall;
                }
                else
                {
                    return $"{Name}()";
                }
            }

            // True if the object is referenced from more than one method and
            // therefore must be stored after it is created.
            internal bool RequiresStorage { get; set; }

            // Set to indicate that the node relies on Win2D / D2D.
            internal bool RequiresWin2D => Object is CanvasGeometry;

            // True if the code to create the object will be generated inline.
            internal bool Inlined => _overriddenFactoryCall != null;

            internal void ForceInline(string replacementFactoryCall)
            {
                _overriddenFactoryCall = replacementFactoryCall;
            }

            // The name of the type of the object described by this node.
            // This is the name used as the return type of a factory method.
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
                        case Graph.NodeType.CanvasGeometry:
                            return "CanvasGeometry";
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            // For debugging purposes only.
            public override string ToString() => Name == null ? $"{TypeName} {IsCanonical}" : $"{Name} {IsCanonical}";

            // Sets the first character to lower case.
            static string CamelCase(string value) => $"_{char.ToLowerInvariant(value[0])}{value.Substring(1)}";
        }
    }
}
