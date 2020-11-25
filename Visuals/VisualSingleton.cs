using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VisualStyle
{
    PerCollider,    // Only the collider hit is visualized
    Compound,       // All colliders connected to the CollisionDetector are visualized
    Mesh,           // All meshfilters/meshes connected to the CollisionDetector are visualized
};

public class VisualSingleton : Singleton<VisualSingleton>
{
    private Guard<CollisionTool.CollisionEventArgs> collisionEvents;
    private Mesh primitiveSphere;

    protected VisualSingleton() { }

    private void Awake()
    {
        collisionEvents = new Guard<CollisionTool.CollisionEventArgs>();
        primitiveSphere = GetPrimitiveMesh(PrimitiveType.Sphere);
    }

    public void AddCollisionEvent(CollisionTool.CollisionEventArgs e)
    {
        collisionEvents.Add(e);
    }

    public void RemoveCollisionEvent(CollisionTool.CollisionEventArgs e)
    {
        collisionEvents.Remove(e);
    }

    // Get any of Unitys primitive meshes
    Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
        Destroy(go);
        return mesh;
    }

    private void OnDrawGizmos()
    {
        // if application is not playing no collisions can happen
        if (Application.isPlaying == false) return;

        void DrawCollider(Collider col)
        {
            // Match gizmos transform with the given collider
            Gizmos.matrix = Matrix4x4.TRS(col.transform.position, col.transform.rotation, col.transform.lossyScale);

            // Draw collider based on its type
            switch (col)
            {
                case SphereCollider c:
                    //Gizmos.DrawSphere(c.center, c.radius); // produces a low quality mesh
                    Gizmos.DrawMesh(primitiveSphere, c.center, Quaternion.identity, c.radius * 2 * Vector3.one);
                    break;
                case BoxCollider c:
                    Gizmos.DrawCube(c.center, c.size * 1.01f); // make the size slightly bigger so it doesn't clip inside the model
                    break;
                case MeshCollider c:
                    Gizmos.DrawMesh(c.sharedMesh);
                    break;
                default:
                    Debug.LogWarning("Visuals for \"" + col.GetType().ToString() + "\" have not been implemented.");
                    break;
            }
        }

        void DrawVisuals(CollisionDetector detector, Collider col)
        {
            Visuals visuals = detector.GetComponent<Visuals>();
            Gizmos.color = visuals.color;

            switch (visuals.visualStyle)
            {
                case VisualStyle.PerCollider:
                    DrawCollider(col);
                    break;

                case VisualStyle.Compound:
                    foreach (Collider c in detector.GetComponentsInChildren<Collider>())
                        DrawCollider(c);
                    break;

                case VisualStyle.Mesh:
                    foreach (MeshFilter meshFilter in detector.GetComponentsInChildren<MeshFilter>())
                    {
                        if (meshFilter.sharedMesh.normals.Length > 0)
                        {
                            Gizmos.matrix = Matrix4x4.TRS(meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
                            Gizmos.DrawMesh(meshFilter.sharedMesh);
                        }
                    }
                    break;
            }
        }

        foreach (var e in collisionEvents.GetValues())
        {
            DrawVisuals(e.MyDetector, e.MyCollider);
            DrawVisuals(e.OtherDetector, e.OtherCollider);
        }
    }
}
