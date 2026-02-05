using UnityEngine;

public static class FlappyBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Object.FindObjectOfType<FlappyGameController>() != null)
        {
            return;
        }

        var gameRoot = new GameObject("FlappyGame");
        gameRoot.AddComponent<FlappyGameController>();
        Object.DontDestroyOnLoad(gameRoot);
    }
}
