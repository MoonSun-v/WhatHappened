using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Vector3 startPosition;
    public float moveDistance = 1f;
    public float moveDuration = 1f;

    private void Start()
    {
        startPosition = transform.position;
    }

    public void MoveUp()
    {
        StopAllCoroutines();
        StartCoroutine(MoveSmoothly());
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    private IEnumerator MoveSmoothly()
    {
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * moveDistance;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }
}
