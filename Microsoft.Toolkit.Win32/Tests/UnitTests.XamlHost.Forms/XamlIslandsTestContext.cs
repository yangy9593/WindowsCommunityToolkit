// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Toolkit.Forms.UI.XamlHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.XamlHost.Forms
{
    [DebuggerStepThrough]
    public abstract class XamlIslandsTestContext : TestContextBase
    {
        protected XamlIslandsTestContext()
        {
            Form = new TestHostForm();
        }

        protected TestHostForm Form { get; private set; }

        protected WindowsXamlHost Host { get; set; }

        protected abstract void CreateXamlHost();

        protected virtual void PerformActionAndWaitForFormClose(Action action)
        {
            void OnFormLoad(object sender, EventArgs e)
            {
                Application.DoEvents();
                action();
            }

            Form.Load += OnFormLoad;
            Application.Run(Form);
        }

        protected override void Arrange()
        {
            Assert.IsNotNull(Form);

            Form.Text = TestContext.TestName;

            CreateXamlHost();

            Assert.IsNotNull(Host);

            base.Arrange();
        }

        protected override void Cleanup()
        {
            try
            {
                if (Form != null && !Form.IsDisposed)
                {
                    Form.BringToFront();
                    Form.Close();
                    Form.Dispose();
                }
            }
            finally
            {
                base.Cleanup();
            }
        }
    }
}
