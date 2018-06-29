// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Extend this class to replace animation text with custom text. This can be useful to handle
    /// translations.
    ///
    /// The only method you should have to override is <seealso cref="GetText(string)"/>.
    /// </summary>
    public class TextDelegate
    {
        private readonly Dictionary<string, string> _stringMap = new Dictionary<string, string>();
        private readonly LottieAnimationView _animationView;
        private readonly LottieDrawable _drawable;
        private bool _cacheText = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDelegate"/> class.
        /// This normally needs to be able to invalidate the view/drawable but not for the test.
        /// </summary>
        internal TextDelegate()
        {
            _animationView = null;
            _drawable = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDelegate"/> class.
        /// </summary>
        /// <param name="animationView">The <see cref="LottieAnimationView"/> associated with this TextDelegate</param>
        public TextDelegate(LottieAnimationView animationView)
        {
            _animationView = animationView;
            _drawable = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDelegate"/> class.
        /// </summary>
        /// <param name="drawable">The <see cref="LottieDrawable"/> associated with this TextDelegate</param>
        public TextDelegate(LottieDrawable drawable)
        {
            _drawable = drawable;
            _animationView = null;
        }

        /// <summary>
        /// Override this to replace the animation text with something dynamic. This can be used for
        /// translations or custom data.
        /// </summary>
        /// <returns>Returns the text to be used for rendering.</returns>
        private string GetText(string input)
        {
            return input;
        }

        /// <summary>
        /// Update the text that will be rendered for the given input text.
        /// </summary>
        public virtual void SetText(string input, string output)
        {
            _stringMap[input] = output;
            Invalidate();
        }

        /// <summary>
        /// Sets a value indicating whether or not <seealso cref="TextDelegate"/> will cache (memorize) the results of getText.
        /// If this isn't necessary then set it to false.
        /// </summary>
        public virtual bool CacheText
        {
            set => _cacheText = value;
        }

        /// <summary>
        /// Invalidates a cached string with the given input.
        /// </summary>
        public virtual void InvalidateText(string input)
        {
            _stringMap.Remove(input);
            Invalidate();
        }

        /// <summary>
        /// Invalidates all cached strings
        /// </summary>
        public virtual void InvalidateAllText()
        {
            _stringMap.Clear();
            Invalidate();
        }

        internal string GetTextInternal(string input)
        {
            if (_cacheText && _stringMap.ContainsKey(input))
            {
                return _stringMap[input];
            }

            var text = GetText(input);
            if (_cacheText)
            {
                _stringMap[input] = text;
            }

            return text;
        }

        private void Invalidate()
        {
            _animationView?.InvalidateArrange(); // Equivalent?
            _drawable?.InvalidateSelf();
        }
    }
}
