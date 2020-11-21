
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class CollisionUITool : EditorWindow
{
	private readonly GUILayoutOption[] toggleOptions = new GUILayoutOption[]
	{
		GUILayout.Width(75)
	};

	private readonly GUILayoutOption[] labelOptions = new GUILayoutOption[]
	{
		GUILayout.Width(200)
	};

	private int selectedRootObject = 0;
	private string[] rootObjectNames;
	private Dictionary<int, Tuple<int, string>[]> toggleObjects = new Dictionary<int, Tuple<int, string>[]>();

	[MenuItem("Mevea/Tools/CollisionUITool")]
	public static void OpenTool()
	{
		CollisionUITool collisionUiTool = (CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool));
		collisionUiTool.Show();
	}

	private void Awake()
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

	private void OnDestroy()
	{
		//Selection.selectionChanged -= FocusOnSelected;
	}

	private void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Root",
			new GUILayoutOption[]
			{
				GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Root")).x)
			});

		// Create a popup selector for the root object
		selectedRootObject = EditorGUILayout.Popup(selectedRootObject, rootObjectNames,
			new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false),
				GUILayout.MinWidth(80)
			});
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", labelOptions);
		EditorGUILayout.LabelField("Collisions", toggleOptions);
		EditorGUILayout.LabelField("Sounds", toggleOptions);
		EditorGUILayout.LabelField("Visualization", toggleOptions);
		EditorGUILayout.EndHorizontal();

		// Create toggle content based on selected root object
		foreach (Tuple<int, string> toggleObject in toggleObjects[selectedRootObject])
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(toggleObject.Item2, labelOptions);
			CollisionVisuals.Instance.visualsEnabled[toggleObject.Item1]
				= EditorGUILayout.Toggle(CollisionVisuals.Instance.visualsEnabled[toggleObject.Item1], toggleOptions);
			//contents.soundsEnabled = EditorGUILayout.Toggle(contents.soundsEnabled, toggleOptions);
			//contents.visualizationEnabled = EditorGUILayout.Toggle(contents.visualizationEnabled, toggleOptions);

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();
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
