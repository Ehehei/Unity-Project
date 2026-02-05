using UnityEngine;

public class PipeMover : MonoBehaviour
{
    public float Speed { get; set; } = 2.8f;

    private void Update()
    {
        if (FlappyGameController.Instance.IsGameOver)
        {
            return;
        }

        transform.position += Vector3.left * Speed * Time.deltaTime;
        if (transform.position.x < -12f)
        {
            Destroy(gameObject);
        }
    }
}
