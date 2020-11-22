
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

	private float clickTime = 0f;
	private bool executeFocus = false;
	private bool focusing = false;
	private int selectedRootObject = 0;
	private string[] rootObjectNames;
	private Dictionary<int, Tuple<int, string>[]> toggleObjects = new Dictionary<int, Tuple<int, string>[]>();
	private bool inPlayMode = false;
	private Vector2 horizontalScollView;
	private Vector2 verticalScrollView;

	private bool SPACEHOLDER_collision = true;
	private bool SPACEHOLDER_sound = true;

	[MenuItem("Mevea/Tools/CollisionUITool")]
	public static void OpenTool()
	{
		CollisionUITool collisionUiTool = (CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool));
		collisionUiTool.Show();
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		//Selection.selectionChanged -= FocusOnSelected;
	}

	private void OnGUI()
	{
		if (!Application.isPlaying)
		{
			inPlayMode = false;
			return;
		}
		else if (!inPlayMode)
		{
			CreateToggleContent();
			inPlayMode = true;
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
			createHead();
			createToggleAllButtons();
			GUILayout.Space(SPACE);

			// Create toggle content based on selected root object
			verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
			{
				foreach (Tuple<int, string> toggleObject in toggleObjects[selectedRootObject])
					createToggleRow(toggleObject);
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

	private void createHead()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", componentOptions);
		GUILayout.Space(SPACE);
		EditorGUILayout.LabelField("Collisions", toggleOptions);
		EditorGUILayout.LabelField("Sounds", toggleOptions);
		EditorGUILayout.LabelField("Visualization", toggleOptions);
		EditorGUILayout.EndHorizontal();
	}

	private void createToggleAllButtons()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", componentOptions);
		GUILayout.Space(SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) { }
		if (GUILayout.Button("none", toggleAllOptions)) { }
		GUILayout.Space(TOGGLE_ALL_SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) { }
		if (GUILayout.Button("none", toggleAllOptions)) { }
		GUILayout.Space(TOGGLE_ALL_SPACE);
		if (GUILayout.Button("all", toggleAllOptions)) ToggleAllVisuals(true);
		if (GUILayout.Button("none", toggleAllOptions)) ToggleAllVisuals(false);
		EditorGUILayout.EndHorizontal();
	}

	private void createToggleRow(Tuple<int, string> toggleObject)
	{
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button(toggleObject.Item2, componentOptions))
		{
			Selection.activeGameObject = (GameObject)EditorUtility.InstanceIDToObject(toggleObject.Item1);
			if (!focusing && Time.time - clickTime < DOUBLE_CLICK_TIME)
			{
				executeFocus = true;
			}
			clickTime = Time.time;
		}
		GUILayout.Space(SPACE);
		SPACEHOLDER_sound = EditorGUILayout.Toggle(SPACEHOLDER_sound, toggleOptions);
		SPACEHOLDER_collision = EditorGUILayout.Toggle(SPACEHOLDER_collision, toggleOptions);
		CollisionVisuals.Instance.visualsEnabled[toggleObject.Item1]
			= EditorGUILayout.Toggle(CollisionVisuals.Instance.visualsEnabled[toggleObject.Item1], toggleOptions);
		EditorGUILayout.EndHorizontal();
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

	private string[] GameObjectNames(GameObject[] gos)
	{
		string[] names = new string[gos.Length];
		for (int i = 0; i < gos.Length; ++i)
		{
			names[i] = gos[i].name;
		}
		return names;
	}

	private void CreateToggleContent()
	{
		List<GameObject> rootGameObjects = GetChildren(GameObject.FindGameObjectWithTag("AssetRoot"));
		rootGameObjects.RemoveAll(item => item.GetComponent<CollisionDetector>() == null);
		rootObjectNames = new string[rootGameObjects.Count];

		for (int i = 0; i < rootGameObjects.Count; ++i)
		{
			rootObjectNames[i] = rootGameObjects[i].name;
			List<GameObject> rootChilds = GetChildren(rootGameObjects[i]);
			rootChilds.RemoveAll(item => item.GetComponent<MeveaObject>() == null);
			toggleObjects.Add(i, new Tuple<int, string>[rootChilds.Count]);
			for (int j = 0; j < rootChilds.Count; ++j)
				toggleObjects[i][j] = new Tuple<int, string>(rootChilds[j].GetInstanceID(), rootChilds[j].name);
		}
	}

	private void ToggleAllVisuals(bool value)
	{
		for (int i = 0; i < toggleObjects[selectedRootObject].Length; ++i)
		{
			CollisionVisuals.Instance.visualsEnabled[toggleObjects[selectedRootObject][i].Item1] = value;
		}
	}

	//private void FocusOnSelected()
	//{
	//	Transform[] selected = Selection.transforms;
	//	if (selected.Length == 0)
	//	{
	//		return;
	//	}
	//	FoldoutContent selectedFoldout = FindFromFoldoutTree(selected[0].gameObject.GetInstanceID(), gameObjectFoldoutTree);
	//	if (selectedFoldout != null)
	//	{
	//		GUI.FocusControl(selectedFoldout.go.name);
	//	}
	//}

	//private FoldoutContent FindFromFoldoutTree(int id, FoldoutContent foldoutContent)
	//{
	//	if (foldoutContent.go != null && foldoutContent.go.GetInstanceID() == id)
	//	{
	//		return foldoutContent;
	//	}
	//	foreach (FoldoutContent next in foldoutContent.next)
	//	{
	//		FoldoutContent result = FindFromFoldoutTree(id, next);
	//		if (result != null && result.go.GetInstanceID() == id)
	//		{
	//			return result;
	//		}
	//	}
	//	return null;
	//}
}
