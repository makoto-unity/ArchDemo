using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class OneStepEffect : ScriptableWizard {
	// Light
	private bool isLightSettings = true;
	private float lightAmbientIntensity = 0.65f;
	private float lightReflectionIntensity = 0.18f;
	private float lightRealtimeResolution = 2.0f;
	private float lightDirectionalIntensity = 0.18f;

	// Normal map
	private bool isCreateNormalMap = true;
	private float myBumpScale = 0.25f;

	// SSAO
	private bool isApplySSAO = true;
	private string myClassSSAO = "Obscurance";

	// SSR
	private bool isApplySSR = true;
	private string myClassSSR = "ScreenSpaceReflection";

	// Bloom
	private bool isApplyBloom = true;
	private string myClassBloom = "BloomOptimized";
	private float myBloomThreshold = 0.7f;

	// Antialiasing
	private bool isApplyAA = true;
	private string myClassAA = "Antialiasing";
	private UnityStandardAssets.ImageEffects.AAMode myAAMode = UnityStandardAssets.ImageEffects.AAMode.FXAA2;

	// Etc
	private GameObject myCameraObj = null;
	private GameObject myRootObj = null;
	private GameObject myLightObj = null;
	private Material mySkybox = null;

	[MenuItem ("Window/One Step Effect")]
	static void CreateWizard () {
		ScriptableWizard.DisplayWizard<OneStepEffect>("Create Light", "Done", "Apply");
		//If you don't want to use the secondary button simply leave it out:
		//ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
	}

	void OnWizardCreate () {
		Debug.Log("done!");
	}

	// First update
	void OnWizardUpdate () {
		helpString = "Please set the color of the light!";
		Debug.Log("updated");
	}

	void OnGUI()
	{
		myRootObj   = EditorGUILayout.ObjectField( "Root Object", myRootObj, typeof( GameObject ), true ) as GameObject;
		myCameraObj = EditorGUILayout.ObjectField( "Camera Object", myCameraObj, typeof( GameObject ), true ) as GameObject;
		myLightObj  = EditorGUILayout.ObjectField( "Light Object", myLightObj, typeof( GameObject ), true ) as GameObject;
		mySkybox    = EditorGUILayout.ObjectField( "Skybox", mySkybox, typeof( Material ), true ) as Material;

		isLightSettings = EditorGUILayout.BeginToggleGroup( "Lighting settings", isLightSettings );
		EditorGUI.indentLevel = 1;
		{
			lightAmbientIntensity = EditorGUILayout.FloatField( "Ambient Intensity", lightAmbientIntensity );
			lightReflectionIntensity = EditorGUILayout.FloatField( "Reflection Intensity", lightReflectionIntensity );
			lightRealtimeResolution = EditorGUILayout.FloatField( "Realtime Resolution", lightRealtimeResolution );
			lightDirectionalIntensity = EditorGUILayout.FloatField( "DirectLight Intensity", lightDirectionalIntensity );
		}
		EditorGUI.indentLevel = 0;
		EditorGUILayout.EndToggleGroup();

		isCreateNormalMap = EditorGUILayout.BeginToggleGroup("Create Normal map", isCreateNormalMap);
		EditorGUI.indentLevel = 1;
		{
			myBumpScale = EditorGUILayout.FloatField( "Normal scale", myBumpScale );
		}
		EditorGUI.indentLevel = 0;
		EditorGUILayout.EndToggleGroup();

		isApplySSAO = EditorGUILayout.BeginToggleGroup( "Apply SSAO", isApplySSAO );
		EditorGUILayout.EndToggleGroup();

		isApplySSR = EditorGUILayout.BeginToggleGroup( "Apply SSR", isApplySSR );
		EditorGUILayout.EndToggleGroup();

		isApplyBloom = EditorGUILayout.BeginToggleGroup( "Apply Bloom", isApplyBloom );
		EditorGUI.indentLevel = 1;
		{
			myBloomThreshold = EditorGUILayout.FloatField( "Threshold", myBloomThreshold );
		}
		EditorGUI.indentLevel = 0;
		EditorGUILayout.EndToggleGroup();

		isApplySSR = EditorGUILayout.BeginToggleGroup( "Apply AA", isApplyAA );
		EditorGUI.indentLevel = 1;
		{
			myAAMode = (UnityStandardAssets.ImageEffects.AAMode)EditorGUILayout.EnumPopup( "Mode", myAAMode );
		}
		EditorGUI.indentLevel = 0;
		EditorGUILayout.EndToggleGroup();

		GUILayout.FlexibleSpace();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Done")) {
			Debug.Log("Done");
		}
		if (GUILayout.Button("Apply")) {
			DoApply();
		}
		EditorGUILayout.EndHorizontal();
	}

	// Apply Button
	void DoApply () {
		Debug.Log("apply");
		if ( myCameraObj == null ) 
		{
			Camera cam = GameObject.FindObjectOfType<Camera>();
			if ( cam == null ) {
				Debug.LogError("Can't find any camera");
				return;
			}
			cam.hdr = true;
			cam.renderingPath = RenderingPath.DeferredShading;
			myCameraObj = cam.gameObject;
		}
		if ( myLightObj == null ) 
		{
			Light [] lights = GameObject.FindObjectsOfType<Light>();
			foreach( Light light in lights )
			{
				if ( light.type == LightType.Directional ) 
				{
					myLightObj = light.gameObject;
				}
			}
			if ( myLightObj == null ) 
			{
				Debug.LogError("Can't find any light");
				return;
			}
		}
		if (mySkybox == null)
		{
			string [] skyboxHashes = AssetDatabase.FindAssets("t:Material");
			if ( skyboxHashes.Length == 0 ) 
			{
				Debug.LogError("Can't find any skybox");
				return;
			}
			foreach( string skyboxHash in skyboxHashes )
			{
				string path = AssetDatabase.GUIDToAssetPath( skyboxHash );
				Debug.Log(path);
				Material mat = AssetDatabase.LoadAssetAtPath<Material>( path );
				if ( mat.shader.name.Contains( "Skybox" ) ) 
				{
					mySkybox = mat;
					break;
				}
			}
			if ( mySkybox == null ) 
			{
				Debug.LogError("Can't find any skybox");
				return;
			}
		}

		if ( isLightSettings )
		{
			LightSettings();
		}

		if ( isCreateNormalMap ) 
		{
			CreateNormalMap();
		}

		if ( isApplySSAO ) 
		{
			ApplySSAO();
		}

		if ( isApplySSR )
		{
			ApplySSR();
		}

		if ( isApplyBloom ) 
		{
			ApplyBloom();
		}

		if ( isApplyAA )
		{
			ApplyAA();
		}
	}

	private void ApplyAA()
	{
		Object cmp = myCameraObj.GetComponent(myClassAA);
		if ( cmp == null ) 
		{
			cmp = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(
				myCameraObj, "Assets/Editor/OneStepEffects.cs (203,10)", myClassAA);
		}
		UnityStandardAssets.ImageEffects.Antialiasing aa = cmp as UnityStandardAssets.ImageEffects.Antialiasing;
		aa.mode = myAAMode;
	}

	private void LightSettings()
	{
		Renderer [] renderers;
		if ( myRootObj == null ) 
		{
			renderers = GameObject.FindObjectsOfType<Renderer>();
		}
		else 
		{
			myRootObj.isStatic = true;
			renderers = myRootObj.GetComponentsInChildren<Renderer>();
		}
		foreach( Renderer renderer in renderers ) 
		{
			renderer.gameObject.isStatic = true;
		}
		Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
		LightmapEditorSettings.resolution = lightRealtimeResolution;
		RenderSettings.ambientIntensity = lightAmbientIntensity;
		RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
		RenderSettings.skybox = mySkybox;
		myLightObj.GetComponent<Light>().intensity = lightDirectionalIntensity;

//		LightmapParameters.clusterResolution = 0.123f;
		Lightmapping.BakeAsync();
	}

	private void ApplyBloom()
	{
		Object cmp = myCameraObj.GetComponent(myClassBloom);
		if ( cmp == null ) 
		{
			cmp = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(
				myCameraObj, "Assets/Editor/OneStepEffects.cs (240,10)", myClassBloom);
		}
		UnityStandardAssets.ImageEffects.BloomOptimized opt = cmp as UnityStandardAssets.ImageEffects.BloomOptimized;
		opt.threshold = myBloomThreshold;
	}

	private void ApplySSR()
	{
		
		Object cmp = myCameraObj.GetComponent(myClassSSR);
		if ( cmp == null ) 
		{
			cmp = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(
				myCameraObj, "Assets/Editor/OneStepEffects.cs (251,10)", myClassSSR);
		}
	}

	private void ApplySSAO()
	{
		Object cmp = myCameraObj.GetComponent(myClassSSAO);
		if ( cmp == null ) 
		{
			cmp = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(
				myCameraObj, "Assets/Editor/OneStepEffects.cs (261,10)", myClassSSAO );
		}

	}

	private void CreateNormalMap()
	{
		Renderer [] renderers;
		if ( myRootObj == null ) 
		{
			renderers = GameObject.FindObjectsOfType<Renderer>();
		}
		else 
		{
			renderers = myRootObj.GetComponentsInChildren<Renderer>();
		}
		foreach( Renderer renderer in renderers ) 
		{
			foreach( Material mat in renderer.sharedMaterials ) 
			{
				if ( mat.shader.name == "Standard" ) 
				{
					Texture tex = mat.GetTexture("_MainTex");
					if ( tex != null ) {
						Texture nmlTex = mat.GetTexture("_BumpMap");
						if ( nmlTex == null ) {
							string path = AssetDatabase.GetAssetPath( tex );
							string nmlFile = Path.GetFileNameWithoutExtension(path);
							string dirPath = Path.GetDirectoryName(path);
							string extPath = Path.GetExtension(path);
							//AssetDatabase.AssetPathToGUID(
							string nmlPath = dirPath + "/" + nmlFile + "_nml" + extPath;
							Debug.Log( nmlPath );
							if ( AssetImporter.GetAtPath(nmlPath) == null ) {
								AssetDatabase.CopyAsset( path, nmlPath );
							}
							TextureImporter ti = AssetImporter.GetAtPath(nmlPath) as TextureImporter;
							ti.textureType = TextureImporterType.Bump;
//							ti.normalmap = true;
//							ti.normalmapFilter = TextureImporterNormalFilter.Sobel;
							ti.convertToNormalmap = true;
//							TextureImporterSettings settings = new TextureImporterSettings();
//							ti.ReadTextureSettings(settings);
//							settings.ApplyTextureType(TextureImporterType.Bump, false);
//							ti.SetTextureSettings(settings);
							AssetDatabase.ImportAsset(nmlPath, ImportAssetOptions.ForceUpdate);
							nmlTex = AssetDatabase.LoadAssetAtPath<Texture>( nmlPath );
							Debug.Log( nmlTex.name );
							mat.SetTexture("_BumpMap", nmlTex );
							mat.SetFloat("_BumpScale", myBumpScale);
						}
					}
				}
			}
		}

//		if (Selection.activeTransform != null) {
//			Light lt = Selection.activeTransform.GetComponent<Light>();
//
//			if (lt != null) {
//				lt.color = Color.red;
//			}
//		}
	}
}