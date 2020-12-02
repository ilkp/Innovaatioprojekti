
/*
 * Root GameObjects containing models must the tagged with "AssetRoot"
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CollisionUITool : EditorWindow
{
	private class GoId
	{
		public int Id { get; set; }
		public bool Active { get; set; }
		public SerializedObject SerializedColDetector { get; set; }
	}

	private const float SPACE = 20f;
	private const float SPACE_HALF = SPACE / 2f;
	private const float NAME_WIDTH = 200f;
	private const float TOGGLE_WIDTH = 75f;
	private const float TOGGLE_ALL_WIDTH = 30f;
	private const float COLOR_WIDTH = 75f;
	private const float SERIALIZED_PROPERTY_MIN_WIDTH = 260f;
	private const float DOUBLE_CLICK_TIME = 0.2f;

	private Vector2 horizontalScollView;
	private Vector2 verticalScrollView;
	private bool dummyToggle = false;
	private Color dummyColor = Color.gray;

	private int selectedRootObject = 0;
	private string[] rootObjectNames;
	private Dictionary<int, GoId[]> objectIds;

	private double clickTime = 0f;
	private bool executeFocus = false;
	private bool focusing = false;

	private readonly GUILayoutOption[] toggleOptions = new GUILayoutOption[] { GUILayout.Width(TOGGLE_WIDTH) };
	private readonly GUILayoutOption[] nameOptions = new GUILayoutOption[] { GUILayout.Width(NAME_WIDTH) };
	private readonly GUILayoutOption[] toggleAllOptions = new GUILayoutOption[] { GUILayout.Width(TOGGLE_ALL_WIDTH) };
	private readonly GUILayoutOption[] colorOptions = new GUILayoutOption[] { GUILayout.Width(COLOR_WIDTH * 2f) };
	private readonly GUILayoutOption[] ignoredColOptions = new GUILayoutOption[] { GUILayout.MinWidth(SERIALIZED_PROPERTY_MIN_WIDTH), GUILayout.ExpandWidth(true) };

	private Texture2D[] rowTex;
	private GUIStyle headerStyleCentered;
	private GUIStyle headerStyleLeft;
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
		FindToggleContent();
		InitBackgroundTex();
		headerStyleCentered = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
		headerStyleLeft = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
		rowStyle = new GUIStyle();
		rowStyle.normal.background = rowTex[0];
	}

	private void OnHierarchyChange()
	{
		FindToggleContent();
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

		GUILayout.Space(SPACE_HALF);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(SPACE_HALF);
		horizontalScollView = EditorGUILayout.BeginScrollView(horizontalScollView, GUI.skin.horizontalScrollbar, GUIStyle.none);

		// Header
		CreateSelectors();
		GUILayout.Space(SPACE);
		CreateLabels();
		CreateToggleAllButtons();
		GUILayout.Space(SPACE_HALF);

		// Toggle body based on selected root object
		verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
		for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
		{
			rowStyle.normal.background = rowTex[i % 2];
			CreateRow(objectIds[selectedRootObject][i]);
		}

		EditorGUILayout.EndScrollView();
		GUILayout.Space(SPACE_HALF);
		EditorGUILayout.EndScrollView();
		GUILayout.Space(SPACE_HALF);
		EditorGUILayout.EndHorizontal();

		// Execute focus on game object if name button was double clicked
		if (executeFocus && !focusing)
		{
			focusing = true;
			executeFocus = false;
			EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
			focusing = false;
		}
	}

	private void FindToggleContent()
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

	private void CreateSelectors()
	{
		// Selector for root object
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Root", new GUILayoutOption[] {
			GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Root")).x)});
		selectedRootObject = EditorGUILayout.Popup(selectedRootObject, rootObjectNames, new GUILayoutOption[] {
			GUILayout.ExpandWidth(false),
			GUILayout.MinWidth(80) });

		// Selector for visual style
		GUILayout.Space(SPACE);
		EditorGUILayout.LabelField("Visual style", new GUILayoutOption[] {
			GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Visual style")).x) });
		Visuals.visualStyle = (VisualStyle)EditorGUILayout.EnumPopup(Visuals.visualStyle, new GUILayoutOption[] {
			GUILayout.ExpandWidth(false),
			GUILayout.MinWidth(80) });
		EditorGUILayout.EndHorizontal();
	}

	private void CreateLabels()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Active", headerStyleCentered, toggleOptions);
		EditorGUILayout.LabelField("Name", headerStyleCentered, nameOptions);
		EditorGUILayout.LabelField("Collisions", headerStyleCentered, toggleOptions);
		EditorGUILayout.LabelField("Sounds", headerStyleCentered, toggleOptions);
		EditorGUILayout.LabelField("Visualization", headerStyleCentered, toggleOptions);
		EditorGUILayout.LabelField("Color", headerStyleCentered, colorOptions);
		EditorGUILayout.LabelField("Ignored Colliders", headerStyleLeft, ignoredColOptions);
		EditorGUILayout.EndHorizontal();
	}

	private void CreateToggleAllButtons()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", toggleOptions);
		EditorGUILayout.LabelField("", nameOptions);
		CreateToggleAllButton<CollisionDetector>();
		CreateToggleAllButton<CollisionSoundManager>();
		CreateToggleAllButton<Visuals>();
		EditorGUILayout.EndHorizontal();
	}

	private void CreateRow(GoId goId)
	{
		GameObject go = (GameObject)EditorUtility.InstanceIDToObject(goId.Id);
		EditorGUILayout.BeginHorizontal(rowStyle);

		EditorGUI.BeginChangeCheck();
		goId.Active = NoLabelToggle(goId.Active, toggleOptions);
		if (EditorGUI.EndChangeCheck())
			go.SetActive(goId.Active);

		if (GUILayout.Button(go.name, nameOptions))
		{
			Selection.activeGameObject = go;
			if (!focusing && EditorApplication.timeSinceStartup - clickTime < DOUBLE_CLICK_TIME)
				executeFocus = true;
			clickTime = EditorApplication.timeSinceStartup;
		}

		CreateToggle(go.GetComponent<CollisionDetector>());
		CreateToggle(go.GetComponent<CollisionSoundManager>());
		CreateToggle(go.GetComponent<Visuals>());
		CreateColorPicker(go.GetComponent<Visuals>());
		CreateColliderIgnoreArr(goId);
		EditorGUILayout.EndHorizontal();
	}

	private void CreateToggle<T>(T component) where T : Behaviour
	{
		if (component)
			component.enabled = NoLabelToggle(component.enabled, toggleOptions);
		else
		{
			GUI.enabled = false;
			dummyToggle = NoLabelToggle(dummyToggle, toggleOptions);
			GUI.enabled = true;
		}
	}
	private void CreateToggleAllButton<T>() where T : Behaviour
	{
		EditorGUILayout.BeginHorizontal(toggleOptions);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("on", toggleAllOptions)) ToggleAll<T>(true);
		if (GUILayout.Button("off", toggleAllOptions)) ToggleAll<T>(false);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	private void CreateColorPicker(Visuals visuals)
	{
		EditorGUILayout.BeginHorizontal(colorOptions);
		GUILayout.FlexibleSpace();
		if (visuals != null)
			visuals.color = EditorGUILayout.ColorField(visuals.color, GUILayout.Width(COLOR_WIDTH));
		else
		{
			GUI.enabled = false;
			dummyColor = EditorGUILayout.ColorField(dummyColor, GUILayout.Width(COLOR_WIDTH));
			GUI.enabled = true;
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	private void CreateColliderIgnoreArr(GoId goId)
	{
		SerializedProperty ignoredColliders = goId.SerializedColDetector.FindProperty("ignoredColliders");
		EditorGUILayout.PropertyField(ignoredColliders, new GUIContent { text = "Size: " + ignoredColliders.arraySize }, true, ignoredColOptions);
		goId.SerializedColDetector.ApplyModifiedProperties();
	}

	private bool NoLabelToggle(bool value, GUILayoutOption[] horizontalOptions)
	{
		float originalLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 0.1f;
		EditorGUILayout.BeginHorizontal(horizontalOptions);
		GUILayout.FlexibleSpace();
		value = EditorGUILayout.Toggle(value);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
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