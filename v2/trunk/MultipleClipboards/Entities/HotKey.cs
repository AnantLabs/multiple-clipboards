using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MultipleClipboards.Entities
{
	/// <summary>
	/// Class to hold data about a specific Hot Key.
	/// </summary>
	public class HotKey : IEquatable<HotKey>
	{
		private int modifierBitMask;

		/// <summary>
		/// The collection of HotKeys that are reserved and cannot be used by users.
		/// </summary>
		public static readonly ReadOnlyCollection<HotKey> ReservedHotKeys = new ReadOnlyCollection<HotKey>(new List<HotKey>
		{
			new HotKey(Key.X, System.Windows.Input.ModifierKeys.Control),
			new HotKey(Key.C, System.Windows.Input.ModifierKeys.Control),
			new HotKey(Key.V, System.Windows.Input.ModifierKeys.Control)
		});

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		public HotKey()
			: this(0, HotKeyType.Undefined, Key.None)
		{
		}

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="modifierKeys">The collection of modifier keys for this hot key.</param>
		public HotKey(Key key, params ModifierKeys[] modifierKeys)
			: this(0, HotKeyType.Undefined, key, modifierKeys)
		{
		}

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		/// <param name="clipboardId">The ID of the clipboard that this hot key belongs to.</param>
		/// <param name="hotKeyType">The hot key type.</param>
		/// <param name="key">The key.</param>
		/// <param name="modifierKeys">The collection of modifier keys for this hot key.</param>
		public HotKey(int clipboardId, HotKeyType hotKeyType, Key key, params ModifierKeys[] modifierKeys)
		{
			this.ClipboardId = clipboardId;
			this.HotKeyType = hotKeyType;
			this.Key = key;
			this.ModifierKeys = (IList<ModifierKeys>)modifierKeys ?? new List<ModifierKeys>();
		}

		public short HotKeyId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ID of the clipboard that this hot key belongs to.
		/// </summary>
		public int ClipboardId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the operation for this hot key.
		/// </summary>
		public HotKeyType HotKeyType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the key for this hot key.
		/// </summary>
		public Key Key
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the collection of modifier keys used in this HotKey.
		/// </summary>
		public IList<ModifierKeys> ModifierKeys
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the bitmask of all the modifier keys for this HotKey.
		/// </summary>
		public int ModifierBitmask
		{
			get
			{
				if (this.modifierBitMask == 0)
				{
					Array.ForEach(this.ModifierKeys.ToArray(), value => this.modifierBitMask = this.modifierBitMask | (int)value);
				}

				return this.modifierBitMask;
			}
		}

		/// <summary>
		/// Gets the Win32 key code for this HotKey.
		/// </summary>
		public int KeyCode
		{
			get
			{
				return KeyInterop.VirtualKeyFromKey(this.Key);
			}
		}

		/// <summary>
		/// Compares one HotKey object to another.
		/// </summary>
		/// <remarks>
		/// A HotKey is equal to another HotKey when the modifers and keys match.
		/// The other properties are ignored because when a HotKey is pressed this is the only information we know about it.
		/// We must use the keys to match it to the real HotKey that contains the clipboard and HotKeyType information.
		/// </remarks>
		/// <param name="lhs">The HotKey object on the left side of the comparison.</param>
		/// <param name="rhs">The HotKey object on the right side of the comparison.</param>
		/// <returns>True if the two HotKeys are equal, false if not.</returns>
		public static bool operator ==(HotKey lhs, HotKey rhs)
		{
			if (ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			else if ((object)lhs == null || (object)rhs == null)
			{
				return false;
			}

			return lhs.ModifierBitmask == rhs.ModifierBitmask && lhs.Key == rhs.Key;
		}

		/// <summary>
		/// Compares one HotKey object to another.
		/// </summary>
		/// <remarks>
		/// A hotkey is equal to another hotkey when the modifers and keys match.
		/// The other properties are ignored because when a hotkey is pressed this is the only information we know about it.
		/// We must use the keys to match it to the real hotkey that contains the clipboard and operation information.
		/// </remarks>
		/// <param name="lhs">The HotKey object on the left side of the comparison.</param>
		/// <param name="rhs">The HotKey object on the right side of the comparison.</param>
		/// <returns>True if the two hot keys are not equal, false if they are.</returns>
		public static bool operator !=(HotKey lhs, HotKey rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// Compares a HotKey object to another object.
		/// </summary>
		/// <param name="obj">The object to compare the HotKey to.</param>
		/// <returns>True if the two objects are equal, false if they are not.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is HotKey))
			{
				return false;
			}

			return this == (HotKey)obj;
		}

		/// <summary>
		/// Gets the hash code for this HotKey.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return this.ModifierKeys.GetHashCode() ^ this.Key.GetHashCode();
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(HotKey other)
		{
			return this == other;
		}

		/// <summary>
		/// Indicates whether or not the given key and modifier key collection is the same as this HotKey instance.
		/// </summary>
		/// <param name="key">The key to compare.</param>
		/// <param name="modifierKeys">The collection of modifier keys for the compare.</param>
		/// <returns>True if the given key and modifiers match this HotKey, false otherwise.</returns>
		public bool Equals(Key key, params ModifierKeys[] modifierKeys)
		{
			return this == new HotKey(key, modifierKeys);
		}

		/// <summary>
		/// Gets a string representation of the HotKey.
		/// </summary>
		/// <returns>A string representation of the HotKey.</returns>
		public override string ToString()
		{
			StringBuilder modifierStringBuilder = new StringBuilder();

			foreach (ModifierKeys modifierKey in this.ModifierKeys)
			{
				if (modifierKey == System.Windows.Input.ModifierKeys.None)
				{
					continue;
				}

				if (modifierStringBuilder.Length > 0)
				{
					modifierStringBuilder.Append(" + ");
				}

				modifierStringBuilder.Append(modifierKey);
			}

			return string.Format("{0} + {1}", modifierStringBuilder, this.Key);
		}
	}
}
