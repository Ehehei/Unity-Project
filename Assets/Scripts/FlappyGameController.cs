using UnityEngine;
using UnityEngine.UI;

public class FlappyGameController : MonoBehaviour
{
    public static FlappyGameController Instance { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsGameOver { get; private set; }

    private int score;
    private BirdController bird;
    private PipeSpawner pipeSpawner;
    private Text scoreText;
    private Text hintText;

    private Sprite solidSprite;
    private Sprite skySprite;
    private Sprite groundSprite;
    private Sprite pipeSprite;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetupCamera();
        CreateSprites();
        BuildWorld();
        BuildUi();
    }

    public void StartRun()
    {
        if (IsRunning || IsGameOver)
        {
            return;
        }

        IsRunning = true;
        hintText.text = string.Empty;
        pipeSpawner.Begin();
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
        {
            return;
        }

        score += amount;
        scoreText.text = score.ToString();
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        IsRunning = false;
        hintText.text = "Game Over\nPress R to Restart";
        pipeSpawner.Stop();
    }

    private void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void SetupCamera()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            var cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.52f, 0.83f, 0.98f);
    }

    private void CreateSprites()
    {
        solidSprite = CreateTextureSprite(1, 1, (x, y) => Color.white);
        skySprite = CreateTextureSprite(64, 64, (x, y) => Color.Lerp(new Color(0.40f, 0.74f, 0.98f), new Color(0.80f, 0.94f, 1.0f), y / 63f));
        groundSprite = CreateTextureSprite(64, 64, (x, y) =>
        {
            float t = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
            return Color.Lerp(new Color(0.55f, 0.37f, 0.20f), new Color(0.45f, 0.28f, 0.14f), t);
        });
        pipeSprite = CreateTextureSprite(16, 64, (x, y) =>
        {
            bool border = x < 2 || x > 13;
            return border ? new Color(0.12f, 0.56f, 0.18f) : new Color(0.20f, 0.72f, 0.24f);
        });
    }

    private void BuildWorld()
    {
        CreateBackgroundLayer("Sky", skySprite, new Vector3(0f, 0f, 5f), -5);
        CreateBackgroundLayer("HillsFar", solidSprite, new Vector3(0f, -1.8f, 4f), -4, new Color(0.62f, 0.85f, 0.56f), new Vector2(22f, 5f));
        CreateBackgroundLayer("HillsNear", solidSprite, new Vector3(0f, -2.4f, 3f), -3, new Color(0.48f, 0.73f, 0.42f), new Vector2(22f, 3.3f));

        var ground = new GameObject("Ground");
        ground.transform.position = new Vector3(0f, -4.7f, 2f);
        var groundRenderer = ground.AddComponent<SpriteRenderer>();
        groundRenderer.sprite = groundSprite;
        groundRenderer.drawMode = SpriteDrawMode.Sliced;
        groundRenderer.size = new Vector2(30f, 2.5f);
        groundRenderer.sortingOrder = 3;

        var groundCollider = ground.AddComponent<BoxCollider2D>();
        groundCollider.size = new Vector2(30f, 2.5f);

        var birdObject = new GameObject("Bird");
        birdObject.transform.position = new Vector3(-2.5f, 0f, 0f);
        var birdRenderer = birdObject.AddComponent<SpriteRenderer>();
        birdRenderer.sprite = solidSprite;
        birdRenderer.color = new Color(1.0f, 0.90f, 0.15f);
        birdRenderer.sortingOrder = 6;
        birdObject.transform.localScale = new Vector3(0.65f, 0.48f, 1f);

        var body = birdObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 2.2f;
        body.freezeRotation = true;
        body.simulated = false;

        var collider = birdObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;

        bird = birdObject.AddComponent<BirdController>();

        var spawnerObject = new GameObject("PipeSpawner");
        spawnerObject.transform.position = Vector3.zero;
        pipeSpawner = spawnerObject.AddComponent<PipeSpawner>();
        pipeSpawner.Configure(pipeSprite, solidSprite);
    }

    private void BuildUi()
    {
        var canvasObject = new GameObject("UI");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        scoreText = CreateText("ScoreText", canvas.transform, new Vector2(0f, -24f), 56, TextAnchor.UpperCenter, "0");
        hintText = CreateText("HintText", canvas.transform, new Vector2(0f, 52f), 28, TextAnchor.MiddleCenter, "Tap / Space to fly");
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, int size, TextAnchor align, string value)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = align;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.white;
        text.text = value;

        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(900f, 180f);
        return text;
    }

    private static void CreateBackgroundLayer(string name, Sprite sprite, Vector3 position, int sortingOrder, Color? color = null, Vector2? size = null)
    {
        var layer = new GameObject(name);
        layer.transform.position = position;
        var renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size ?? new Vector2(24f, 12f);
        renderer.sortingOrder = sortingOrder;
        renderer.color = color ?? Color.white;
    }

    private static Sprite CreateTextureSprite(int width, int height, System.Func<int, int, Color> pixel)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, pixel(x, y));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 16f);
    }
}
