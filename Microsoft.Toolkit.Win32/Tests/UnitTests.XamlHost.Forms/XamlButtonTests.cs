// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Forms.UI.XamlHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.XamlHost.Forms
{

    [TestClass]
    public class XamlButtonTest : XamlIslandsTestContext
    {
        // Arrange
        protected override void CreateXamlHost()
        {
            // What the designer emits
            Host = new WindowsXamlHost();
            Form.SuspendLayout();

            // XAML Host
            Host.AutoSize = true;
            Host.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            Host.InitialTypeName = "Windows.UI.Xaml.Controls.Button";
            Host.Location = new System.Drawing.Point(121, 158);
            Host.Name = "WindowsXamlHost";
            Host.Size = new System.Drawing.Size(330, 78);
            Host.TabIndex = 0;
            Host.Text = "WindowsXamlHost";

            Form.Controls.Add(Host);
            Form.ResumeLayout(false);
        }

        protected override void Arrange()
        {
            base.Arrange();

            Host.ChildChanged += (o, e) =>
            {
                // This is actually executed when the test is executed (e.g. "Act")
                var btn = Host.Child as Windows.UI.Xaml.Controls.Button;
                _childIsXamlButton = btn != null;

                btn.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                btn.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                btn.Content = "UWP XAML Button";
            };
        }

        // Act

        private bool _childIsXamlButton = false;

        protected override void Act()
        {
            PerformActionAndWaitForFormClose(() =>
            {
                Form.BringToFront();
                Form.Close();
            });
        }

        // Assert
        [TestMethod]
        public void XamlButtonCreated()
        {
            Assert.IsTrue(_childIsXamlButton);
        }
    }
}
