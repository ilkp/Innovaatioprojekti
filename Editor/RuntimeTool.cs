using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RuntimeTool : EditorWindow
{
	Vector2 verticalScrollView;

	List<CollisionTool.CollisionEventArgs> collisionEvents;
	List<bool> toggles;

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

		verticalScrollView = EditorGUILayout.BeginScrollView(verticalScrollView, GUIStyle.none, GUI.skin.verticalScrollbar);
		{
            for (int i = 0; i < collisionEvents.Count; i++)
            {
				EditorGUILayout.BeginHorizontal();

				GUILayout.Label(collisionEvents[i].EntryTime.ToString());
				GUILayout.Label(collisionEvents[i].MyName.ToString());
				GUILayout.Label(collisionEvents[i].OtherName.ToString());

				toggles[i] = GUILayout.Toggle(toggles[i], "");

				//if (toggles[i])
				//	VisualSingleton.Instance.AddCollisionEvent(collisionEvents[i]);
				//else
				//	VisualSingleton.Instance.RemoveCollisionEvent(collisionEvents[i]);

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
			collisionEvents.Add(e);
			toggles.Add(false);
			Repaint();
		}
	}
}
