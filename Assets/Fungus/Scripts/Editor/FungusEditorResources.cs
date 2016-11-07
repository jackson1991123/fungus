// This code is part of the Fungus library (http://fungusgames.com) maintained by Chris Gregan (http://twitter.com/gofungus).
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Fungus.EditorUtils
{
	internal static partial class FungusEditorResources
	{
		private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		
		static FungusEditorResources()
		{
			LoadResourceAssets();
		}

		private static void LoadResourceAssets()
		{
			// Get first folder named "Fungus Editor Resources"
			var rootGuid = AssetDatabase.FindAssets("\"Fungus Editor Resources\"")[0];
			var root = AssetDatabase.GUIDToAssetPath(rootGuid);
			var guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { root });
			var paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).OrderBy(path => path.ToLower().Contains("/pro/"));

			foreach (var path in paths)
			{
				if (path.ToLower().Contains("/pro/") && !EditorGUIUtility.isProSkin)
				{
					return;
				}
				var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
				textures[texture.name] = texture;
			}
		}

		[MenuItem("Tools/Fungus/Utilities/Update Editor Resources Script")]
		private static void GenerateResourcesScript()
		{
			var guid = AssetDatabase.FindAssets("FungusEditorResources t:MonoScript")[0];
			var relativePath = AssetDatabase.GUIDToAssetPath(guid).Replace("FungusEditorResources.cs", "FungusEditorResourcesGenerated.cs");
			var absolutePath = Application.dataPath + relativePath.Substring("Assets".Length);//
			
			using (var writer = new StreamWriter(absolutePath))
			{
				writer.WriteLine("// This code is part of the Fungus library (http://fungusgames.com) maintained by Chris Gregan (http://twitter.com/gofungus).");
				writer.WriteLine("// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)");
				writer.WriteLine("");				
				writer.WriteLine("using UnityEngine;");
				writer.WriteLine("");
				writer.WriteLine("namespace Fungus.EditorUtils");
				writer.WriteLine("{");
				writer.WriteLine("\tinternal static partial class FungusEditorResources");
				writer.WriteLine("\t{");

				foreach (var pair in textures)
				{
					var name = pair.Key;
					var pascalCase = string.Join("", name.Split(new [] { '_' }, StringSplitOptions.RemoveEmptyEntries).Select(
						s => s.Substring(0, 1).ToUpper() + s.Substring(1)).ToArray()
					);
					writer.WriteLine("\t\tpublic static Texture2D " + pascalCase + " { get { return textures[\"" + name + "\"]; } }");
				}

				writer.WriteLine("\t}");
				writer.WriteLine("}");
			}

			AssetDatabase.ImportAsset(relativePath);
		}
	}
}
