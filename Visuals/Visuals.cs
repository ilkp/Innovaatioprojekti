using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visuals : CollisionTool
{
    public float delay = .5f;
    public Color color = new Color(1, 0, 0, 0.5f);

    public enum VisualStyle { PerCollider, Compound, Mesh };
    public VisualStyle visualStyle;

    readonly List<CollisionEventArgs> collisionEvents = new List<CollisionEventArgs>();

    private void Start()
    {
        CollisionDetector myCd = gameObject.GetComponent<CollisionDetector>();
        myCd.OnCollisionDetected += OnCollisionEvent;
    }
    
    void OnCollisionEvent(object sender, CollisionEventArgs e)
    {
        if (enabled && (e.IsUniqueDetection || e.OtherCollider.GetComponentInParent<Visuals>().enabled))
        {
            StartCoroutine(VisulizeCollision(e));
        }
    }

    IEnumerator VisulizeCollision(CollisionEventArgs e)
    {
        collisionEvents.Add(e);
        yield return new WaitForSeconds(delay);
        collisionEvents.Remove(e);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;

        void DrawCollider(Collider col)
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
                    Debug.LogWarning("Visuals for \"" + col.GetType().ToString() + "\" have not been implemented.");
                    break;
            }
        }

        Gizmos.color = color;

        foreach (CollisionEventArgs e in collisionEvents)
        {
            switch (visualStyle)
            {
                case VisualStyle.PerCollider:
                    DrawCollider(e.MyCollider);
                    break;

                case VisualStyle.Compound:
                    foreach (Collider col in e.MyDetector.GetComponentsInChildren<Collider>())
                        DrawCollider(col);
                    break;

                case VisualStyle.Mesh:
                    foreach (MeshFilter meshFilter in e.MyDetector.GetComponentsInChildren<MeshFilter>())
                    {
                        if (meshFilter.sharedMesh.normals.Length > 0)
                        {
                            Gizmos.matrix = Matrix4x4.TRS(meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
                            Gizmos.DrawMesh(meshFilter.sharedMesh);
                        }
                    }
                    break;
            }

            // if other doesn't have Collision Detector just draw the other collider
            if (e.IsUniqueDetection) DrawCollider(e.OtherCollider);
        }
        
    }
}
