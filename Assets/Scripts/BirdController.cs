using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BirdController : MonoBehaviour
{
    [SerializeField] private float flapVelocity = 5.8f;
    [SerializeField] private float maxRotation = 30f;
    [SerializeField] private float minRotation = -70f;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (FlappyGameController.Instance.IsGameOver)
        {
            return;
        }

        bool pressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (pressed)
        {
            if (!FlappyGameController.Instance.IsRunning)
            {
                body.simulated = true;
                FlappyGameController.Instance.StartRun();
            }

            body.velocity = new Vector2(0f, flapVelocity);
        }

        float normalizedVelocity = Mathf.InverseLerp(-8f, 8f, body.velocity.y);
        float targetRotation = Mathf.Lerp(minRotation, maxRotation, normalizedVelocity);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, targetRotation), Time.deltaTime * 10f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        FlappyGameController.Instance.TriggerGameOver();
    }
}
