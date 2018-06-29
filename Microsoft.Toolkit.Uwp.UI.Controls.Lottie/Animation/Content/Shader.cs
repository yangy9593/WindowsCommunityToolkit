// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Content
{
    internal abstract class Shader
    {
        public Matrix3X3 LocalMatrix { get; set; } = Matrix3X3.CreateIdentity();
    }
}