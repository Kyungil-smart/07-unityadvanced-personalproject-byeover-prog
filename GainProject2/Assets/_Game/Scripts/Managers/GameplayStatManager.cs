using UnityEngine;

public sealed class GameplayStatManager : MonoBehaviour
{
    private GameManager gameManager;
    private int currentCombo;
    private int maxCombo;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        ResetStats();
    }

    public void ResetStats()
    {
        currentCombo = 0;
        maxCombo = 0;
    }

    public void AddCombo()
    {
        currentCombo++;
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
    }

    public void BreakCombo()
    {
        currentCombo = 0;
    }
}