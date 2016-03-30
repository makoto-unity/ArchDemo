using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildLightmap {

	[MenuItem("Window/Bake Lightmap All")]
	static void CreateLightmapAllScene()
	{
		string nowSceneName = EditorSceneManager.GetActiveScene ().path;
		EditorSceneManager.OpenScene ("Assets/Arch03.unity");
		Lightmapping.Bake ();
		EditorSceneManager.OpenScene (nowSceneName);
	}

}
