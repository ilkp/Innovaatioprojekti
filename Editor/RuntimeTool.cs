using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RuntimeTool : EditorWindow
{
	Vector2 verticalScrollView;

	List<CollisionTool.CollisionEventArgs> collisionEvents;
	List<bool> toggles;

	bool hideDuplicates = false;
	bool hideUniques = false;

	[MenuItem("Mevea/Tools/RuntimeTool")]
	public static void Init()
	{
		GetWindow(typeof(RuntimeTool)).Show();
	}

    private void OnGUI()
    {
		if (Application.isPlaying == false)
        {
			return;
        }

		if (collisionEvents == null)
        {
			collisionEvents = new List<CollisionTool.CollisionEventArgs>();
			toggles = new List<bool>();
			SubscribeToCollisionDetectors();
		}

		hideDuplicates = GUILayout.Toggle(hideDuplicates, "Hide Duplicates");
		hideUniques = GUILayout.Toggle(hideUniques, "Hide Uniques");
		GUILayout.Space(15);

		HashSet<string> nonDuplicates = new HashSet<string>();

		verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
		{
            for (int i = collisionEvents.Count - 1; i >= 0; i--)
            {
				if (hideUniques && collisionEvents[i].IsUniqueDetection) continue;

				if (hideDuplicates)
                {
					string key = collisionEvents[i].EntryTime + VisualSingleton.Instance.GetKey(collisionEvents[i]);
					if (nonDuplicates.Contains(key)) continue;
					nonDuplicates.Add(key);
				}

				EditorGUILayout.BeginHorizontal();

				bool newValue = GUILayout.Toggle(toggles[i], "");
				
				GUILayout.Label(collisionEvents[i].EntryTime.ToString());
				GUILayout.Label(collisionEvents[i].MyName.ToString());

				var style = new GUIStyle(GUI.skin.label);
				if (collisionEvents[i].IsUniqueDetection)
					style.normal.textColor = Color.blue;

				GUILayout.Label(collisionEvents[i].OtherName.ToString(), style);

				GUILayout.FlexibleSpace();

				if (toggles[i] != newValue)
				{
					toggles[i] = newValue;

					if (newValue)
						VisualSingleton.Instance.Add(collisionEvents[i], "RT");
					else
						VisualSingleton.Instance.Remove(collisionEvents[i], "RT");
				}

				EditorGUILayout.EndHorizontal();
			}
		}
		EditorGUILayout.EndScrollView();
	}

	void SubscribeToCollisionDetectors()
    {
        foreach (var detector in FindObjectsOfType<CollisionDetector>())
        {
			detector.OnCollisionDetected += OnCollisionDetected;
        } 
    }

	void OnCollisionDetected(object sender, CollisionTool.CollisionEventArgs e)
    {
		if (e.MyDetector.enabled && (e.IsUniqueDetection || e.OtherDetector.enabled))
		{
			e.EntryTime = Time.time; // for some reason this doesn't work when set in CompoundCollisionHack.cs
			collisionEvents.Add(e);
			toggles.Add(false);
			Repaint();
		}
	}
}
