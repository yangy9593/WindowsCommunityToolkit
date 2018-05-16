using System;
using System.Collections.Generic;
using System.Linq;
using WinCompData.Sn;
using WinCompData.Wui;

namespace WinCompData.CodeGen
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CSharpInstantiatorGenerator : InstantiatorGeneratorBase
    {
        readonly CSharpStringifier _stringifier;

        CSharpInstantiatorGenerator(CompositionObject graphRoot, bool setCommentProperties, CSharpStringifier stringifier)
            : base(graphRoot, setCommentProperties, stringifier)
        {
            _stringifier = stringifier;
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
            TimeSpan duration)
        {
            var generator = new CSharpInstantiatorGenerator(rootVisual, setCommentProperties: false, stringifier: new CSharpStringifier());
            return generator.GenerateCode(className, rootVisual, width, height, progressPropertySet, duration);
        }

        protected override void WritePreamble(CodeBuilder builder, bool requiresWin2d)
        {
            builder.WriteLine("using Host = Lottie;");
            if (requiresWin2d)
            {
                builder.WriteLine("using Microsoft.Graphics.Canvas.Geometry;");
            }
            builder.WriteLine("using System;");
            builder.WriteLine("using System.Numerics;");
            builder.WriteLine("using Windows.UI;");
            builder.WriteLine("using Windows.UI.Composition;");
        }

        protected override void WriteClassStart(
            CodeBuilder builder, 
            string className, 
            Vector2 size, 
            CompositionPropertySet progressPropertySet, 
            TimeSpan duration)
        {
            builder.WriteLine();
            builder.WriteLine("namespace Compositions");
            builder.OpenScope();
            builder.WriteLine($"sealed class {className} : Host.ICompositionSource");
            builder.OpenScope();

            // Generate the method that creates an instance of the composition.
            builder.WriteLine("public bool TryCreateInstance(");
            builder.Indent();
            builder.WriteLine("Compositor compositor,");
            builder.WriteLine("out Visual rootVisual,");
            builder.WriteLine("out Vector2 size,");
            builder.WriteLine("out CompositionPropertySet progressPropertySet,");
            builder.WriteLine("out TimeSpan duration,");
            builder.WriteLine("out object diagnostics)");
            builder.UnIndent();
            builder.OpenScope();
            builder.WriteLine("rootVisual = Instantiator.InstantiateComposition(compositor);");
            builder.WriteLine($"size = {_stringifier.Vector2(size)};");
            builder.WriteLine("progressPropertySet = rootVisual.Properties;");
            builder.WriteLine($"duration = {_stringifier.TimeSpan(duration)};");
            builder.WriteLine("diagnostics = null;");
            builder.WriteLine("return true;");
            builder.CloseScope();
            builder.WriteLine();

            // Write the instantiator.
            builder.WriteLine("sealed class Instantiator");
            builder.OpenScope();
            builder.WriteLine("readonly Compositor _c;");
            builder.WriteLine($"readonly ExpressionAnimation {c_singletonExpressionAnimationName};");
        }

        protected override void WriteClassEnd(CodeBuilder builder, Visual rootVisual)
        {
            // Generate the code for the root method.
            builder.WriteLine("internal static Visual InstantiateComposition(Compositor compositor)");
            builder.Indent();
            builder.WriteLine($"=> new Instantiator(compositor).{NodeFor(rootVisual).FactoryCall()};");
            builder.UnIndent();

            // Write the constructor for the instantiator.
            builder.WriteLine();
            builder.WriteLine("Instantiator(Compositor compositor)");
            builder.OpenScope();
            builder.WriteLine("_c = compositor;");
            builder.WriteLine($"{c_singletonExpressionAnimationName} = compositor.CreateExpressionAnimation();");
            builder.CloseScope();
            builder.WriteLine();

            builder.CloseScope();
            builder.CloseScope();
            builder.CloseScope();
        }

        protected override void WriteField(CodeBuilder builder, string typeName, string fieldName)
        {
            builder.WriteLine($"{typeName} {fieldName};");
        }

        sealed class CSharpStringifier : IStringifier
        {
            string IStringifier.Deref => ".";

            string IStringifier.MemberSelect => ".";

            string IStringifier.ScopeResolve => ".";

            string IStringifier.Var => "var";

            string IStringifier.New => "new";

            string IStringifier.Null => "null";

            string IStringifier.IListAdd => "Add";

            string IStringifier.Bool(bool value) => value ? "true" : "false";

            string IStringifier.CanvasFigureLoop(Mgcg.CanvasFigureLoop value)
            {
                switch (value)
                {
                    case Mgcg.CanvasFigureLoop.Open:
                        return "CanvasFigureLoop.Open";
                    case Mgcg.CanvasFigureLoop.Closed:
                        return "CanvasFigureLoop/Closed";
                    default:
                        throw new InvalidOperationException();
                }
            }

            public string CanvasGeometryCombine(Mgcg.CanvasGeometryCombine value)
            {
                switch (value)
                {
                    case Mgcg.CanvasGeometryCombine.Union:
                        return "CanvasGeometryCombine.Union";
                    case Mgcg.CanvasGeometryCombine.Exclude:
                        return "CanvasGeometryCombine.Exclude";
                    case Mgcg.CanvasGeometryCombine.Intersect:
                        return "CanvasGeometryCombine.Intersect";
                    case Mgcg.CanvasGeometryCombine.Xor:
                        return "CanvasGeometryCombine.Xor";
                    default:
                        throw new InvalidOperationException();
                }
            }

            string IStringifier.Color(Color value) => $"Color.FromArgb({Hex(value.A)}, {Hex(value.R)}, {Hex(value.G)}, {Hex(value.B)})";

            public string FilledRegionDetermination(Mgcg.CanvasFilledRegionDetermination value)
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

            string IStringifier.Float(float value) => Float(value);

            string IStringifier.Int(int value) => value.ToString();

            string IStringifier.Matrix3x2(Matrix3x2 value)
            {
                return $"new Matrix3x2({Float(value.M11)}, {Float(value.M12)},{Float(value.M21)}, {Float(value.M22)}, {Float(value.M31)}, {Float(value.M32)})";
            }

            string IStringifier.String(string value) => $"\"{value}\"";


            public string TimeSpan(TimeSpan value) => $"TimeSpan.FromTicks({value.Ticks})";

            public string Vector2(Vector2 value) => $"new Vector2({ Float(value.X) }, { Float(value.Y)})";

            string IStringifier.Vector3(Vector3 value) => $"new Vector3({ Float(value.X) }, { Float(value.Y)}, {Float(value.Z)})";


            static string Float(float value)
            {
                if (Math.Floor(value) == value)
                {
                    // Round numbers don't need decimal places or the F suffix.
                    return value.ToString("0");
                }
                else
                {
                    return value == 0 ? "0" : (value.ToString("0.######################################") + "F");
                }
            }

            static string Hex(int value) => $"0x{value.ToString("X2")}";
        }
    }

}