using System;
using System.Drawing;
using SharpDX;
using SharpDX.Windows;
using WizChess.Graphics;
using WizChess.Events;
using WizChess.Entities.Components;
using WizChess.Entities;
using System.Diagnostics;
using System.Windows.Forms;
using WizChess.Input;
using SharpDX.Direct3D11;

namespace WizChess
{
	public class Game : IDisposable
	{
		private readonly RenderForm m_Window;

		public int Width => m_Window.ClientSize.Width;
		public int Height => m_Window.ClientSize.Height;

		private bool m_WasResized = true;

		private Entity m_PlayerInstance;

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

		private Texture2D texture;

		private void Init()
		{
			InputManager.Initialize();
			InputManager.IsMouseLocked = true;

			Renderer.Initialize(m_Window);

			Entity template = new Entity();
			template.AddComponent<TransformComponent>();
			template.AddComponent<MeshComponent>(new Mesh("res/Meshes/Scene.fbx"));
			CameraComponent camera = template.AddComponent<CameraComponent>(80.0F, (float)Width / Height);
			camera.MyCamera.Position = new Vector3(0.0F, 10.0F, -20.0F);

			m_PlayerInstance = template.Instantiate(new Vector3(0.0F, 0.0F, 0.0F));
			TransformComponent transform = m_PlayerInstance.GetComponent<TransformComponent>();
			transform.Rotation = new Vector3(-90.0F, 0.0F, 0.0F);

			texture = TextureLoader.LoadFromFile("res/Textures/Test.png");
			ShaderLibrary.Get("Basic").Set("texture0", texture);
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

				TransformComponent playerTransform = m_PlayerInstance.GetComponent<TransformComponent>();
				playerTransform.Rotation.Y += 0.5F;

				m_PlayerInstance.OnUpdate(time);

				Renderer.Clear(SharpDX.Color.Black);

				Renderer.BeginDraw(m_PlayerInstance.GetComponent<CameraComponent>());
				Renderer.Submit(m_PlayerInstance.GetComponent<MeshComponent>(), playerTransform);
				Renderer.EndDraw();

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
			Renderer.Close();
			m_Window.Dispose();
		}
	}
}
