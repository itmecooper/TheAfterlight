using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    //stolen from another project bc im lazy, sorry

    public Vector3 targetOffset = new Vector3(0, -2f, 0); // 2 units down
    public float moveDuration = 1.0f;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;


    public void Move()
    {
        if (!isMoving)
        {
            initialPosition = transform.position;
            targetPosition = initialPosition + targetOffset;
            StartCoroutine(MoveToTarget());
        }
    }

    private System.Collections.IEnumerator MoveToTarget()
    {
        isMoving = true;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
