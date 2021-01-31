using SharpDX;
using System;
using System.Collections.Generic;
using WizChess.Entities.Components;

namespace WizChess.Entities
{
	public class Entity
	{
		public Guid EntityId { get; }
		private Dictionary<int, Component> m_Components;

		public Entity()
		{
			EntityId = Guid.NewGuid();
			m_Components = new Dictionary<int, Component>();
		}

		~Entity()
		{
			m_Components.Clear();
		}

		public T AddComponent<T>(params object[] parameters) where T : Component, new()
		{
			int compId = typeof(T).GetHashCode();
			if (m_Components.ContainsKey(compId))
				return (T)m_Components[compId];

			T comp = (T)Activator.CreateInstance(typeof(T), parameters);
			m_Components.Add(compId, comp);
			return comp;
		}

		public T GetComponent<T>() where T : Component
		{
			int compId = typeof(T).GetHashCode();
			return m_Components.ContainsKey(compId) ? (T)m_Components[compId] : null;
		}

		public void OnUpdate(float ts)
		{
			foreach (Component comp in m_Components.Values)
				comp.OnUpdate(ts);
		}

		public Entity Instantiate(Vector3 position)
		{
			Entity entity = new Entity
			{
				m_Components = new Dictionary<int, Component>(m_Components)
			};

			entity.GetComponent<TransformComponent>().Position = position;

			return entity;
		}
	}
}
