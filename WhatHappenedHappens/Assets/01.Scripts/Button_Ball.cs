using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Ball : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Ball ball = FindObjectOfType<Ball>();
            if (ball != null)
            {
                ball.MoveUp();

                float eventTime = Time.time - ParadoxManager.Instance.recordingStartTime;
                var ev = new ParadoxEvent(eventTime, ball.gameObject, ActionType.MoveBall);
                ParadoxManager.Instance.RecordEvent(ev);
            }
        }
    }
}
