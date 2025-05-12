using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var box = GameObject.Find("Box");
            box.GetComponent<Box>().MoveRight();

            ParadoxManager.Instance.RecordEvent(box, ActionType.MoveBox);
        }
    }
}
