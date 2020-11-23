using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visuals : CollisionTool
{
    public float delay = .5f;
    public Color color = new Color(1, 0, 0, 0.5f);
    readonly List<Collider> colliders = new List<Collider>();

    private void Start()
    {
        CollisionDetector myCd = gameObject.GetComponent<CollisionDetector>();
        myCd.OnCollisionDetected += OnCollisionEvent;
    }
    
    void OnCollisionEvent(object sender, CollisionEventArgs e)
    {
        if (enabled && (e.IsUniqueDetection || e.OtherCollider.GetComponent<Visuals>().enabled))
        {
            StartCoroutine(VisulizeCollision(e.MyCollider));
            if (e.IsUniqueDetection)
                StartCoroutine(VisulizeCollision(e.OtherCollider));
        }
    }
    
    IEnumerator VisulizeCollision(Collider col)
    {
        colliders.Add(col);
        yield return new WaitForSeconds(delay);
        colliders.Remove(col);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = color;

        foreach (Collider col in colliders)
        {
            Gizmos.matrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, col.transform.lossyScale);

            switch (col)
            {
                case SphereCollider c:
                    Gizmos.DrawSphere(c.center, c.radius);
                    break;
                case BoxCollider c:
                    Gizmos.DrawCube(c.center, c.size);
                    break;
                case MeshCollider c:
                    Gizmos.DrawMesh(c.sharedMesh);
                    break;
                default:
                    Debug.LogWarning("Visuals for \"" + col.GetType().ToString() + "\" have not been implemented yet.");
                    break;
            }
        }
    }
}
