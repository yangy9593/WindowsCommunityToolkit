// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.XamlHost.Forms
{
    [DebuggerStepThrough]
    public abstract class TestContextBase
    {
        public TestContext TestContext { get; set; }

        [TestCleanup]
        public void TestCleanup()
        {
            Cleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Arrange();
            Act();
        }

        protected virtual void Cleanup()
        {
        }

        protected virtual void Arrange()
        {
        }

        protected virtual void Act()
        {
        }
    }
}
