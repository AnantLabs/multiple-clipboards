using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MultipleClipboards
{
	/// <summary>
	/// All the possible Modifier Keys.
	/// </summary>
	public enum ModifierKeys
	{
		NONE = 0,
		ALT = 1,
		CONTROL = 2,
		SHIFT = 4,
		WINDOWS = 8
	}

	/// <summary>
	/// The Hot Key Types.
	/// </summary>
	public enum HotKeyType
	{
		UNDEFINED = 0,
		CUT = 1,
		COPY = 2,
		PASTE = 3
	}

	/// <summary>
	/// Class to hold data about a specific Hot Key.
	/// </summary>
	public class HotKey
	{
		List<string> modifierKeyCollection;
		private int _modifiers;

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		public HotKey()
		{
			this.Init(0, Keys.None, HotKeyType.UNDEFINED, 0);
		}

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		/// <param name="hotKey">A HotKey object to use as a template.</param>
		public HotKey(HotKey hotKey)
		{
			this.Init(hotKey.Modifiers, hotKey.Key, hotKey.Operation, hotKey.ClipboardID);
		}

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		/// <param name="modifiers">The modifier keys.</param>
		/// <param name="key">The key.</param>
		public HotKey(int modifiers, int key)
		{
			this.Init(modifiers, (Keys)key, HotKeyType.UNDEFINED, 0);
		}

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		/// <param name="modifier1">The first modifier key.</param>
		/// <param name="modifier2">The second modifier key.</param>
		/// <param name="key">The key.</param>
		/// <param name="operation">The operation for this hot key.</param>
		/// <param name="clipboardID">The ID of the clipboard that this hot key belongs to.</param>
		public HotKey(int modifier1, int modifier2, int key, HotKeyType operation, int clipboardID)
		{
			this.Init(modifier1 | modifier2, (Keys)key, operation, clipboardID);
		}

		/// <summary>
		/// Constructs a new HotKey object.
		/// </summary>
		/// <param name="modifier1">The first modifier key.</param>
		/// <param name="modifier2">The second modifier key.</param>
		/// <param name="key">The key.</param>
		/// <param name="operation">The operation for this hot key.</param>
		/// <param name="clipboardID">The ID of the clipboard that this hot key belongs to.</param>
		public HotKey(ModifierKeys modifier1, ModifierKeys modifier2, Keys key, HotKeyType operation, int clipboardID)
		{
			this.Init((int)modifier1 | (int)modifier2, key, operation, clipboardID);
		}

		/// <summary>
		/// Gets or sets the key for this hot key.
		/// </summary>
		public Keys Key
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the operation for this hot key.
		/// </summary>
		public HotKeyType Operation
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ID of the clipboard that this hot key belongs to.
		/// </summary>
		public int ClipboardID
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the modifier keys for this hot key.
		/// </summary>
		public int Modifiers
		{
			get
			{
				return this._modifiers;
			}
			set
			{
				this._modifiers = value;
				this.modifierKeyCollection.Clear();

				if ((this._modifiers & 0x0001) == 0x0001)
				{
					this.modifierKeyCollection.Add("Alt");
				}
				if ((this._modifiers & 0x0002) == 0x0002)
				{
					this.modifierKeyCollection.Add("Control");
				}
				if ((this._modifiers & 0x0004) == 0x0004)
				{
					this.modifierKeyCollection.Add("Shift");
				}
				if ((this._modifiers & 0x0008) == 0x0008)
				{
					this.modifierKeyCollection.Add("Windows");
				}
			}
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
		/// <returns>True if the two hot keys are equal, false if not.</returns>
		public static bool operator ==(HotKey lhs, HotKey rhs)
		{
			return (lhs.Modifiers == rhs.Modifiers && lhs.Key == rhs.Key);
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
			return (lhs.Modifiers != rhs.Modifiers || lhs.Key != rhs.Key);
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
			int sum = Modifiers + (int)Key;
			return sum.GetHashCode();
		}

		/// <summary>
		/// Gets a string representation of the HotKey.
		/// </summary>
		/// <returns>A string representation of the HotKey.</returns>
		public override string ToString()
		{
			string modifierString = "";
			foreach (string modifier in this.modifierKeyCollection)
			{
				modifierString += " + " + modifier;
			}

			if (modifierString.Length > 4)
			{
				modifierString = modifierString.Substring(3);
			}

			return string.Format("{0} + {1}", modifierString, this.Key.ToString());
		}

		/// <summary>
		/// Initializes the hot key.
		/// </summary>
		/// <param name="modifiers">The modifier keys.</param>
		/// <param name="key">The key.</param>
		/// <param name="operation">The operation for this hot key.</param>
		/// <param name="clipboardID">The ID of the clipboard that this hot key belongs to.</param>
		private void Init(int modifiers, Keys key, HotKeyType operation, int clipboardID)
		{
			this.modifierKeyCollection = new List<string>();
			this.Modifiers = modifiers;
			this.Key = key;
			this.Operation = operation;
			this.ClipboardID = clipboardID;
		}
	}
}
