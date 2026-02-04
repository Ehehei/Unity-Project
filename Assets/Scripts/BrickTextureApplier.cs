using UnityEngine;

[ExecuteAlways]
public class BrickTextureApplier : MonoBehaviour
{
    [SerializeField] private Material diffuseMaterial;
    [SerializeField] private Material specularMaterial;
    [SerializeField] private Material bumpedMaterial;
    [SerializeField] private Material bumpedSpecularMaterial;

    [SerializeField] private int textureSize = 64;
    [SerializeField] private Vector2 tiling = new Vector2(3f, 3f);

    private Texture2D brickTexture;
    private Texture2D normalTexture;

    private void OnEnable()
    {
        ApplyTextures();
    }

    private void OnValidate()
    {
        ApplyTextures();
    }

    private void ApplyTextures()
    {
        if (textureSize < 4)
        {
            textureSize = 4;
        }

        brickTexture = CreateBrickTexture(textureSize);
        normalTexture = CreateFlatNormalMap(textureSize);

        ApplyToMaterial(diffuseMaterial, brickTexture, null);
        ApplyToMaterial(specularMaterial, brickTexture, null);
        ApplyToMaterial(bumpedMaterial, brickTexture, normalTexture);
        ApplyToMaterial(bumpedSpecularMaterial, brickTexture, normalTexture);
    }

    private void ApplyToMaterial(Material material, Texture2D albedo, Texture2D normal)
    {
        if (material == null)
        {
            return;
        }

        material.SetTexture("_MainTex", albedo);
        material.SetTextureScale("_MainTex", tiling);

        if (normal != null)
        {
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_BumpMap", normal);
        }
        else
        {
            material.DisableKeyword("_NORMALMAP");
            material.SetTexture("_BumpMap", null);
        }
    }

    private static Texture2D CreateBrickTexture(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 3
        };

        var mortar = new Color32(200, 200, 200, 255);
        var brick = new Color32(150, 60, 40, 255);
        var pixels = new Color32[size * size];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var isMortar = (y % 16 == 0) || (x % 16 == 0 && (y / 8) % 2 == 0);
                pixels[y * size + x] = isMortar ? mortar : brick;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(true, true);
        return texture;
    }

    private static Texture2D CreateFlatNormalMap(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear
        };

        var normal = new Color32(128, 128, 255, 255);
        var pixels = new Color32[size * size];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = normal;
        }

        texture.SetPixels32(pixels);
        texture.Apply(true, true);
        return texture;
    }
}
