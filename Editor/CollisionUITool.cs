
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CollisionUITool : EditorWindow
{
	private class FoldoutContent
	{
		public GameObject go = null;
		public bool isOpen = false;
		public bool collisionEnabled = true;
		public bool soundEnabled = true;
		public bool visualizationEnabled = true;
		public List<FoldoutContent> next = new List<FoldoutContent>();
	}

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

	private bool contentIsDirty = true;
	private GameObject[] rootGameObjects;
	private int selectedRootObject = 0;
	private FoldoutContent gameObjectFoldoutTree = new FoldoutContent();
	private Dictionary<int, RowContent[]> rowContents = new Dictionary<int, RowContent[]>();
	//private RowContent[] rowContents;

	[MenuItem("Mevea/Tools/CollisionUITool")]
	public static void OpenTool()
	{
		CollisionUITool collisionUiTool = (CollisionUITool)EditorWindow.GetWindow(typeof(CollisionUITool));
		collisionUiTool.Show();
	}

	private void Awake()
	{
		//Selection.selectionChanged += FocusOnSelected;
	}

	private void OnDestroy()
	{
		//Selection.selectionChanged -= FocusOnSelected;
	}

	private void OnGUI()
	{
		rootGameObjects = GetChildren(GameObject.FindGameObjectWithTag("AssetRoot"));

		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField("Root",
			new GUILayoutOption[]
			{
				GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Root")).x)
			});

		// Create a popup selector for the root object
		EditorGUI.BeginChangeCheck();
		selectedRootObject = EditorGUILayout.Popup(selectedRootObject, GameObjectNames(rootGameObjects),
			new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false),
				GUILayout.MinWidth(80)
			});
		if (EditorGUI.EndChangeCheck())
		{
			contentIsDirty = true;
		}

		// If the root object changes, recreate the the foldout data
		if (contentIsDirty)
		{
			int id = rootGameObjects[selectedRootObject].GetInstanceID();
			if (!rowContents.ContainsKey(id))
			{
				List<GameObject> objectsInSelected = new List<GameObject>(GetChildren(rootGameObjects[selectedRootObject]));
				objectsInSelected.RemoveAll(item => item.GetComponent<MeveaObject>() == null);
				rowContents.Add(id, new RowContent[objectsInSelected.Count]);
				for (int i = 0; i < objectsInSelected.Count; ++i)
				{
					rowContents[id][i] = new RowContent(objectsInSelected[i]);
				}
			}

			//gameObjectFoldoutTree.next = new List<FoldoutContent>();
			//foreach (GameObject child in objectsInSelected)
			//{
			//	CreateFoldoutContentTree(gameObjectFoldoutTree, child);
			//}
			contentIsDirty = false;
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", labelOptions);
		EditorGUILayout.LabelField("Collisions", toggleOptions);
		EditorGUILayout.LabelField("Sounds", toggleOptions);
		EditorGUILayout.LabelField("Visualization", toggleOptions);
		EditorGUILayout.EndHorizontal();

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

		// Update game object foldout contents
		//foreach(FoldoutContent foldoutContent in gameObjectFoldoutTree.next)
		//{
		//	CreateFoldout(foldoutContent, 0);
		//}

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

	private void CreateFoldoutContentTree(FoldoutContent parent, GameObject go)
	{
		FoldoutContent content = new FoldoutContent();
		content.go = go;
		content.next = new List<FoldoutContent>();
		parent.next.Add(content);
		if (go.GetComponent<MeveaObject>() == null)
		{
			foreach (GameObject child in GetChildren(go))
			{
				CreateFoldoutContentTree(content, child);
			}
		}
	}

	private void CreateFoldout(FoldoutContent foldoutContent, int depth)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUI.indentLevel = depth;
		GUI.SetNextControlName(foldoutContent.go.name);
		foldoutContent.isOpen = EditorGUILayout.Foldout(foldoutContent.isOpen, foldoutContent.go.name, true);
		EditorGUI.indentLevel = 0;
		foldoutContent.collisionEnabled = EditorGUILayout.Toggle(foldoutContent.collisionEnabled, toggleOptions);
		foldoutContent.soundEnabled = EditorGUILayout.Toggle(foldoutContent.soundEnabled, toggleOptions);
		foldoutContent.visualizationEnabled = EditorGUILayout.Toggle(foldoutContent.visualizationEnabled, toggleOptions);
		EditorGUILayout.EndHorizontal();
		if (foldoutContent.isOpen)
		{
			foreach (FoldoutContent next in foldoutContent.next)
			{
				CreateFoldout(next, depth + 1);
			}
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
