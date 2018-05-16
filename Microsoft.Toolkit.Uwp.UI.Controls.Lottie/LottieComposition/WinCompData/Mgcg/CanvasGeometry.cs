using System;
using System.Collections.Generic;
using System.Linq;
using WinCompData.Mgc;
using WinCompData.Sn;
using WinCompData.Wg;

namespace WinCompData.Mgcg
{
#if !WINDOWS_UWP
    public
#endif
    abstract class CanvasGeometry : IGeometrySource2D
    {
        CanvasGeometry() { }

        public enum GeometryType
        {
            Combination,
            Ellipse,
            Path,
            RoundedRectangle,
        }

        public static CanvasGeometry CreatePath(CanvasPathBuilder pathBuilder)
            => new Path(pathBuilder.Commands);

        public static CanvasGeometry CreateRoundedRectangle(CanvasDevice device, float x, float y, float w, float h, float radiusX, float radiusY)
            => new RoundedRectangle
            {
                X = x,
                Y = y,
                W = w,
                H = h,
                RadiusX = radiusX,
                RadiusY = radiusY
            };

        public static CanvasGeometry CreateEllipse(CanvasDevice device, float x, float y, float radiusX, float radiusY)
            => new Ellipse
            {
                X = x,
                Y = y,
                RadiusX = radiusX,
                RadiusY = radiusY
            };

        public CanvasGeometry CombineWith(CanvasGeometry other, Matrix3x2 matrix, CanvasGeometryCombine combineMode)
         => new Combination
         {
             A = this,
             B = other,
             Matrix = matrix,
             CombineMode = combineMode
         };


        public abstract GeometryType Type { get; }

        public sealed class Combination : CanvasGeometry
        {
            public CanvasGeometry A { get; internal set; }
            public CanvasGeometry B { get; internal set; }
            public Matrix3x2 Matrix { get; internal set; }
            public CanvasGeometryCombine CombineMode { get; internal set; }
            public override GeometryType Type => GeometryType.Combination;
        }

        public sealed class Ellipse : CanvasGeometry
        {
            internal Ellipse() { }
            public float X { get; internal set; }
            public float Y { get; internal set; }
            public float RadiusX { get; internal set; }
            public float RadiusY { get; internal set; }

            public override GeometryType Type => GeometryType.Ellipse;
        }

        public sealed class Path : CanvasGeometry, IEquatable<Path>
        {
            internal Path(IEnumerable<CanvasPathBuilder.Command> commands)
            {
                Commands = commands.ToArray();
            }

            public CanvasPathBuilder.Command[] Commands { get; }

            public override GeometryType Type => GeometryType.Path;

            public bool Equals(Path other)
            {
                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                if (other == null)
                {
                    return false;
                }

                if (other.Commands.Length != Commands.Length)
                {
                    return false;
                }

                for (var i = 0; i < Commands.Length; i++)
                {
                    var thisCommand = Commands[i];
                    var otherCommand = other.Commands[i];

                    if (thisCommand.Type != otherCommand.Type)
                    {
                        return false;
                    }

                    switch (thisCommand.Type)
                    {
                        case CanvasPathBuilder.CommandType.BeginFigure:
                            {
                                var thisArg = (Vector2)thisCommand.Args;
                                var otherArg = (Vector2)otherCommand.Args;
                                if (!thisArg.Equals(otherArg))
                                {
                                    return false;
                                }
                            }
                            break;
                        case CanvasPathBuilder.CommandType.EndFigure:
                            {
                                var thisArg = (CanvasFigureLoop)thisCommand.Args;
                                var otherArg = (CanvasFigureLoop)otherCommand.Args;
                                if (thisArg != otherArg)
                                {
                                    return false;
                                }
                            }
                            break;
                        case CanvasPathBuilder.CommandType.AddCubicBezier:
                            {
                                var thisArg = (Vector2[])thisCommand.Args;
                                var otherArg = (Vector2[])otherCommand.Args;
                                if (!thisArg[0].Equals(otherArg[0]) ||
                                    !thisArg[1].Equals(otherArg[1]) ||
                                    !thisArg[2].Equals(otherArg[2]))
                                {
                                    return false;
                                }
                            }
                            break;
                        case CanvasPathBuilder.CommandType.SetFilledRegionDetermination:
                            {
                                var thisArg = (CanvasFilledRegionDetermination)thisCommand.Args;
                                var otherArg = (CanvasFilledRegionDetermination)otherCommand.Args;
                                if (thisArg != otherArg)
                                {
                                    return false;
                                }
                            }
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                // Less than ideal but cheap hash function.
                return Commands.Length;
            }
        }

        public sealed class RoundedRectangle : CanvasGeometry
        {
            public float X { get; internal set; }
            public float Y { get; internal set; }
            public float W { get; internal set; }
            public float H { get; internal set; }
            public float RadiusX { get; internal set; }
            public float RadiusY { get; internal set; }
            public override GeometryType Type => GeometryType.RoundedRectangle;
        }
    }
}
