﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visuals : CollisionTool
{
    public float delay = .5f;
    public Color color = new Color(1, 0, 0, 0.5f);

    public enum VisualStyle { 
        PerCollider,    // Only the collider hit is visualized
        Compound,       // All colliders connected to the CollisionDetector are visualized
        Mesh,           // All meshfilters/meshes connected to the CollisionDetector are visualized
    };
    public VisualStyle visualStyle;

    private readonly List<CollisionEventArgs> collisionEvents = new List<CollisionEventArgs>();

    private Mesh primitiveSphere;

    private void Start()
    {
        CollisionDetector myCd = gameObject.GetComponent<CollisionDetector>();
        myCd.OnCollisionDetected += OnCollisionEvent;

        primitiveSphere = GetPrimitiveMesh(PrimitiveType.Sphere);
    }
    
    void OnCollisionEvent(object sender, CollisionEventArgs e)
    {
        if (e.IsUniqueDetection == false // Accept only collisions between two CollisionDetectors
            && e.MyDetector.enabled && e.OtherDetector.enabled // both CollisionDetectors are enabled
            && enabled && e.OtherCollider.GetComponentInParent<Visuals>().enabled) // both Visuals are enabled
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

        // Set gizmos color
        Gizmos.color = color;

        // Draw my side of all collision events
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
        }
        
    }
}
