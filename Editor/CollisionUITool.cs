
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class CollisionUITool : EditorWindow
{
	private const int SPACE = 20;
	private const int COMPONENT_WIDTH = 200;
	private const int TOGGLE_WIDTH = 90;
	private const int TOGGLE_ALL_WIDTH = 40;
	private const int TOGGLE_ALL_SPACE = TOGGLE_WIDTH - 2 * TOGGLE_ALL_WIDTH - 4;
	private const float DOUBLE_CLICK_TIME = 0.2f;

	private readonly GUILayoutOption[] toggleOptions = new GUILayoutOption[]
	{
		GUILayout.Width(TOGGLE_WIDTH)
	};

	private readonly GUILayoutOption[] componentOptions = new GUILayoutOption[]
	{
		GUILayout.Width(COMPONENT_WIDTH)
	};

	private readonly GUILayoutOption[] toggleAllOptions = new GUILayoutOption[]
	{
		GUILayout.Width(TOGGLE_ALL_WIDTH)
	};

	private double clickTime = 0f;
	private bool executeFocus = false;
	private bool focusing = false;
	private Vector2 horizontalScollView;
	private Vector2 verticalScrollView;
	public int selectedRootObject = 0;
	public string[] rootObjectNames;
	public Dictionary<int, int[]> objectIds;
	public bool dummyToggle = false;

	[MenuItem("Mevea/Tools/CollisionUITool")]
	public static void OpenTool()
	{
		CollisionUITool collisionUiTool = (CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool));
		collisionUiTool.Show();
	}

	private void OnEnable()
	{
		EditorApplication.playModeStateChanged += OnPlayModeChanged;
	}

	private void OnDisable()
	{
		EditorApplication.playModeStateChanged -= OnPlayModeChanged;
	}

	private void Awake()
	{
		CreateToggleContent();
	}

	private void OnGUI()
	{
		if (objectIds == null)
		{
			return;
		}

		horizontalScollView = EditorGUILayout.BeginScrollView(horizontalScollView, GUI.skin.horizontalScrollbar, GUIStyle.none);
		{
			// Create label and popup selector for root object
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Root",
					new GUILayoutOption[]
					{
						GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Root")).x)
					});

				selectedRootObject = EditorGUILayout.Popup(selectedRootObject, rootObjectNames,
					new GUILayoutOption[]
					{
						GUILayout.ExpandWidth(false),
						GUILayout.MinWidth(80)
					});
			}
			EditorGUILayout.EndHorizontal();

			// Create toggle header and buttons
			GUILayout.Space(SPACE);
			CreateHead();
			CreateToggleAllButtons();
			GUILayout.Space(SPACE);
			// Create toggle content based on selected root object
			verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
			{
				for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
					CreateRow(objectIds[selectedRootObject][i], i);
			}
			EditorGUILayout.EndScrollView();
		}
		EditorGUILayout.EndScrollView();

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
		List<GameObject> rootGameObjects = GetChildren(GameObject.FindGameObjectWithTag("AssetRoot"));
		rootObjectNames = new string[rootGameObjects.Count];
		objectIds = new Dictionary<int, int[]>();

		for (int i = 0; i < rootGameObjects.Count; ++i)
		{
			rootObjectNames[i] = rootGameObjects[i].name;
			List<GameObject> childs = GetChildren(rootGameObjects[i]);
			childs.RemoveAll(item => item.GetComponent<MeveaObject>() == null);
			objectIds.Add(i, new int[childs.Count]);
			for (int j = 0; j < childs.Count; ++j)
				objectIds[i][j] = childs[j].GetInstanceID();
		}
	}

	private void CreateHead()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", componentOptions);
		GUILayout.Space(SPACE);
		EditorGUILayout.LabelField("Collisions", toggleOptions);
		EditorGUILayout.LabelField("Sounds", toggleOptions);
		EditorGUILayout.LabelField("Visualization", toggleOptions);
		EditorGUILayout.EndHorizontal();
	}

	private void CreateToggleAllButtons()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", componentOptions);

		GUILayout.Space(SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) { ToggleAllColliders(true); }
		if (GUILayout.Button("none", toggleAllOptions)) { ToggleAllColliders(false); }

		GUILayout.Space(TOGGLE_ALL_SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) { }
		if (GUILayout.Button("none", toggleAllOptions)) { }

		GUILayout.Space(TOGGLE_ALL_SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAllVisuals(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAllVisuals(false);

		EditorGUILayout.EndHorizontal();
	}

	private void CreateRow(int goId, int rowIndex)
	{
		GameObject go = (GameObject)EditorUtility.InstanceIDToObject(goId);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button(go.name, componentOptions))
		{
			Selection.activeGameObject = go;
			if (!focusing && EditorApplication.timeSinceStartup - clickTime < DOUBLE_CLICK_TIME)
			{
				executeFocus = true;
			}
			clickTime = EditorApplication.timeSinceStartup;
		}
		GUILayout.Space(SPACE);

		go.GetComponent<CollisionDetector>().enabled = EditorGUILayout.Toggle(go.GetComponent<CollisionDetector>().enabled, toggleOptions);
		dummyToggle = EditorGUILayout.Toggle(dummyToggle, toggleOptions);
		go.GetComponent<Visuals>().enabled = EditorGUILayout.Toggle(go.GetComponent<Visuals>().enabled, toggleOptions);

		EditorGUILayout.EndHorizontal();
	}

	private void ToggleAllColliders(bool value)
	{
		for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
		{
			((GameObject)EditorUtility.InstanceIDToObject(objectIds[selectedRootObject][i]))
				.GetComponent<CollisionDetector>().enabled = value;
		}
	}

	private void ToggleAllVisuals(bool value)
	{
		for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
		{
			((GameObject)EditorUtility.InstanceIDToObject(objectIds[selectedRootObject][i]))
				.GetComponent<Visuals>().enabled = value;
		}
	}

	private List<GameObject> GetChildren(GameObject go)
	{
		List<GameObject> children = new List<GameObject>();
		for (int i = 0; i < go.transform.childCount; ++i)
		{
			children.Add(go.transform.GetChild(i).gameObject);
		}
		return children;
	}

	private static void OnPlayModeChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredPlayMode)
		{
			((CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool))).CreateToggleContent();
		}
	}
}