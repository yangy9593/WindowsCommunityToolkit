using System;
using System.Collections.Generic;
using Windows.Data.Json;
using JsonArray = LottieData.Serialization.CheckedJsonArray;
using System.Collections;

namespace LottieData.Serialization
{
    sealed class CheckedJsonObject : IEnumerable<KeyValuePair<string, IJsonValue>>
    {
        internal readonly Windows.Data.Json.JsonObject _wrapped;
        internal readonly HashSet<string> _readFields = new HashSet<string>();

        internal CheckedJsonObject(Windows.Data.Json.JsonObject wrapped)
        {
            _wrapped = wrapped;
        }

        internal static CheckedJsonObject Parse(string input) => new CheckedJsonObject(Windows.Data.Json.JsonObject.Parse(input));

        internal JsonValueType ValueType => _wrapped.ValueType;

        internal bool ContainsKey(string key)
        {
            _readFields.Add(key);
            return _wrapped.ContainsKey(key);
        }

        internal IJsonValue GetNamedValue(string name)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedValue(name);
        }

        internal string GetNamedString(string name)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedString(name);
        }

        internal string GetNamedString(string name, string defaultValue)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedString(name, defaultValue);
        }

        internal double GetNamedNumber(string name)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedNumber(name);
        }
        internal double GetNamedNumber(string name, double defaultValue)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedNumber(name, defaultValue);
        }

        internal CheckedJsonArray GetNamedArray(string name)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedArray(name);
        }
        internal JsonArray GetNamedArray(string name, CheckedJsonArray defaultValue)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedArray(name, defaultValue?._wrapped);
        }

        internal CheckedJsonObject GetNamedObject(string name)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedObject(name);
        }

        internal CheckedJsonObject GetNamedObject(string name, CheckedJsonObject defaultValue)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedObject(name, defaultValue?._wrapped);
        }

        internal bool GetNamedBoolean(string name)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedBoolean(name);
        }

        internal bool GetNamedBoolean(string name, bool defaultValue)
        {
            _readFields.Add(name);
            return _wrapped.GetNamedBoolean(name, defaultValue);
        }

        public IEnumerator<KeyValuePair<string, IJsonValue>> GetEnumerator()
        {
            foreach (var pair in _wrapped)
            {
                yield return pair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator CheckedJsonObject(Windows.Data.Json.JsonObject value)
        {
            return value == null ? null : new CheckedJsonObject(value);
        }
    }

    sealed class CheckedJsonArray : IList<IJsonValue>
    {
        internal readonly Windows.Data.Json.JsonArray _wrapped;
        internal CheckedJsonArray(Windows.Data.Json.JsonArray wrapped)
        {
            _wrapped = wrapped;
        }

        public IJsonValue this[int index] { get => _wrapped[index]; set => throw new NotImplementedException(); }

        public int Count => _wrapped.Count;


        internal CheckedJsonObject GetObjectAt(uint index)
        {
            return _wrapped.GetObjectAt(index);
        }

        internal double GetNumberAt(uint index)
        {
            return _wrapped.GetNumberAt(index);
        }

        internal CheckedJsonArray GetArrayAt(uint index)
        {
            return _wrapped.GetArrayAt(index);
        }
        public bool IsReadOnly => true;

        public void Add(IJsonValue item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IJsonValue item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IJsonValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IJsonValue> GetEnumerator()
        {
            foreach (var value in _wrapped)
            {
                yield return value;
            }
        }

        public int IndexOf(IJsonValue item) => throw new NotImplementedException();
        public void Insert(int index, IJsonValue item) => throw new NotImplementedException();
        public bool Remove(IJsonValue item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator CheckedJsonArray(Windows.Data.Json.JsonArray value)
        {
            return value == null ? null : new CheckedJsonArray(value);
        }
    }
}
