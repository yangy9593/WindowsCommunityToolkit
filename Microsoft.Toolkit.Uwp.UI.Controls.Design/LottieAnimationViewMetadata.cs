// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Design.Common;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.ComponentModel;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Design
{
	internal class LottieAnimationViewMetadata : AttributeTableBuilder
	{
        public LottieAnimationViewMetadata()
			: base()
		{
			AddCallback(typeof(LottieAnimationView),
				b =>
				{   
					b.AddCustomAttributes(nameof(LottieAnimationView.Source),
						new CategoryAttribute(Properties.Resources.CategoryCommon)
					);
                    b.AddCustomAttributes(nameof(LottieAnimationView.AutoPlay),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.RepeatCount),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.Progress),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.Scale),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.FrameRate),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.Speed),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.RepeatMode),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.MinFrame),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(nameof(LottieAnimationView.MaxFrame),
                        new CategoryAttribute(Properties.Resources.CategoryCommon)
                    );
                    b.AddCustomAttributes(new ToolboxCategoryAttribute(ToolboxCategoryPaths.Toolkit, false));
				}
			);
		}
	}
}
