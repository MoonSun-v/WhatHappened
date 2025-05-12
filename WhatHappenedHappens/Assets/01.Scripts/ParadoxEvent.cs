using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [ �з����� �׼� ���� Enum ]
public enum ActionType { MoveBox }


// [ �ð� ��� + ���� �ڵ� ���� �̺�Ʈ ���� ]
// 
public class ParadoxEvent 
{
    public float time;
    public GameObject target;
    public ActionType action;

    public ParadoxEvent(float t, GameObject go, ActionType a)
    {
        time = t;
        target = go;
        action = a;
    }

    public void Execute()
    {
        switch (action)
        {
            case ActionType.MoveBox:
                target.GetComponent<Box>().MoveRight();
                Debug.Log($"[Paradox] {time:F2}s: {target.name} - {action} �̺�Ʈ ������");
                break;
        }
    }
}
