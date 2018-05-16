using LottieData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lottie
{
    /// <summary>
    /// Diagnostics information about a Lottie and its translation.
    /// </summary>
    public sealed class LottieCompositionDiagnostics
    {
        static readonly string[] s_emptyStrings = new string[0];
        static readonly KeyValuePair<string, double>[] s_emptyMarkers = new KeyValuePair<string, double>[0];

        public string FileName { get; internal set; } = "";

        public string SuggestedName
        {
            get
            {
                var name = string.IsNullOrWhiteSpace(FileName) ? "MyComposition" : FileName;
                return Path.GetFileNameWithoutExtension(name);
            }
        }

        public TimeSpan Duration => LottieComposition?.Duration ?? TimeSpan.Zero;

        public TimeSpan ReadTime { get; internal set; }

        public TimeSpan ParseTime { get; internal set; }

        public TimeSpan ValidationTime { get; internal set; }

        public TimeSpan TranslationTime { get; internal set; }

        public TimeSpan InstantiationTime { get; internal set; }

        public IEnumerable<string> JsonParsingIssues { get; internal set; } = s_emptyStrings;

        public IEnumerable<string> LottieValidationIssues { get; internal set; } = s_emptyStrings;

        public IEnumerable<string> TranslationIssues { get; internal set; } = s_emptyStrings;

        public double LottieWidth => LottieComposition?.Width ?? 0;

        public double LottieHeight => LottieComposition?.Height ?? 0;

        public string LottieDetails => DescribeLottieComposition();
        public string LottieVersion => LottieComposition?.Version.ToString() ?? "";

        /// <summary>
        /// The options that were set on the <see cref="LottieCompositionSource"/> when it 
        /// produced this diagnostics object.
        /// </summary>
        public LottieCompositionOptions Options { get; internal set; }

        public string GenerateLottieXml()
        {
            if (LottieComposition == null) { return null; }
            return LottieData.Tools.LottieCompositionXmlSerializer.ToXml(LottieComposition).ToString();
        }

        public string GenerateWinCompXml()
        {
            return WinCompData.Tools.CompositionObjectXmlSerializer.ToXml(RootVisual).ToString();
        }

        public string GenerateCSharpCode(string suggestedClassName)
        {
            if (LottieComposition == null) { return null; }
            return
                WinCompData.CodeGen.CSharpInstantiatorGenerator.CreateFactoryCode(
                    MakeNameSuitableForTypeName(suggestedClassName),
                    RootVisual,
                    (float)LottieComposition.Width,
                    (float)LottieComposition.Height,
                    RootVisual.Properties,
                    LottieComposition.Duration);
        }


        public string GenerateCxCode(string suggestedClassName)
        {
            if (LottieComposition == null) { return null; }
            return
                WinCompData.CodeGen.CxInstantiatorGenerator.CreateFactoryCode(
                    MakeNameSuitableForTypeName(suggestedClassName),
                    RootVisual,
                    (float)LottieComposition.Width,
                    (float)LottieComposition.Height,
                    RootVisual.Properties,
                    LottieComposition.Duration);
        }

        static string MakeNameSuitableForTypeName(string name)
        {
            // If the first character is not a letter, prepend an underscore.
            if (!char.IsLetter(name, 0))
            {
                name = "_" + name;
            }

            // Replace any disallowed character with underscores.
            name =
                new string((from ch in name
                            select char.IsLetterOrDigit(ch) ? ch : '_').ToArray());

            // Remove any duplicated underscores.
            name = name.Replace("__", "_");

            // Capitalize the first letter.
            name = name.ToUpperInvariant().Substring(0, 1) + name.Substring(1);

            return name;
        }

        public KeyValuePair<string, double>[] Markers { get; internal set; } = s_emptyMarkers;

        // Holds the parsed LottieComposition. Only used if one of the codegen or XML options was selected.
        internal LottieComposition LottieComposition { get; set; }

        // Holds the translated Visual. Only used if one of the codgen or XML options was selected.
        internal WinCompData.Visual RootVisual { get; set; }

        internal LottieCompositionDiagnostics Clone() =>
            new LottieCompositionDiagnostics
            {
                FileName = FileName,
                InstantiationTime = InstantiationTime,
                JsonParsingIssues = JsonParsingIssues,
                LottieComposition = LottieComposition,
                LottieValidationIssues = LottieValidationIssues,
                Markers = Markers,
                Options = Options,
                ParseTime = ParseTime,
                ReadTime = ReadTime,
                RootVisual = RootVisual,
                TranslationTime = TranslationTime,
                ValidationTime = ValidationTime,
                TranslationIssues = TranslationIssues,
            };

        // Creates a string that describes the Lottie.
        string DescribeLottieComposition()
        {
            if (LottieComposition == null) { return null; }

            int precompLayerCount = 0;
            int solidLayerCount = 0;
            int imageLayerCount = 0;
            int nullLayerCount = 0;
            int shapeLayerCount = 0;
            int textLayerCount = 0;

            // Get the layers stored in assets.
            var layersInAssets =
                from asset in LottieComposition.Assets
                where asset.Type == Asset.AssetType.LayerCollection
                let layerCollection = (LayerCollectionAsset)asset
                from layer in layerCollection.Layers.GetLayersBottomToTop()
                select layer;

            foreach (var layer in LottieComposition.Layers.GetLayersBottomToTop().Concat(layersInAssets))
            {
                switch (layer.Type)
                {
                    case Layer.LayerType.PreComp:
                        precompLayerCount++;
                        break;
                    case Layer.LayerType.Solid:
                        solidLayerCount++;
                        break;
                    case Layer.LayerType.Image:
                        imageLayerCount++;
                        break;
                    case Layer.LayerType.Null:
                        nullLayerCount++;
                        break;
                    case Layer.LayerType.Shape:
                        shapeLayerCount++;
                        break;
                    case Layer.LayerType.Text:
                        textLayerCount++;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return $"LottieCompositionSource w={LottieComposition.Width} h={LottieComposition.Height} " +
                $"layers: precomp={precompLayerCount} solid={solidLayerCount} image={imageLayerCount} null={nullLayerCount} shape={shapeLayerCount} text={textLayerCount}";
        }
    }
}
