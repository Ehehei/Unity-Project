using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1.6f;
    [SerializeField] private float scrollSpeed = 2.8f;
    [SerializeField] private float horizontalSpawn = 8.5f;
    [SerializeField] private float verticalRange = 2.4f;
    [SerializeField] private float gapSize = 2.8f;

    private float timer;
    private bool active;
    private Sprite pipeSprite;
    private Sprite gateSprite;

    public void Configure(Sprite pipe, Sprite gate)
    {
        pipeSprite = pipe;
        gateSprite = gate;
    }

    public void Begin()
    {
        active = true;
        timer = 0f;
    }

    public void Stop()
    {
        active = false;
    }

    private void Update()
    {
        if (!active)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPipePair();
        }
    }

    private void SpawnPipePair()
    {
        float centerY = Random.Range(-verticalRange, verticalRange);

        GameObject parent = new GameObject("PipePair");
        parent.transform.position = new Vector3(horizontalSpawn, 0f, 0f);

        CreatePipe("TopPipe", parent.transform, centerY + gapSize * 0.5f + 3.5f, true);
        CreatePipe("BottomPipe", parent.transform, centerY - gapSize * 0.5f - 3.5f, false);
        CreateGate(parent.transform, centerY);

        PipeMover mover = parent.AddComponent<PipeMover>();
        mover.Speed = scrollSpeed;
    }

    private void CreatePipe(string name, Transform parent, float yPos, bool flipped)
    {
        GameObject pipe = new GameObject(name);
        pipe.transform.SetParent(parent, false);
        pipe.transform.localPosition = new Vector3(0f, yPos, 0f);
        pipe.transform.localScale = new Vector3(1.5f, 7.2f, 1f);

        SpriteRenderer renderer = pipe.AddComponent<SpriteRenderer>();
        renderer.sprite = pipeSprite;
        renderer.sortingOrder = 5;

        if (flipped)
        {
            renderer.flipY = true;
        }

        BoxCollider2D collider = pipe.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
    }

    private void CreateGate(Transform parent, float centerY)
    {
        GameObject gate = new GameObject("ScoreGate");
        gate.transform.SetParent(parent, false);
        gate.transform.localPosition = new Vector3(0f, centerY, 0f);

        var collider = gate.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.2f, gapSize);
        collider.isTrigger = true;

        var scorer = gate.AddComponent<ScoreGate>();

        var hiddenRenderer = gate.AddComponent<SpriteRenderer>();
        hiddenRenderer.sprite = gateSprite;
        hiddenRenderer.color = new Color(0f, 0f, 0f, 0f);

        scorer.enabled = true;
    }
}
