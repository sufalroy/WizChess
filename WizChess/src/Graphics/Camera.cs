using SharpDX;
using System;
using WizChess.Events;
using WizChess.Input;

namespace WizChess.Graphics
{
	public class Camera
	{
		public Matrix ProjectionMatrix { get; private set; }
		public Matrix ViewMatrix { get; private set; }
		public Matrix ViewProjectionMatrix { get; private set; }

		public Vector3 Position;

		public readonly float FieldOfView;

		private readonly float m_MaxVerticalAngle = MathUtil.DegreesToRadians(60.0F);
		private readonly float m_MinVerticalAngle = MathUtil.DegreesToRadians(-60.0F);

		private float m_VerticalAngle;
		private float m_HorizontalAngle;

		private readonly float m_MouseSpeed = 0.1F;
		private readonly float m_MovementSpeed = 4.0F;

		public Camera(float fov, float aspect)
		{
			FieldOfView = fov;
			ProjectionMatrix = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(FieldOfView), aspect, 0.1F, 100.0F);
			ViewMatrix = Matrix.Identity;
			ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;

			EventHooks.RegisterHook<ResizeEventArgs>(OnResize);
		}

		public void OnUpdate(float deltaTime)
		{
			if (!InputManager.IsMouseLocked)
				return;

			m_HorizontalAngle += m_MouseSpeed * deltaTime * InputManager.MouseDeltaX;
			m_VerticalAngle += m_MouseSpeed * deltaTime * InputManager.MouseDeltaY;
			m_VerticalAngle = MathUtil.Clamp(m_VerticalAngle, m_MinVerticalAngle, m_MaxVerticalAngle);

			Vector3 dir = new Vector3(
				(float)(System.Math.Cos(m_VerticalAngle) * System.Math.Sin(m_HorizontalAngle)),
				(float)System.Math.Sin(m_VerticalAngle),
				(float)(System.Math.Cos(m_VerticalAngle) * System.Math.Cos(m_HorizontalAngle))
			);

			Vector3 right = new Vector3(
				(float)System.Math.Sin(m_HorizontalAngle - System.Math.PI / 2.0F),
				0.0F,
				(float)System.Math.Cos(m_HorizontalAngle - System.Math.PI / 2.0F)
			);

			Vector3 up = Vector3.Cross(right, dir);

			if (InputManager.IsKeyDown(KeyCode.W))
			{
				Position += dir * deltaTime * m_MovementSpeed;
			}
			else if (InputManager.IsKeyDown(KeyCode.S))
			{
				Position -= dir * deltaTime * m_MovementSpeed;
			}

			if (InputManager.IsKeyDown(KeyCode.A))
			{
				Position += right * deltaTime * m_MovementSpeed;
			}
			else if (InputManager.IsKeyDown(KeyCode.D))
			{
				Position -= right * deltaTime * m_MovementSpeed;
			}

			if (InputManager.IsKeyDown(KeyCode.Space))
			{
				Position += Vector3.UnitY * deltaTime * m_MovementSpeed;
			}
			else if (InputManager.IsKeyDown(KeyCode.LeftShift))
			{
				Position -= Vector3.UnitY * deltaTime * m_MovementSpeed;
			}

			ViewMatrix = Matrix.LookAtLH(Position, Position + dir, up);
			ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
		}

		private void OnResize(object sender, EventArgs args)
		{
			ResizeEventArgs e = args as ResizeEventArgs;

			ProjectionMatrix = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(FieldOfView), (float)e.Width / e.Height, 0.1F, 100.0F);
			ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
		}
	}
}
