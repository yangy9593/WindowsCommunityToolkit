// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    internal static class HashMapHelperClass
    {
        internal static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            var entries = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var keyValuePair in dictionary)
            {
                entries.Add(keyValuePair);
            }

            return entries;
        }
    }
}