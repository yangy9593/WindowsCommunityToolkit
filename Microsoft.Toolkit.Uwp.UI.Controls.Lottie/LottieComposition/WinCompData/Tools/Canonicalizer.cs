using System;
using System.Collections.Generic;
using System.Linq;
using WinCompData.Mgcg;
using WinCompData.Sn;
using WinCompData.Wui;

namespace WinCompData.Tools
{
    static class Canonicalizer
    {
        internal static void Canonicalize<T>(ObjectGraph<T> graph, bool ignoreCommentProperties) where T : CanonicalizedNode<T>, new()
        {
            CanonicalizerWorker<T>.Canonicalize(graph, ignoreCommentProperties);
        }

        sealed class CanonicalizerWorker<T> where T : CanonicalizedNode<T>, new()
        {
            readonly ObjectGraph<T> _graph;
            readonly bool _ignoreCommentProperties;

            CanonicalizerWorker(ObjectGraph<T> graph, bool ignoreCommentProperties)
            {
                _graph = graph;
                _ignoreCommentProperties = ignoreCommentProperties;
            }

            internal static void Canonicalize(ObjectGraph<T> graph, bool ignoreCommentProperties)
            {
                var canonicalizer = new CanonicalizerWorker<T>(graph, ignoreCommentProperties);
                canonicalizer.Canonicalize();
            }

            // Find the nodes that are equivalent and point them all to a single canonical representation.
            void Canonicalize()
            {
                CanonicalizeInsetClips();
                CanonicalizeEllipseGeometries();
                CanonicalizeRectangleGeometries();

                CanonicalizeCanvasGeometryPaths();

                // Easing functions must be canonicalized before keyframes are canonicalized.
                CanonicalizeLinearEasingFunctions();
                CanonicalizeCubicBezierEasingFunctions();

                CanonicalizeExpressionAnimations();

                CanonicalizeKeyFrameAnimations<KeyFrameAnimation<Color>, Color>(CompositionObjectType.ColorKeyFrameAnimation);
                CanonicalizeKeyFrameAnimations<KeyFrameAnimation<float>, float>(CompositionObjectType.ScalarKeyFrameAnimation);
                CanonicalizeKeyFrameAnimations<KeyFrameAnimation<Vector2>, Vector2>(CompositionObjectType.Vector2KeyFrameAnimation);
                CanonicalizeKeyFrameAnimations<KeyFrameAnimation<Vector3>, Vector3>(CompositionObjectType.Vector3KeyFrameAnimation);

                // ColorKeyFrameAnimations must be canonicalized before color brushes are canonicalized.
                CanonicalizeColorBrushes();
            }

            T NodeFor(object obj) => _graph[obj].Canonical;

            C CanonicalObject<C>(object obj) => (C)(NodeFor(obj).Object);

            IEnumerable<T> GetCompositionObjects(CompositionObjectType type)
            {
                return from node in _graph
                       where node.Type == Graph.NodeType.CompositionObject
                       let obj = (CompositionObject)node.Object
                       where obj.Type == type
                       select node;
            }

            IEnumerable<NodeAndObject<C>> GetCanonicalizableCompositionObjects<C>(CompositionObjectType type)
                where C : CompositionObject
            {
                var nodes = GetCompositionObjects(type);
                return
                    from node in nodes
                    let obj = (C)node.Object
                    where (_ignoreCommentProperties || obj.Comment == null)
                       && !obj.Properties.PropertyNames.Any()
                       && !obj.Animators.Any()
                    select NewNodeAndObject(node, obj);
            }

            IEnumerable<NodeAndObject<C>> GetCanonicalizableCanvasGeometries<C>(CanvasGeometry.GeometryType type) where C : CanvasGeometry
            {
                return
                    from node in _graph
                    where node.Type == Graph.NodeType.CanvasGeometry
                    let obj = (CanvasGeometry)node.Object
                    where obj.Type == type
                    select NewNodeAndObject(node, (C)obj);
            }

