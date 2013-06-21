//
// DefaultDictionary.cs
//
// Author: najmeddine nouri
//
// Copyright (c) 2013 najmeddine nouri, amine gassem
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Except as contained in this notice, the name(s) of the above copyright holders
// shall not be used in advertising or otherwise to promote the sale, use or other
// dealings in this Software without prior written authorization.
//
using System;
using System.Collections.Generic;
using System.Collections;

namespace Badr.Net
{
	/// <summary>
	/// A dictionary (wrapper around Dictionary&lt;TKey, TValue&gt;) that returns a default value if key not found (does not throw exception). 
	/// </summary>
	public class DefaultMap<TKey, TValue>: IEnumerable<KeyValuePair<TKey, TValue>>
	{
		private Dictionary<TKey, TValue> _dict;

		/// <summary>
		/// Initializes a new instance of DefaultMap&lt;TKey, TValue&gt; with DefaultValue = default(TValue)
		/// </summary>
		public DefaultMap ()
			: this(default(TValue))
		{
		}

		/// <summary>
		/// Initializes a new instance of DefaultMap&lt;TKey, TValue&gt; with DefaultValue = defaultValue
		/// </summary>
		/// <param name="defaultValue">
		/// The value to assign to DefaultValue
		/// </param>
		public DefaultMap (TValue defaultValue)
		{
			_dict = new Dictionary<TKey, TValue>();
			DefaultValue = defaultValue;
		}

		/// <summary>
		/// Returns wether a key was added to this map or not.
		/// </summary>
		/// <param name='key'>
		/// Key value.
		/// </param>
		public bool Contains(TKey key)
		{
			return _dict.ContainsKey(key);
		}

		/// <summary>
		/// Add the specified key, value. If key already exists and replaceIfExists == false, nothing is done.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='replaceIfExists'>
		/// If true and key was already added, the value will be replaced, otherwise nothing is done.
		/// </param>
		public DefaultMap<TKey, TValue> Add(TKey key, TValue value, bool replaceIfExists = true)
		{
			if(!_dict.ContainsKey(key) || replaceIfExists)
				_dict[key] = value;
			return this;
		}

		/// <summary>
		/// Remove the specified key.
		/// </summary>
		/// <param name='key'>
		/// Key to remove.
		/// </param>
		/// <returns>
		/// True if key was successfully removed.
		/// </returns>
		public bool Remove(TKey key)
		{
			return _dict.Remove(key);
		}

		/// <summary>
		/// Returns the number of key/value pairs in this map.
		/// </summary>
		public int Count
		{
			get{ return _dict.Count;}
		}

		#region Properties

		/// <summary>
		/// Gets or sets the default value to return when a key is not found. (equal to default(TValue) if not set) 
		/// </summary>
		/// <value>
		/// The default value to use when a key is not found.
		/// </value>
		public TValue DefaultValue { get; set; }

		/// <summary>
		/// Gets or sets specified key value.
		/// For `Get` operation, if not found, returns DefaultValue.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		public TValue this[TKey key]
		{
			get
			{
				if(Contains(key))
					return _dict[key];
				return DefaultValue;
			}
			set
			{
				_dict[key] = value;
			}
		}

		/// <summary>
		/// Gets or sets specified key value.
		/// For `Get` operation, if not found, returns defaultValue.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='defaultValue'>
		/// default value to return if key not found.
		/// </param>
		public TValue this[TKey key, TValue defaultValue]
		{
			get
			{
				if(Contains(key))
					return _dict[key];
				return defaultValue;
			}
		}

		/// <summary>
		/// Gets a collection of this map keys.
		/// </summary>
		public Dictionary<TKey, TValue>.KeyCollection Keys
		{
			get{
				return _dict.Keys;
			}
		}

		/// <summary>
		/// Gets a collection of this map values.
		/// </summary>
		public Dictionary<TKey, TValue>.ValueCollection Values
		{
			get{
				return _dict.Values;
			}
		}

		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Gets a KeyValuePair&lt;TKey, TValue&gt; enumerator
		/// </summary>
		/// <returns>
		/// KeyValuePair&lt;TKey, TValue&gt; enumerator.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Gets a KeyValuePair&lt;TKey, TValue&gt; enumerator
		/// </summary>
		/// <returns>
		/// KeyValuePair&lt;TKey, TValue&gt; enumerator.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return _dict.GetEnumerator();
		}
		#endregion
	}
}

