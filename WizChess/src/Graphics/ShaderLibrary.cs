using System;
using System.Collections.Generic;

namespace WizChess.Graphics
{
	public static class ShaderLibrary
	{
		private static Dictionary<string, Shader> s_LoadedShaders = new Dictionary<string, Shader>();

		public static Shader Load(string name, string filepath)
		{
			if (s_LoadedShaders.ContainsKey(name))
				throw new ArgumentException("A shader with that name has already been loaded!");

			Shader shader = new Shader(filepath);
			s_LoadedShaders.Add(name, shader);
			return shader;
		}

		public static Shader Get(string name) => s_LoadedShaders[name];

		public static void Close()
		{
			foreach (Shader shader in s_LoadedShaders.Values)
				shader.Dispose();

			s_LoadedShaders.Clear();
		}
	}
}
