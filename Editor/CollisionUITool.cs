
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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

	private Vector2 horizontalScollView;
	private Vector2 verticalScrollView;

	private double clickTime = 0f;
	private bool executeFocus = false;
	private bool focusing = false;
	private int selectedRootObject = 0;
	private string[] rootObjectNames;
	private Dictionary<int, int[]> objectIds;
	private bool dummyToggle = false;

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

			// Create toggle header and toggle all/none buttons
			GUILayout.Space(SPACE);
			CreateHead();
			CreateToggleAllButtons();
			GUILayout.Space(SPACE);

			// Create toggles based on selected root object
			verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
			{
				for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
					CreateRow(objectIds[selectedRootObject][i], i);
			}
			EditorGUILayout.EndScrollView();
		}
		EditorGUILayout.EndScrollView();

		// Execute focus on game object if double clicked
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
		rootGameObjects.RemoveAll(item => !item.activeInHierarchy);
		rootObjectNames = new string[rootGameObjects.Count];
		objectIds = new Dictionary<int, int[]>();

		for (int i = 0; i < rootGameObjects.Count; ++i)
		{
			rootObjectNames[i] = rootGameObjects[i].name;
			List<GameObject> childs = GetChildren(rootGameObjects[i]);
			childs.RemoveAll(item => item.GetComponent<MeveaObject>() == null || !item.activeInHierarchy);
			objectIds.Add(i, new int[childs.Count]);
			for (int j = 0; j < childs.Count; ++j)
			{
				objectIds[i][j] = childs[j].GetInstanceID();
			}
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
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAll<CollisionDetector>(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAll<CollisionDetector>(false);

		GUILayout.Space(TOGGLE_ALL_SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAll<CollisionSoundManager>(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAll<CollisionSoundManager>(false);

		GUILayout.Space(TOGGLE_ALL_SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAll<Visuals>(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAll<Visuals>(false);

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

		CreateToggle<CollisionDetector>(go);
		CreateToggle<CollisionSoundManager>(go);
		CreateToggle<Visuals>(go);

		EditorGUILayout.EndHorizontal();
	}

	private void CreateToggle<T>(GameObject go) where T : Behaviour
	{
		if (go.GetComponent<T>() != null)
		{
			go.GetComponent<T>().enabled = EditorGUILayout.Toggle(go.GetComponent<T>().enabled, toggleOptions);
		}
		else
		{
			GUI.enabled = false;
			dummyToggle = EditorGUILayout.Toggle(dummyToggle, toggleOptions);
			GUI.enabled = true;
		}
	}

	private void ToggleAll<T>(bool value) where T : Behaviour
	{
		GameObject go;
		for (int i = 0; i < objectIds[selectedRootObject].Length; ++i)
		{
			go = (GameObject)EditorUtility.InstanceIDToObject(objectIds[selectedRootObject][i]);
			if (go.GetComponent<T>() != null)
				go.GetComponent<T>().enabled = value;
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
			((CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool))).CreateToggleContent();
	}
}