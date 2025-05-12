using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ �ڽ� �̺�Ʈ ]
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
