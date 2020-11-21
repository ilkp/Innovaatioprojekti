using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollisionVisuals : CollisionTool
{
    public Color color = Color.red;
    public float delay = 0.5f;

    private void Start() 
    {
        CollisionDetector myCd = gameObject.GetComponent<CollisionDetector>();
        myCd.OnCollisionDetected += OnCollisionEvent;
    }

    void OnCollisionEvent(object sender, CollisionEventArgs e)
    {
        //Debug.Log(sender + ": " + e.MyName + " & " + e.OtherName);
        int idFirst = e.MyCollider.gameObject.GetInstanceID();
        int idSecond = e.OtherCollider.gameObject.GetInstanceID();
        if (!CollisionVisuals.Instance.visualsEnabled.ContainsKey(idFirst) || !CollisionVisuals.Instance.visualsEnabled[idFirst])
            return;
        if (!CollisionVisuals.Instance.visualsEnabled.ContainsKey(idSecond) || !CollisionVisuals.Instance.visualsEnabled[idSecond])
            return;
        StartCoroutine(VisulizeCollision(e));
    }

    
    IEnumerator VisulizeCollision(CollisionEventArgs e)
    {
        CollisionVisuals.Instance.AddVisual(e.MyCollider, color);
        CollisionVisuals.Instance.AddVisual(e.OtherCollider, color);
        yield return new WaitForSeconds(delay);
        CollisionVisuals.Instance.RemoveVisual(e.MyCollider, color);
        CollisionVisuals.Instance.RemoveVisual(e.OtherCollider, color);
    }
}