            void CanonicalizeExpressionAnimations()
            {
                var items = GetCanonicalizableCompositionObjects<ExpressionAnimation>(CompositionObjectType.ExpressionAnimation);

                // TODO - handle more than one reference parameter.
                var grouping =
                    from item in items
                    where item.Obj.ReferenceParameters.Count() == 1
                    group item.Node by GetExpressionAnimationKey1(item)
                    into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }


            ValueTuple<string, string, string, CompositionObject> GetExpressionAnimationKey1(NodeAndObject<ExpressionAnimation> item)
            {
                var rp0 = item.Obj.ReferenceParameters.First();

                return ValueTuple.Create(item.Obj.Expression, item.Obj.Target, rp0.Key, CanonicalObject<CompositionObject>(rp0.Value));
            }

            void CanonicalizeKeyFrameAnimations<A, V>(CompositionObjectType animationType) where A : KeyFrameAnimation<V>
            {
                var items = GetCanonicalizableCompositionObjects<A>(animationType);

                var grouping =
                    from item in items
                    group item.Node by new KeyFrameAnimationKey<V>(this, item.Obj)
                    into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }



            sealed class KeyFrameAnimationKey<V>
            {
                readonly CanonicalizerWorker<T> _owner;
                readonly KeyFrameAnimation<V> _obj;

                internal KeyFrameAnimationKey(CanonicalizerWorker<T> owner, KeyFrameAnimation<V> obj)
                {
                    _owner = owner;
                    _obj = obj;
                }

                public override int GetHashCode()
                {
                    // Not the perfect hash, but not terrible
                    return _obj.KeyFrameCount ^ (int)_obj.Duration.Ticks;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(this, obj))
                    {
                        return true;
                    }
                    if (obj == null)
                    {
                        return false;
                    }
                    var other = obj as KeyFrameAnimationKey<V>;
                    if (other == null)
                    {
                        return false;
                    }

                    var thisObj = _obj;
                    var otherObj = other._obj;

                    if (thisObj.Duration != otherObj.Duration)
                    {
                        return false;
                    }

                    if (thisObj.KeyFrameCount != otherObj.KeyFrameCount)
                    {
                        return false;
                    }

                    if (thisObj.Target != otherObj.Target)
                    {
                        return false;
                    }

                    var thisKfs = thisObj.KeyFrames.ToArray();
                    var otherKfs = otherObj.KeyFrames.ToArray();

