using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visuals : CollisionTool
{
    public float delay = .5f;
    public Color color = new Color(1, 0, 0, 0.5f);
    public static VisualStyle visualStyle;

    private void Start()
    {
        CollisionDetector myCd = gameObject.GetComponent<CollisionDetector>();
        myCd.OnCollisionDetected += OnCollisionEvent;
    }
    
    void OnCollisionEvent(object sender, CollisionEventArgs e)
    {
        if (e.MyDetector.enabled && enabled)
        {
            if (e.IsUniqueDetection || (e.OtherDetector.enabled && e.OtherDetector.GetComponent<Visuals>().enabled))
                StartCoroutine(VisulizeCollision(e));
        }
    }

    IEnumerator VisulizeCollision(CollisionEventArgs e)
    {
        VisualSingleton.Instance.Add(e);
        yield return new WaitForSeconds(delay);
        VisualSingleton.Instance.Remove(e);
    }
}
