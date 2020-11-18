
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CollisionUITool : EditorWindow
{
	private struct RowContent
	{
		public GameObject go;
		public bool collisionsEnabled;
		public bool soundsEnabled;
		public bool visualizationEnabled;
		public RowContent(GameObject go)
		{
			this.go = go;
			collisionsEnabled = true;
			soundsEnabled = true;
			visualizationEnabled = true;
		}
	}

	private readonly GUILayoutOption[] toggleOptions = new GUILayoutOption[]
	{
		GUILayout.Width(75)
	};

	private readonly GUILayoutOption[] labelOptions = new GUILayoutOption[]
	{
		GUILayout.Width(200)
	};

	private GameObject[] rootGameObjects;
	private int selectedRootObject = 0;
	private Dictionary<int, RowContent[]> rowContents = new Dictionary<int, RowContent[]>();

	[MenuItem("Mevea/Tools/CollisionUITool")]
	public static void OpenTool()
	{
		CollisionUITool collisionUiTool = (CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool));
		collisionUiTool.Show();
	}

	private void Awake()
	{
		rootGameObjects = GetChildren(GameObject.FindGameObjectWithTag("AssetRoot"));
		for (int i = 0; i < rootGameObjects.Length; ++i)
		{
			int id = rootGameObjects[i].GetInstanceID();
			List<GameObject> gameObjectInRoot = new List<GameObject>(GetChildren(rootGameObjects[i]));
			gameObjectInRoot.RemoveAll(item => item.GetComponent<MeveaObject>() == null);
			rowContents.Add(id, new RowContent[gameObjectInRoot.Count]);
			for (int j = 0; j < gameObjectInRoot.Count; ++j)
			{
				rowContents[id][j] = new RowContent(gameObjectInRoot[j]);
			}
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
		selectedRootObject = EditorGUILayout.Popup(selectedRootObject, GameObjectNames(rootGameObjects),
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
		int rootObjectId = rootGameObjects[selectedRootObject].GetInstanceID();
		for (int i = 0; i < rowContents[rootObjectId].Length; ++i)
		{
			EditorGUILayout.BeginHorizontal();

			ref RowContent contents = ref rowContents[rootObjectId][i];
			EditorGUILayout.LabelField(contents.go.name, labelOptions);
			contents.collisionsEnabled = EditorGUILayout.Toggle(contents.collisionsEnabled, toggleOptions);
			contents.soundsEnabled = EditorGUILayout.Toggle(contents.soundsEnabled, toggleOptions);
			contents.visualizationEnabled = EditorGUILayout.Toggle(contents.visualizationEnabled, toggleOptions);

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();
	}

	private GameObject[] GetChildren(GameObject go)
	{
		List<GameObject> children = new List<GameObject>();
		for (int i = 0; i < go.transform.childCount; ++i)
		{
			children.Add(go.transform.GetChild(i).gameObject);
		}
		return children.ToArray();
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
