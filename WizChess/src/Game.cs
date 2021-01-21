using System;
using System.Drawing;
using SharpDX;
using SharpDX.Windows;
using WizChess.Events;
using System.Diagnostics;
using System.Windows.Forms;
using WizChess.Input;

namespace WizChess
{
    public class Game : IDisposable
    {
        private readonly RenderForm m_Window;

        public int Width => m_Window.ClientSize.Width;
        public int Height => m_Window.ClientSize.Height;

        private bool m_WasResized = true;

        public Game(int width, int height) 
        {
            m_Window = new RenderForm("WizChess")
            {
                ClientSize = new Size(width, height),
                AllowUserResizing = true
            };

            Configuration.EnableObjectTracking = true;
            Configuration.ThrowOnShaderCompileError = true;

            Init();
        }

        private void Init()
        {
            InputManager.Initialize();
            InputManager.IsMouseLocked = true;
        }

        public void Run() 
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RenderLoop.Run(m_Window, () => 
            {
                if (m_WasResized)
                {
                    EventHooks.CallHooks(new ResizeEventArgs(m_Window.ClientSize.Width, m_Window.ClientSize.Height));
                    m_WasResized = false;
                }

                float time = (float)stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();

                if (!InputManager.IsMouseLocked)
                    return;

                Cursor.Position = new System.Drawing.Point(Width / 2, Height / 2);

                if (InputManager.IsKeyDown(KeyCode.Escape))
                    InputManager.IsMouseLocked = false;

            });
        }

        public void Dispose()
        {
            InputManager.Close();
            m_Window.Dispose();
        }
    }
}
