using System.Collections;
using System.Collections.Generic;
using Unity.UNetWeaver;
using UnityEngine;

public class CollisionVisuals : Singleton<CollisionVisuals>
{
    public Material highlightMaterial;
    public Dictionary<int, bool> visualsEnabled;

	private void Start()
	{
        visualsEnabled = new Dictionary<int, bool>();

        GameObject assetRoot = GameObject.FindGameObjectWithTag("AssetRoot");
        for (int i = 0; i < assetRoot.transform.childCount; ++i)
        {
            Transform asset = assetRoot.transform.GetChild(i);
            if (asset.GetComponent<CollisionDetector>() == null)
                continue;
            for (int j = 0; j < asset.childCount; ++j)
                if (asset.GetChild(j).GetComponent<MeveaObject>() != null)
                    visualsEnabled.Add(asset.GetChild(j).gameObject.GetInstanceID(), true);
        }
    }

	struct Visual
    {
        public int count;
        public MeshRenderer renderer;
        public Material originalMaterial;
        public List<Color> colors;

        public Visual(Collider collider, Color color) 
        {
            renderer = collider.GetComponentInChildren<MeshRenderer>();
            originalMaterial = renderer.material;
            count = 0;
            colors = new List<Color>{ color };
        }

        public void SetMaterial(Material material)
        {
            Material m = new Material(material);
            m.mainTexture = originalMaterial.mainTexture;
            m.color = AverageColor();
            renderer.material = m;
        }

        public void ResetMaterial()
        {
            renderer.material = originalMaterial;
        }

        Color AverageColor()
        {
            Color sum = colors[0];
            for (int i = 1; i < colors.Count; i++)
            {
                sum += colors[i];
            }
            sum /= colors.Count;
            return sum;
        }
    };

    readonly Dictionary<Collider, Visual> visuals = new Dictionary<Collider, Visual>();

    public void AddVisual(Collider collider, Color color)
    {
        if (collider == null) return;

        if (visuals.ContainsKey(collider))
        {
            Visual visual = visuals[collider];
            visual.count++;
            visual.colors.Add(color);
            visuals[collider] = visual;
        }
        else
        {
            visuals[collider] = new Visual(collider, color);
        }

        visuals[collider].SetMaterial(highlightMaterial);
    }

    public void RemoveVisual(Collider collider, Color color)
    {
        if (collider == null) return;

        if (visuals.ContainsKey(collider))
        {
            Visual visual = visuals[collider];
            
            if (visuals[collider].count > 0)
            {
                visual.count--;
                visual.colors.Remove(color);
                visual.SetMaterial(highlightMaterial);
                visuals[collider] = visual;
            }
            else
            {
                visual.ResetMaterial();
                visuals.Remove(collider);
            }
        }
    }
}
