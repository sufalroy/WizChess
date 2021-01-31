using System;

namespace WizChess.Events
{

	public class ResizeEventArgs : EventArgs
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		public ResizeEventArgs(int width, int height)
		{
			Width = width;
			Height = height;
		}
	}
}
