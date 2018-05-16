using System;
using WinCompData.Sn;
using WinCompData.Wui;

namespace WinCompData.CodeGen
{
    /// <summary>
    /// Stringifiers for C++ syntax.
    /// </summary>
    sealed class CppStringifier : InstantiatorGeneratorBase.IStringifier
    {
        string InstantiatorGeneratorBase.IStringifier.Bool(bool value) => value ? "true" : "false";

        public string CanvasFigureLoop(Mgcg.CanvasFigureLoop value)
        {
            switch (value)
            {
                case Mgcg.CanvasFigureLoop.Open:
                    return "D2D1_FIGURE_END_OPEN";
                case Mgcg.CanvasFigureLoop.Closed:
                    return "D2D1_FIGURE_END_CLOSED";
                default:
                    throw new InvalidOperationException();
            }
        }

        public string CanvasGeometryCombine(Mgcg.CanvasGeometryCombine value)
        {
            switch (value)
            {
                case Mgcg.CanvasGeometryCombine.Union:
                    return "CanvasGeometryCombine::Union";
                case Mgcg.CanvasGeometryCombine.Exclude:
                    return "CanvasGeometryCombine::Exclude";
                case Mgcg.CanvasGeometryCombine.Intersect:
                    return "CanvasGeometryCombine::Intersect";
                case Mgcg.CanvasGeometryCombine.Xor:
                    return "CanvasGeometryCombine::Xor";
                default:
                    throw new InvalidOperationException();
            }
        }

        string InstantiatorGeneratorBase.IStringifier.Color(Color value) => $"ColorHelper::FromArgb({Hex(value.A)}, {Hex(value.R)}, {Hex(value.G)}, {Hex(value.B)})";

        public string Deref => "->";

        string InstantiatorGeneratorBase.IStringifier.Float(float value) => Float(value);

        public string FilledRegionDetermination(Mgcg.CanvasFilledRegionDetermination value)
        {
            switch (value)
            {
                case Mgcg.CanvasFilledRegionDetermination.Alternate:
                    return "D2D1_FILL_MODE_ALTERNATE";
                case Mgcg.CanvasFilledRegionDetermination.Winding:
                    return "D2D1_FILL_MODE_WINDING";
                default:
                    throw new InvalidOperationException();
            }
        }

        string InstantiatorGeneratorBase.IStringifier.Int(int value) => value.ToString();


        string InstantiatorGeneratorBase.IStringifier.ScopeResolve => "::";

        public string MemberSelect => ".";

        public string New => "ref new";

        string InstantiatorGeneratorBase.IStringifier.Null => "nullptr";


        string InstantiatorGeneratorBase.IStringifier.Matrix3x2(Matrix3x2 value)
        {
            return $"*(ref new float3x2({Float(value.M11)}, {Float(value.M12)},{Float(value.M21)}, {Float(value.M22)}, {Float(value.M31)}, {Float(value.M32)}))";
        }

        string InstantiatorGeneratorBase.IStringifier.String(string value) => $"\"{value}\"";

        public string TimeSpan(TimeSpan value) => $"TimeSpan{{{value.Ticks}L}}";

        string InstantiatorGeneratorBase.IStringifier.Var => "auto";

        string InstantiatorGeneratorBase.IStringifier.Vector2(Vector2 value) => $"float2({ Float(value.X) }, { Float(value.Y)})";

        string InstantiatorGeneratorBase.IStringifier.Vector3(Vector3 value) => $"float3({ Float(value.X) }, { Float(value.Y)}, {Float(value.Z)})";

        internal string Vector2Raw(Vector2 value) => $"{{{Float(value.X)}, {Float(value.Y)}}}";

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

        string InstantiatorGeneratorBase.IStringifier.IListAdd => "Append";
    }
}
