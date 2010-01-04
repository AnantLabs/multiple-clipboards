using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MultipleClipboards
{
	public enum ModifierKeys
	{
		NONE = 0,
		ALT = 1,
		CONTROL = 2,
		SHIFT = 4,
		WINDOWS = 8
	}

	public enum HotKeyType
	{
		UNDEFINED = 0,
		CUT = 1,
		COPY = 2,
		PASTE = 3
	}

	public class HotKey
	{
		public Keys Key
		{
			get;
			set;
		}
		public HotKeyType Operation
		{
			get;
			set;
		}
		public int ClipboardID
		{
			get;
			set;
		}

		List<string> modifierKeyCollection;
		private int _modifiers;
		public int Modifiers
		{
			get
			{
				return _modifiers;
			}
			set
			{
				_modifiers = value;
				modifierKeyCollection.Clear();
				if ((_modifiers & 0x0001) == 0x0001)
					modifierKeyCollection.Add("Alt");
				if ((_modifiers & 0x0002) == 0x0002)
					modifierKeyCollection.Add("Control");
				if ((_modifiers & 0x0004) == 0x0004)
					modifierKeyCollection.Add("Shift");
				if ((_modifiers & 0x0008) == 0x0008)
					modifierKeyCollection.Add("Windows");
			}
		}

		public HotKey()
		{
			Init(0, Keys.None, HotKeyType.UNDEFINED, 0);
		}

		public HotKey(HotKey hotKey)
		{
			Init(hotKey.Modifiers, hotKey.Key, hotKey.Operation, hotKey.ClipboardID);
		}

		public HotKey(int modifiers, int key)
		{
			Init(modifiers, (Keys)key, HotKeyType.UNDEFINED, 0);
		}

		public HotKey(int modifier1, int modifier2, int key, HotKeyType operation, int clipboardID)
		{
			Init(modifier1 | modifier2, (Keys)key, operation, clipboardID);
		}

		public HotKey(ModifierKeys modifier1, ModifierKeys modifier2, Keys key, HotKeyType operation, int clipboardID)
		{
			Init((int)modifier1 | (int)modifier2, key, operation, clipboardID);
		}

		private void Init(int modifiers, Keys key, HotKeyType operation, int clipboardID)
		{
			modifierKeyCollection = new List<string>();
			Modifiers = modifiers;
			Key = key;
			Operation = operation;
			ClipboardID = clipboardID;
		}

		// A hotkey is equal to another hotkey when the modifers and keys match
		// the other properties are ignored because when a hotkey is pressed this is the only information we know about it
		// we must use the keys to match it to the real hotkey that contains the clipboard and operation information
		public static bool operator ==(HotKey lhs, HotKey rhs)
		{
			return (lhs.Modifiers == rhs.Modifiers && lhs.Key == rhs.Key);
		}

		public static bool operator !=(HotKey lhs, HotKey rhs)
		{
			return (lhs.Modifiers != rhs.Modifiers || lhs.Key != rhs.Key);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is HotKey))
				return false;

			return this == (HotKey)obj;
		}

		public override int GetHashCode()
		{
			int sum = Modifiers + (int)Key;
			return sum.GetHashCode();
		}

		public override string ToString()
		{
			string modifierString = "";
			foreach (string modifier in modifierKeyCollection)
			{
				modifierString += " + " + modifier;
			}
			if (modifierString.Length > 4)
				modifierString = modifierString.Substring(3);

			return string.Format("{0} + {1}", modifierString, Key.ToString());
		}
	}
}
