using System.Collections.Generic;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    abstract class CompositionAnimation : CompositionObject
    {
        readonly Dictionary<string, CompositionObject> _referencedParameters = new Dictionary<string, CompositionObject>();

        internal CompositionAnimation(CompositionAnimation other)
        {
            if (other != null)
            {
                foreach (var pair in other._referencedParameters)
                {
                    _referencedParameters.Add(pair.Key, pair.Value);
                }
                Target = other.Target;
            }
        }

        public string Target { get; set; }

        public void SetReferenceParameter(string key, CompositionObject compositionObject)
        {
            _referencedParameters.Add(key, compositionObject);
        }

        public IEnumerable<KeyValuePair<string, CompositionObject>> ReferenceParameters => _referencedParameters;

        internal abstract CompositionAnimation Clone();
    }
}
