using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum VisualStyle
{
    PerCollider,    // Only the collider hit is visualized
    Compound,       // All colliders connected to the CollisionDetector are visualized
    Mesh,           // All meshfilters connected to the CollisionDetector are visualized
};

public class VisualSingleton : Singleton<VisualSingleton>
{
    public enum VisualMode { Gizmos, Build}
    public VisualMode visualMode;

    [Space]
    public Material highlight;
    
    // hide created visual gameobjects from hierarchy
    public bool hideInHierarchy = true;

    Color uniqueColor = new Color(0, 0, 1, 0.5f);

    class Visual
    {
        public CollisionTool.CollisionEventArgs collisionEvent;
        public List<GameObject> gameObjects;

        public Visual(CollisionTool.CollisionEventArgs e)
        {
            collisionEvent = e;
            gameObjects = new List<GameObject>();
        }
    }

    private DictionaryGuard<string, Visual> visuals;
    private Mesh primitiveSphere, primitiveCube;

    protected VisualSingleton() { }

    private void Awake()
    {
        visuals = new DictionaryGuard<string, Visual>();
        primitiveSphere = GetPrimitiveMesh(PrimitiveType.Sphere);
        primitiveCube = GetPrimitiveMesh(PrimitiveType.Cube);
    }

    public void Add(CollisionTool.CollisionEventArgs e, string tag = "")
    {
        string key = tag + GetKey(e);
        if (visuals.Add(key, new Visual(e)) && visualMode == VisualMode.Build)
        {
            DrawVisuals(e.MyDetector, e.MyCollider, ref visuals[key].gameObjects);

            if (e.IsUniqueDetection)
                visuals[key].gameObjects.Add(CreateVisual(GetColliderMeshInfo(e.OtherCollider), e.OtherCollider.transform, uniqueColor));
            else
                DrawVisuals(e.OtherDetector, e.OtherCollider, ref visuals[key].gameObjects);
        }
    }

    public void Remove(CollisionTool.CollisionEventArgs e, string tag = "")
    {
        Visual visual = visuals.Remove(tag + GetKey(e));
        if (visual != null)
        {
            for (int i = 0; i < visual.gameObjects.Count; i++)
            {
                Destroy(visual.gameObjects[i]);
            }
        }
    }

    public string GetKey(CollisionTool.CollisionEventArgs e)
    {
        int a = e.MyCollider.GetInstanceID();
        int b = e.OtherCollider.GetInstanceID();

        string key;
        if (a > b)
            key = a + "." + b;
        else
            key = b + "." + a;

        return key;
    }

    // Get any of Unitys primitive meshes
    Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;
        Destroy(go);
        return mesh;
    }

    GameObject CreateVisual(MeshInfo meshInfo, Transform parent, Color color)
    {
        GameObject go = new GameObject("Visual");

        if (hideInHierarchy)        
            go.hideFlags = HideFlags.HideInHierarchy;

        go.transform.parent = parent;
        go.transform.localPosition = meshInfo.position;
        go.transform.localScale = meshInfo.scale;
        go.transform.localRotation = Quaternion.identity;

        go.AddComponent<MeshFilter>().mesh = meshInfo.mesh;
        go.AddComponent<MeshRenderer>().material = new Material(highlight) { color = color };

        return go;
    }

    struct MeshInfo
    {
        public Mesh mesh;
        public Vector3 position;
        public Vector3 scale;

        public MeshInfo(Mesh mesh, Vector3 position, Vector3 scale)
        {
            this.mesh = mesh;
            this.position = position;
            this.scale = scale;
        }
    }

    MeshInfo GetColliderMeshInfo(Collider col)
    {
        MeshInfo meshInfo = new MeshInfo();

        // Draw collider based on its type
        switch (col)
        {
            case SphereCollider c:
                meshInfo.mesh = primitiveSphere;
                meshInfo.position = c.center;
                meshInfo.scale = c.radius * 2 * Vector3.one;
                break;
            case BoxCollider c:
                meshInfo.mesh = primitiveCube;
                meshInfo.position = c.center;
                meshInfo.scale = c.size; // * 1.01f;
                break;
            case MeshCollider c:
                meshInfo.mesh = c.sharedMesh;
                meshInfo.position = Vector3.zero;
                meshInfo.scale = Vector3.one;
                break;
            default:
                throw new Exception("Visuals for \"" + col.GetType().ToString() + "\" have not been implemented.");
        }

        return meshInfo;
    }

    void DrawVisuals(CollisionDetector detector, Collider col, ref List<GameObject> gameObjects)
    {
        Color color = detector.GetComponent<Visuals>().color;

        switch (Visuals.visualStyle)
        {
            case VisualStyle.PerCollider:
                gameObjects.Add(CreateVisual(GetColliderMeshInfo(col), col.transform, color));
                break;

            case VisualStyle.Compound:
                foreach (Collider c in detector.GetComponentsInChildren<Collider>())
                {
                    gameObjects.Add(CreateVisual(GetColliderMeshInfo(c), c.transform, color));
                }
                break;

            case VisualStyle.Mesh:
                foreach (MeshFilter meshFilter in detector.GetComponentsInChildren<MeshFilter>())
                {
                    if (meshFilter.sharedMesh.normals.Length > 0)
                    {
                        Mesh mesh = meshFilter.mesh;
                        mesh.SetTriangles(mesh.triangles, 0);
                        mesh.subMeshCount = 1;
                        gameObjects.Add(CreateVisual(new MeshInfo(mesh, Vector3.zero, Vector3.one), meshFilter.transform, color));
                    }
                }
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (visualMode != VisualMode.Gizmos) return;

        // if application is not playing no collisions can happen
        if (Application.isPlaying == false) return;

        void DrawCollider(Collider col)
        {
            Gizmos.matrix = col.transform.localToWorldMatrix;

            MeshInfo meshInfo = GetColliderMeshInfo(col);
            Gizmos.DrawMesh(meshInfo.mesh, meshInfo.position, Quaternion.identity, meshInfo.scale);
        }

        void DrawVisualsGizmos(CollisionDetector detector, Collider col)
        {
            Gizmos.color = detector.GetComponent<Visuals>().color;

            switch (Visuals.visualStyle)
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
                            Gizmos.matrix = meshFilter.transform.localToWorldMatrix;

                            Mesh mesh = meshFilter.mesh;
                            mesh.SetTriangles(mesh.triangles, 0);
                            mesh.subMeshCount = 1;
                            Gizmos.DrawMesh(mesh);
                        }
                    }
                    break;
            }
        }

        foreach (var e in visuals.GetValues())
        {
            DrawVisualsGizmos(e.collisionEvent.MyDetector, e.collisionEvent.MyCollider);

            if (e.collisionEvent.IsUniqueDetection)
                DrawCollider(e.collisionEvent.OtherCollider);
            else
                DrawVisualsGizmos(e.collisionEvent.OtherDetector, e.collisionEvent.OtherCollider);
        }
    }
}
