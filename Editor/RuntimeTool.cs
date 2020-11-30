using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RuntimeTool : EditorWindow
{
	Vector2 verticalScrollView;

	List<CollisionTool.CollisionEventArgs> collisionEvents;
	List<bool> toggles;

	bool uniquesOnly = false;

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

		uniquesOnly = GUILayout.Toggle(uniquesOnly, "Uniques Only");
		HashSet<string> uniques = new HashSet<string>();

		verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
		{
            for (int i = collisionEvents.Count - 1; i >= 0; i--)
            {
				if (uniquesOnly)
                {
					string key = collisionEvents[i].EntryTime + VisualSingleton.Instance.GetKey(collisionEvents[i]);
					if (uniques.Contains(key)) continue;
					uniques.Add(key);
				}

				EditorGUILayout.BeginHorizontal();

				bool newValue = GUILayout.Toggle(toggles[i], "");

				
				GUILayout.Label(collisionEvents[i].EntryTime.ToString());
				GUILayout.Label(collisionEvents[i].MyName.ToString());
				GUILayout.Label(collisionEvents[i].OtherName.ToString());

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
		if (e.IsUniqueDetection == false // Accept only collisions between two CollisionDetectors
			&& e.MyDetector.enabled && e.OtherDetector.enabled) // both CollisionDetectors are enabled
		{
			e.EntryTime = Time.time; // for some reason this doesn't work when set in CompoundCollisionHack.cs
			collisionEvents.Add(e);
			toggles.Add(false);
			Repaint();
		}
	}
}
