using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Size")]
    [SerializeField] private Vector3 terrainSize = new Vector3(500f, 120f, 500f);
    [SerializeField] private int heightmapResolution = 513;
    [SerializeField] private int alphamapResolution = 256;

    [Header("Decoration")]
    [SerializeField] private int treeCount = 80;
    [SerializeField] private int grassCount = 220;
    [SerializeField] private int randomSeed = 42;

    private const string TerrainName = "Procedural Terrain";
    private const string DecorationRootName = "Terrain Decorations";

    private Terrain terrain;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            GenerateIfNeeded();
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            GenerateIfNeeded();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            GenerateIfNeeded();
        }
    }

    private void GenerateIfNeeded()
    {
        terrain = FindExistingTerrain();
        if (terrain != null)
        {
            return;
        }

        GenerateTerrain();
        PopulateDecorations();
    }

    [ContextMenu("Regenerate Terrain")]
    private void RegenerateTerrain()
    {
        if (terrain == null)
        {
            terrain = FindExistingTerrain();
        }

        var terrainObject = terrain != null ? terrain.gameObject : null;
        if (terrainObject != null)
        {
            SafeDestroy(terrainObject);
        }

        var decorations = GameObject.Find(DecorationRootName);
        if (decorations != null)
        {
            SafeDestroy(decorations);
        }

        terrain = null;
        GenerateTerrain();
        PopulateDecorations();
    }

    private Terrain FindExistingTerrain()
    {
        var existingTerrains = FindObjectsOfType<Terrain>();
        foreach (var existing in existingTerrains)
        {
            if (existing != null && existing.gameObject.name == TerrainName)
            {
                return existing;
            }
        }

        return null;
    }

    private void GenerateTerrain()
    {
        var terrainData = new TerrainData
        {
            heightmapResolution = heightmapResolution,
            alphamapResolution = alphamapResolution,
            size = terrainSize
        };

        var heights = BuildHeights(heightmapResolution);
        terrainData.SetHeights(0, 0, heights);
        terrainData.terrainLayers = BuildTerrainLayers();
        ApplySplatmap(terrainData);

        var terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = TerrainName;
        terrainObject.transform.position = Vector3.zero;
        terrain = terrainObject.GetComponent<Terrain>();
    }

    private float[,] BuildHeights(int resolution)
    {
        var heights = new float[resolution, resolution];
        var random = new System.Random(randomSeed);

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float nx = x / (float)(resolution - 1);
                float nz = z / (float)(resolution - 1);

                float beachBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.25f, nz));
                float plainBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, 0.6f, nz));
                float mountainBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, nz));

                float noise = Mathf.PerlinNoise(nx * 3f, nz * 3f) * 0.02f;
                float beachHeight = Mathf.Lerp(0.02f, 0.06f, beachBlend) + noise * 0.4f;
                float plainHeight = 0.08f + noise * 0.8f;

                float ridge = Mathf.Clamp01(1f - Mathf.Abs(nx - 0.5f) * 2.8f);
                float mountainBase = 0.15f + ridge * 0.6f;
                float mountainNoise = Mathf.PerlinNoise(nx * 6f, nz * 6f) * 0.08f;
                float mountainHeight = mountainBase + mountainNoise;

                float height = Mathf.Lerp(beachHeight, plainHeight, plainBlend);
                height = Mathf.Lerp(height, mountainHeight, mountainBlend);

                float jitter = ((float)random.NextDouble() - 0.5f) * 0.003f;
                heights[z, x] = Mathf.Clamp01(height + jitter);
            }
        }

        return heights;
    }

    private TerrainLayer[] BuildTerrainLayers()
    {
        var layers = new List<TerrainLayer>
        {
            CreateLayer("SandAlbedo", new Color(0.76f, 0.70f, 0.50f)),
            CreateLayer("GrassRockyAlbedo", new Color(0.36f, 0.47f, 0.25f)),
            CreateLayer("GrassHillAlbedo", new Color(0.29f, 0.55f, 0.30f)),
            CreateLayer("GlassRockyAlbedo", new Color(0.43f, 0.43f, 0.43f)),
            CreateLayer("Cliff", new Color(0.35f, 0.35f, 0.35f))
        };

        return layers.ToArray();
    }

    private TerrainLayer CreateLayer(string textureName, Color fallbackColor)
    {
        var layer = new TerrainLayer
        {
            diffuseTexture = CreateSolidTexture(textureName, fallbackColor),
            tileSize = new Vector2(18f, 18f),
            metallic = 0f,
            smoothness = 0f
        };

        return layer;
    }

    private Texture2D CreateSolidTexture(string textureName, Color color)
    {
        var texture = new Texture2D(2, 2);
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply();
        texture.name = textureName;
        return texture;
    }

    private void ApplySplatmap(TerrainData terrainData)
    {
        int layers = terrainData.terrainLayers.Length;
        int resolution = terrainData.alphamapResolution;
        var splatmap = new float[resolution, resolution, layers];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float nx = x / (float)(resolution - 1);
                float nz = z / (float)(resolution - 1);

                float height = terrainData.GetInterpolatedHeight(nx, nz) / terrainData.size.y;
                float slope = terrainData.GetSteepness(nx, nz) / 90f;

                float sand = Mathf.Clamp01(1f - Mathf.InverseLerp(0.03f, 0.09f, height)) * Mathf.Clamp01(1f - slope * 2f);
                float grassRocky = Mathf.Clamp01(1f - Mathf.InverseLerp(0.06f, 0.14f, height)) * (1f - sand);
                float grassHill = Mathf.Clamp01(1f - slope * 1.6f) * Mathf.InverseLerp(0.05f, 0.2f, height);
                float glassRocky = Mathf.Clamp01(slope * 1.7f) * Mathf.InverseLerp(0.12f, 0.45f, height);
                float cliff = Mathf.Clamp01(Mathf.InverseLerp(0.55f, 0.85f, height));

                var weights = new[] { sand, grassRocky, grassHill, glassRocky, cliff };
                Normalize(weights);

                for (int i = 0; i < layers; i++)
                {
                    splatmap[z, x, i] = weights[i];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmap);
    }

    private void Normalize(float[] weights)
    {
        float total = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            total += weights[i];
        }

        if (total <= Mathf.Epsilon)
        {
            weights[0] = 1f;
            for (int i = 1; i < weights.Length; i++)
            {
                weights[i] = 0f;
            }
            return;
        }

        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] /= total;
        }
    }

    private void PopulateDecorations()
    {
        if (terrain == null)
        {
            return;
        }

        var root = GetOrCreateRoot();
        ClearChildren(root.transform);

        var random = new System.Random(randomSeed);
        var treePrefab = CreateTreePrefab();
        var grassPrefab = CreateGrassPrefab();

        for (int i = 0; i < treeCount; i++)
        {
            var position = RandomTerrainPoint(random);
            if (position.y < terrain.transform.position.y + 6f)
            {
                i--;
                continue;
            }

            var tree = Instantiate(treePrefab, position, Quaternion.identity, root.transform);
            float scale = Mathf.Lerp(0.8f, 1.5f, (float)random.NextDouble());
            tree.transform.localScale = new Vector3(scale, scale, scale);
        }

        for (int i = 0; i < grassCount; i++)
        {
            var position = RandomTerrainPoint(random);
            if (position.y < terrain.transform.position.y + 4f)
            {
                continue;
            }

            var grass = Instantiate(grassPrefab, position, Quaternion.Euler(90f, 0f, 0f), root.transform);
            float scale = Mathf.Lerp(0.4f, 0.9f, (float)random.NextDouble());
            grass.transform.localScale = new Vector3(scale, scale, scale);
        }

        SafeDestroy(treePrefab);
        SafeDestroy(grassPrefab);
    }

    private GameObject GetOrCreateRoot()
    {
        var existing = GameObject.Find(DecorationRootName);
        if (existing != null)
        {
            return existing;
        }

        var root = new GameObject(DecorationRootName);
        root.transform.position = Vector3.zero;
        return root;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            SafeDestroy(parent.GetChild(i).gameObject);
        }
    }

    private Vector3 RandomTerrainPoint(System.Random random)
    {
        float x = Mathf.Lerp(0f, terrainData.size.x, (float)random.NextDouble());
        float z = Mathf.Lerp(0f, terrainData.size.z, (float)random.NextDouble());
        float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
        return new Vector3(x, y, z);
    }

    private TerrainData terrainData => terrain != null ? terrain.terrainData : null;

    private GameObject CreateTreePrefab()
    {
        var tree = new GameObject("TreePrefab");
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localScale = new Vector3(0.4f, 1.4f, 0.4f);
        trunk.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.transform.SetParent(tree.transform, false);
        leaves.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        leaves.transform.localPosition = new Vector3(0f, 3f, 0f);

        ApplyMaterial(trunk, new Color(0.4f, 0.27f, 0.18f));
        ApplyMaterial(leaves, new Color(0.23f, 0.5f, 0.25f));

        RemoveCollider(trunk);
        RemoveCollider(leaves);

        return tree;
    }

    private GameObject CreateGrassPrefab()
    {
        var grass = GameObject.CreatePrimitive(PrimitiveType.Quad);
        grass.name = "GrassPrefab";
        ApplyMaterial(grass, new Color(0.2f, 0.6f, 0.25f));
        RemoveCollider(grass);
        return grass;
    }

    private void ApplyMaterial(GameObject target, Color color)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Standard"));
        material.color = color;
        renderer.sharedMaterial = material;
    }

    private void RemoveCollider(GameObject target)
    {
        var collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            SafeDestroy(collider);
        }
    }

    private void SafeDestroy(Object target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
