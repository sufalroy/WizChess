using SharpDX;
using WizChess.Graphics;

namespace WizChess.Entities.Components
{
	public class Component
	{
		public string Name { get; }

		protected Component(string name)
		{
			Name = name;
		}

		public virtual void OnUpdate(float ts) { }
	}

	public class TransformComponent : Component
	{
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Scale;
		public Matrix TransformMatrix { get; private set; }

		public TransformComponent()
			: this(Vector3.Zero) { }

		public TransformComponent(Vector3 position)
			: this(position, Vector3.Zero)
		{ }

		public TransformComponent(Vector3 position, Vector3 rotation)
			: this(position, rotation, Vector3.One)
		{ }

		public TransformComponent(Vector3 position, Vector3 rotation, Vector3 scale)
			: base("TransformComponent")
		{
			Position = position;
			Rotation = rotation;
			Scale = scale;
		}

		public TransformComponent(TransformComponent other)
			: base("TransformComponent")
		{
			Position = other.Position;
			Rotation = other.Rotation;
			Scale = other.Scale;
		}

		public override void OnUpdate(float ts)
		{
			TransformMatrix = Matrix.Scaling(Scale) *
				Matrix.RotationX(MathUtil.DegreesToRadians(Rotation.X)) *
				Matrix.RotationY(MathUtil.DegreesToRadians(Rotation.Y)) *
				Matrix.RotationZ(MathUtil.DegreesToRadians(Rotation.Z)) *
				Matrix.Translation(Position);
		}

		public static implicit operator Matrix(TransformComponent value)
		{
			return value.TransformMatrix;
		}
	}

	public class MeshComponent : Component
	{
		public Mesh MyMesh { get; private set; }

		public MeshComponent()
			: this(null) { }

		public MeshComponent(Mesh mesh)
			: base("MeshComponent")
		{
			MyMesh = mesh;
		}

		~MeshComponent()
		{
			MyMesh.Dispose();
		}

		public static implicit operator Mesh(MeshComponent value)
		{
			return value.MyMesh;
		}
	}

	public class CameraComponent : Component
	{
		public Camera MyCamera { get; private set; }

		public CameraComponent()
			: base("CameraComponent")
		{
			MyCamera = null;
		}

		public CameraComponent(float fov, float aspect)
			: base("CameraComponent")
		{
			MyCamera = new Camera(fov, aspect);
		}

		public override void OnUpdate(float ts)
		{
			MyCamera?.OnUpdate(ts);
		}

		public static implicit operator Camera(CameraComponent value)
		{
			return value.MyCamera;
		}
	}
}
