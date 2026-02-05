using UnityEngine;

public class ScoreGate : MonoBehaviour
{
    private bool scored;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (scored || !other.GetComponent<BirdController>())
        {
            return;
        }

        scored = true;
        FlappyGameController.Instance.AddScore(1);
    }
}
