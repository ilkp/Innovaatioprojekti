using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLog : Singleton<CollisionLog>
{
    private Guard<string, CollisionTool.CollisionEventArgs> collisionEvents;

    private void Awake()
    {
        collisionEvents = new Guard<string, CollisionTool.CollisionEventArgs>();
        GetComponent<CollisionDetector>().OnCollisionDetected += OnCollisionEvent;
    }

    public string GetKey(CollisionTool.CollisionEventArgs e)
    {
        int a = e.MyCollider.GetHashCode();
        int b = e.OtherCollider.GetHashCode();

        string key;
        if (a > b)
            key = a + "." + b;
        else
            key = b + "." + a;

        return key;
    }

    private void OnCollisionEvent(object sender, CollisionTool.CollisionEventArgs e)
    {
        collisionEvents.Add(GetKey(e), e);
    }
}
