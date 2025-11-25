using UnityEngine;

public class TutorialChoiceDetector : MonoBehaviour
{
    public GameObject enemy;
    public SubtitleTrigger followingSequence;
    public SubtitleTrigger sparedSequence;

    private bool hasTriggered = false;
    public bool enemyKilled = false;

    public void EnemyKilledBool()
    {
        enemyKilled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (!enemyKilled)
                sparedSequence.StartSequence();
            else
                followingSequence.StartSequence();
        }
    }
}
