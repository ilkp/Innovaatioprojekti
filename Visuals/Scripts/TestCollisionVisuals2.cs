using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollisionVisuals2 : CollisionTool
{
    public Color color = Color.red;

    Collider myCollider;

    private void Start()
    {
        CollisionDetector myCd = gameObject.GetComponent<CollisionDetector>();
        myCd.OnCollisionDetected += CollisionEnter;
        //myCd.OnCollisionExit += CollisionExit;

        myCollider = GetComponent<Collider>();
    }

    void CollisionEnter(object sender, CollisionEventArgs e)
    {
        //Debug.Log(sender + ": " + e.MyName + " & " + e.OtherName);

        CollisionVisuals.Instance.AddVisual(e.MyCollider, color);
        CollisionVisuals.Instance.AddVisual(e.OtherCollider, color);
    }

    private void OnTriggerExit(Collider other)
    {
        CollisionVisuals.Instance.RemoveVisual(myCollider, color);
        CollisionVisuals.Instance.RemoveVisual(other, color);
    }
}
