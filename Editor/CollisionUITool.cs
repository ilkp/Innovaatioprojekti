
/*
 * GameObject containing models must the tagged with "AssetRoot"
 * Multiple GameObjects can be tagged
 * Disabled root objects, or their children (MeveaObjects), will be ignored.
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class CollisionUITool : EditorWindow
{
	private class GoId
	{
		public int Id { get; set; }
		public bool Active { get; set; }
		public SerializedObject SerializedColDetector { get; set; }
	}

	private const int SPACE = 20;
	private const int SPACE_MARGIN = 10;
	private const int ACTIVE_WIDTH = 60;
	private const int NAME_WIDTH = 200;
	private const int TOGGLE_WIDTH = 90;
	private const int TOGGLE_ALL_WIDTH = 40;
	private const float DOUBLE_CLICK_TIME = 0.2f;

	private Vector2 horizontalScollView;
	private Vector2 verticalScrollView;
	private bool dummyToggle = false;

	private int selectedRootObject = 0;
	private string[] rootObjectNames;
	private Dictionary<int, GoId[]> objectIds;

	private double clickTime = 0f;
	private bool executeFocus = false;
	private bool focusing = false;

	private readonly GUILayoutOption[] activeOptions = new GUILayoutOption[] { GUILayout.Width(ACTIVE_WIDTH) };
	private readonly GUILayoutOption[] toggleOptions = new GUILayoutOption[] { GUILayout.Width(TOGGLE_WIDTH) };
	private readonly GUILayoutOption[] nameOptions = new GUILayoutOption[] { GUILayout.Width(NAME_WIDTH) };
	private readonly GUILayoutOption[] toggleAllOptions = new GUILayoutOption[] { GUILayout.Width(TOGGLE_ALL_WIDTH) };
	private readonly GUILayoutOption[] ignoredColOptions = new GUILayoutOption[] { GUILayout.MinWidth(TOGGLE_WIDTH * 2), GUILayout.MaxWidth(TOGGLE_WIDTH * 4) };

	private Texture2D[] rowTex;
	private GUIStyle headerStyle;
	private GUIStyle headerStyleIgnoredCol;
	private GUIStyle rowStyle;


	[MenuItem("Mevea/Tools/CollisionUITool")]
	public static void OpenTool()
	{
		CollisionUITool collisionUiTool = (CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool));
		collisionUiTool.Show();
	}

	private void OnEnable() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; }
	private void OnDisable() { EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; }
	private void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
			InitBackgroundTex();
	}

	private void Awake()
	{
		CreateToggleContent();
		InitBackgroundTex();
		headerStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
		headerStyleIgnoredCol = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
		rowStyle = new GUIStyle();
		rowStyle.normal.background = rowTex[0];
	}

	private void OnHierarchyChange()
	{
		CreateToggleContent();
	}

	private void InitBackgroundTex()
	{
		rowTex = new Texture2D[2] { new Texture2D(1, 1), new Texture2D(1, 1) };
		rowTex[0].SetPixel(0, 0, Color.grey * 0.05f);
		rowTex[0].Apply();
		rowTex[1].SetPixel(0, 0, Color.clear);
		rowTex[1].Apply();
	}

	private void OnGUI()
	{
		if (objectIds == null)
			return;

		EditorGUILayout.BeginVertical();
		GUILayout.Space(SPACE_MARGIN);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(SPACE_MARGIN);
		horizontalScollView = EditorGUILayout.BeginScrollView(horizontalScollView, GUI.skin.horizontalScrollbar, GUIStyle.none);

		// Create selector for root object
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Root", new GUILayoutOption[] {
			GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Root")).x)});

		selectedRootObject = EditorGUILayout.Popup(selectedRootObject, rootObjectNames, new GUILayoutOption[] {
			GUILayout.ExpandWidth(false),
			GUILayout.MinWidth(80) });

		// Create selector for visual style
		GUILayout.Space(SPACE);
		EditorGUILayout.LabelField("Visual style", new GUILayoutOption[] {
			GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Visual style")).x) });

		Visuals.visualStyle = (VisualStyle)EditorGUILayout.EnumPopup(Visuals.visualStyle, new GUILayoutOption[] {
			GUILayout.ExpandWidth(false),
			GUILayout.MinWidth(80) });

		EditorGUILayout.EndHorizontal();

		// Create toggle header and toggle all/none buttons
		GUILayout.Space(SPACE);
		CreateHeader();
		CreateToggleAllButtons();
		GUILayout.Space(SPACE);

		// Create toggles based on selected root object
		verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
		for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
		{
			rowStyle.normal.background = rowTex[i % 2];
			CreateRow(objectIds[selectedRootObject][i]);
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndScrollView();

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		// Execute focus on game object if component button was double clicked
		if (executeFocus && !focusing)
		{
			focusing = true;
			executeFocus = false;
			EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
			focusing = false;
		}
	}

	private void CreateToggleContent()
	{
		objectIds = new Dictionary<int, GoId[]>();
		List<string> rootObjectNamesTemp = new List<string>();
		int rootIndex = 0;
		foreach (GameObject assetRoot in GameObject.FindGameObjectsWithTag("AssetRoot"))
		{
			foreach (GameObject rootGo in GetChildren(assetRoot))
			{
				CollisionDetector[] cds = rootGo.GetComponentsInChildren<CollisionDetector>(true);
				if (cds.Length == 0)
					continue;
				rootObjectNamesTemp.Add(rootGo.name);
				objectIds.Add(rootIndex, new GoId[cds.Length]);
				for (int i = 0; i < cds.Length; ++i)
				{
					objectIds[rootIndex][i] = new GoId { Id = cds[i].gameObject.GetInstanceID(), Active = cds[i].gameObject.activeInHierarchy };
					objectIds[rootIndex][i].SerializedColDetector = new SerializedObject(cds[i]);
				}
				rootIndex++;
			}
		}
		rootObjectNames = rootObjectNamesTemp.ToArray();
	}

	private void CreateHeader()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Active", headerStyle, activeOptions);
		EditorGUILayout.LabelField("Name", headerStyle, nameOptions);
		EditorGUILayout.LabelField("Collisions", headerStyle, toggleOptions);
		EditorGUILayout.LabelField("Sounds", headerStyle, toggleOptions);
		EditorGUILayout.LabelField("Visualization", headerStyle, toggleOptions);
		EditorGUILayout.LabelField("Ignored Colliders", headerStyleIgnoredCol, ignoredColOptions);
		EditorGUILayout.EndHorizontal();
	}

	private void CreateToggleAllButtons()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", activeOptions);
		EditorGUILayout.LabelField("", nameOptions);

		EditorGUILayout.BeginHorizontal(toggleOptions);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAll<CollisionDetector>(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAll<CollisionDetector>(false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(toggleOptions);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAll<CollisionSoundManager>(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAll<CollisionSoundManager>(false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(toggleOptions);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAll<Visuals>(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAll<Visuals>(false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndHorizontal();
	}

	private void CreateRow(GoId goId)
	{
		GameObject go = (GameObject)EditorUtility.InstanceIDToObject(goId.Id);
		EditorGUILayout.BeginHorizontal(rowStyle);

		EditorGUI.BeginChangeCheck();
		goId.Active = NoLabelToggle(goId.Active, activeOptions);
		if (EditorGUI.EndChangeCheck())
			go.SetActive(goId.Active);

		if (GUILayout.Button(go.name, nameOptions))
		{
			Selection.activeGameObject = go;
			if (!focusing && EditorApplication.timeSinceStartup - clickTime < DOUBLE_CLICK_TIME)
				executeFocus = true;
			clickTime = EditorApplication.timeSinceStartup;
		}
		CreateToggle<CollisionDetector>(go);
		CreateToggle<CollisionSoundManager>(go);
		CreateToggle<Visuals>(go);
		CreateColliderIgnoreArr(goId);
		EditorGUILayout.EndHorizontal();
	}

	private void CreateToggle<T>(GameObject go) where T : Behaviour
	{
		if (go.GetComponent<T>())
			go.GetComponent<T>().enabled = NoLabelToggle(go.GetComponent<T>().enabled, toggleOptions);
		else
		{
			GUI.enabled = false;
			dummyToggle = NoLabelToggle(dummyToggle, toggleOptions);
			GUI.enabled = true;
		}
	}

	private void CreateColliderIgnoreArr(GoId goId)
	{
		SerializedObject so = goId.SerializedColDetector;
		SerializedProperty ignoredColliders = so.FindProperty("ignoredColliders");
		EditorGUILayout.PropertyField(ignoredColliders, new GUIContent { text = "Size: " + ignoredColliders.arraySize }, true, ignoredColOptions);
		so.ApplyModifiedProperties();
	}

	private bool NoLabelToggle(bool value, GUILayoutOption[] horizontalOptions)
	{
		float originalLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 0.1f;
		GUILayout.BeginHorizontal(horizontalOptions);
		GUILayout.FlexibleSpace();
		value = EditorGUILayout.Toggle(value);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = originalLabelWidth;
		return value;
	}

	private void ToggleAll<T>(bool value) where T : Behaviour
	{
		GameObject go;
		for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
		{
			go = (GameObject)EditorUtility.InstanceIDToObject(objectIds[selectedRootObject][i].Id);
			if (go.GetComponent<T>())
				go.GetComponent<T>().enabled = value;
		}
	}

	private List<GameObject> GetChildren(GameObject go)
	{
		List<GameObject> children = new List<GameObject>();
		for (int i = 0; i < go.transform.childCount; ++i)
			children.Add(go.transform.GetChild(i).gameObject);
		return children;
	}
}