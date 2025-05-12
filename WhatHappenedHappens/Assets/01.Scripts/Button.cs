using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ 박스 이벤트 ]
// 
public class Button : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Box box = FindObjectOfType<Box>();
            if (box != null)
            {
                box.MoveRight();
                ParadoxManager.Instance.RecordEvent(box.gameObject, ActionType.MoveBox);
            }
        }
    }
}