                    for (var i = 0; i < thisKfs.Length; i++)
                    {
                        var thisKf = thisKfs[i];
                        var otherKf = otherKfs[i];
                        if (thisKf.Progress != otherKf.Progress)
                        {
                            return false;
                        }
                        if (thisKf.Type != otherKf.Type)
                        {
                            return false;
                        }
                        if (_owner.NodeFor(thisKf.Easing) != _owner.NodeFor(otherKf.Easing))
                        {
                            return false;
                        }
                        switch (thisKf.Type)
                        {
                            case KeyFrameAnimation<V>.KeyFrameType.Expression:
                                var thisExpressionKeyFrame = (KeyFrameAnimation<V>.ExpressionKeyFrame)thisKf;
                                var otherExpressionKeyFrame = (KeyFrameAnimation<V>.ExpressionKeyFrame)otherKf;
                                if (thisExpressionKeyFrame.Expression != otherExpressionKeyFrame.Expression)
                                {
                                    return false;
                                }
                                break;
                            case KeyFrameAnimation<V>.KeyFrameType.Value:
                                var thisValueKeyFrame = (KeyFrameAnimation<V>.ValueKeyFrame)thisKf;
                                var otherValueKeyFrame = (KeyFrameAnimation<V>.ValueKeyFrame)otherKf;
                                if (!thisValueKeyFrame.Value.Equals(otherValueKeyFrame.Value))
                                {
                                    return false;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    return true;
                }
            }



            void CanonicalizeColorBrushes()
            {
                var items = GetCanonicalizableCompositionObjects<CompositionColorBrush>(CompositionObjectType.CompositionColorBrush);

                var grouping =
                    from item in items
                    let obj = item.Obj
                    group item.Node by new
                    {
                        obj.Color.A,
                        obj.Color.R,
                        obj.Color.G,
                        obj.Color.B
                    } into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }

            void CanonicalizeEllipseGeometries()
            {
                var items = GetCanonicalizableCompositionObjects<CompositionEllipseGeometry>(CompositionObjectType.CompositionEllipseGeometry);

                var grouping =
                    from item in items
                    let obj = item.Obj
                    group item.Node by new
                    {
                        obj.Center.X,
                        obj.Center.Y,
                        Rx = obj.Radius.X,
                        Ry = obj.Radius.Y,
                        obj.TrimStart,
                        obj.TrimEnd,
                        obj.TrimOffset
                    } into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }

            void CanonicalizeRectangleGeometries()
            {
                var items = GetCanonicalizableCompositionObjects<CompositionRectangleGeometry>(CompositionObjectType.CompositionRectangleGeometry);

                var grouping =
                    from item in items
                    let obj = item.Obj
                    group item.Node by new
                    {
                        obj.Size.X,
                        obj.Size.Y,
                        obj.TrimStart,
                        obj.TrimEnd,
                        obj.TrimOffset
                    } into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }

            void CanonicalizeCanvasGeometryPaths()
            {
                var items = GetCanonicalizableCanvasGeometries<CanvasGeometry.Path>(CanvasGeometry.GeometryType.Path);
                var grouping =
                    from item in items
                    let obj = item.Obj
                    group item.Node by obj into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }

            void CanonicalizeInsetClips()
            {
                var items = GetCanonicalizableCompositionObjects<InsetClip>(CompositionObjectType.InsetClip);

                var grouping =
                    from item in items
                    let obj = item.Obj
                    group item.Node by
                    new
                    {
                        obj.BottomInset,
                        obj.LeftInset,
                        obj.RightInset,
                        obj.TopInset,
                        CenterPointX = obj.CenterPoint.X,
                        CenterPointY = obj.CenterPoint.Y,
                        ScaleX = obj.Scale.X,
                        ScaleY = obj.Scale.Y
                    }
                    into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }
            void CanonicalizeCubicBezierEasingFunctions()
            {
                var items = GetCanonicalizableCompositionObjects<CubicBezierEasingFunction>(CompositionObjectType.CubicBezierEasingFunction);

                var grouping =
                    from item in items
                    let obj = item.Obj
                    group item.Node by
                    new
                    {
                        Cp1X = obj.ControlPoint1.X,
                        Cp1Y = obj.ControlPoint1.Y,
                        Cp2X = obj.ControlPoint2.X,
                        Cp2Y = obj.ControlPoint2.Y
                    }
                    into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }

            void CanonicalizeLinearEasingFunctions()
            {
                var items = GetCanonicalizableCompositionObjects<LinearEasingFunction>(CompositionObjectType.LinearEasingFunction);

                // Every LinearEasingFunction is equivalent.
                var grouping =
                    from item in items
                    group item.Node by true into grouped
                    select grouped;

                CanonicalizeGrouping(grouping);
            }

            static NodeAndObject<C> NewNodeAndObject<C>(T node, C obj)
                => new NodeAndObject<C> { Node = node, Obj = obj };

            struct NodeAndObject<C>
            {
                internal T Node;
                internal C Obj;
            }


            static void CanonicalizeGrouping<K>(IEnumerable<IGrouping<K, T>> grouping)
            {
                foreach (var group in grouping)
                {
                    var canonical = group.First();
                    var groupArray = group.ToArray();

                    foreach (var node in group)
                    {
                        node.Canonical = canonical;
                        node.NodesInGroup = groupArray;
                    }
                }
            }

        }
    }
}
