using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX.Multimedia;
using SharpDX.RawInput;

namespace WizChess.Input
{
	public static class InputManager
	{
		public static int MouseDeltaX
		{
			get
			{
				int temp = s_MouseDeltaX;
				s_MouseDeltaX = 0;
				return temp;
			}
		}
		public static int MouseDeltaY
		{
			get
			{
				int temp = s_MouseDeltaY;
				s_MouseDeltaY = 0;
				return temp;
			}
		}

		public static int MouseWheelDelta
		{
			get
			{
				int temp = s_MouseWheelDelta;
				s_MouseWheelDelta = 0;
				return temp;
			}
		}

		public static bool IsMouseLocked
		{
			get { return s_IsMouseLocked; }
			set
			{
				s_IsMouseLocked = value;

				if (s_IsMouseLocked)
					Cursor.Hide();
				else
					Cursor.Show();
			}
		}

		private static int s_MouseDeltaX;
		private static int s_MouseDeltaY;
		private static int s_MouseWheelDelta;

		private static bool s_IsMouseLocked;

		private static readonly Dictionary<KeyCode, KeyState> s_KeyStates = new Dictionary<KeyCode, KeyState>();

		public static void Initialize()
		{
			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
			Device.MouseInput += MouseHandler;

			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
			Device.KeyboardInput += KeyboardHandler;
		}

		public static void Close()
		{
			Device.MouseInput -= MouseHandler;
			Device.KeyboardInput -= KeyboardHandler;
		}

		public static bool IsKeyDown(KeyCode key)
		{
			return s_KeyStates.ContainsKey(key) && s_KeyStates[key] == KeyState.KeyDown;
		}

		private static void MouseHandler(object sender, MouseInputEventArgs args)
		{
			s_MouseDeltaX += args.X;
			s_MouseDeltaY += 1 - args.Y;
			s_MouseWheelDelta = args.WheelDelta;
		}

		private static void KeyboardHandler(object sender, KeyboardInputEventArgs args)
		{
			KeyCode keyCode = ToCustomKeyCode(args.Key);
			if (s_KeyStates.ContainsKey(keyCode))
				s_KeyStates[keyCode] = args.State;
			else
				s_KeyStates.Add(keyCode, args.State);
		}

		#region KeyCodeConversion
		private static KeyCode ToCustomKeyCode(Keys key)
		{
			switch (key)
			{
				case Keys.None: return KeyCode.None;
				case Keys.Back: return KeyCode.Backpspace;
				case Keys.Tab: return KeyCode.Tab;
				case Keys.Return: return KeyCode.Enter;
				case Keys.Escape: return KeyCode.Escape;
				case Keys.Space: return KeyCode.Space;
				case Keys.Left: return KeyCode.LeftArrow;
				case Keys.Up: return KeyCode.UpArrow;
				case Keys.Right: return KeyCode.RightArrow;
				case Keys.Down: return KeyCode.DownArrow;
				case Keys.Delete: return KeyCode.Delete;
				case Keys.D0: return KeyCode.Alpha0;
				case Keys.D1: return KeyCode.Alpha1;
				case Keys.D2: return KeyCode.Alpha2;
				case Keys.D3: return KeyCode.Alpha3;
				case Keys.D4: return KeyCode.Alpha4;
				case Keys.D5: return KeyCode.Alpha5;
				case Keys.D6: return KeyCode.Alpha6;
				case Keys.D7: return KeyCode.Alpha7;
				case Keys.D8: return KeyCode.Alpha8;
				case Keys.D9: return KeyCode.Alpha9;
				case Keys.A: return KeyCode.A;
				case Keys.B: return KeyCode.B;
				case Keys.C: return KeyCode.C;
				case Keys.D: return KeyCode.D;
				case Keys.E: return KeyCode.E;
				case Keys.F: return KeyCode.F;
				case Keys.G: return KeyCode.G;
				case Keys.H: return KeyCode.H;
				case Keys.I: return KeyCode.I;
				case Keys.J: return KeyCode.J;
				case Keys.K: return KeyCode.K;
				case Keys.L: return KeyCode.L;
				case Keys.M: return KeyCode.M;
				case Keys.N: return KeyCode.N;
				case Keys.O: return KeyCode.O;
				case Keys.P: return KeyCode.P;
				case Keys.Q: return KeyCode.Q;
				case Keys.R: return KeyCode.R;
				case Keys.S: return KeyCode.S;
				case Keys.T: return KeyCode.T;
				case Keys.U: return KeyCode.U;
				case Keys.V: return KeyCode.V;
				case Keys.W: return KeyCode.W;
				case Keys.X: return KeyCode.X;
				case Keys.Y: return KeyCode.Y;
				case Keys.Z: return KeyCode.Z;
				case Keys.NumPad0: return KeyCode.Keypad0;
				case Keys.NumPad1: return KeyCode.Keypad1;
				case Keys.NumPad2: return KeyCode.Keypad2;
				case Keys.NumPad3: return KeyCode.Keypad3;
				case Keys.NumPad4: return KeyCode.Keypad4;
				case Keys.NumPad5: return KeyCode.Keypad5;
				case Keys.NumPad6: return KeyCode.Keypad6;
				case Keys.NumPad7: return KeyCode.Keypad7;
				case Keys.NumPad8: return KeyCode.Keypad8;
				case Keys.NumPad9: return KeyCode.Keypad9;
				case Keys.F1: return KeyCode.F1;
				case Keys.F2: return KeyCode.F2;
				case Keys.F3: return KeyCode.F3;
				case Keys.F4: return KeyCode.F4;
				case Keys.F5: return KeyCode.F5;
				case Keys.F6: return KeyCode.F6;
				case Keys.F7: return KeyCode.F7;
				case Keys.F8: return KeyCode.F8;
				case Keys.F9: return KeyCode.F9;
				case Keys.F10: return KeyCode.F10;
				case Keys.F11: return KeyCode.F11;
				case Keys.F12: return KeyCode.F12;
				case Keys.ShiftKey: return KeyCode.LeftShift;
			}

			return KeyCode.None;
		}
		#endregion

	}
}
