using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [ 패러독스 액션 관리 Enum ]
public enum ActionType { MoveBox }


// [ 시간 기록 + 이후 자동 실행 이벤트 정의 ]
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
                Debug.Log($"[Paradox] {time:F2}s: {target.name} - {action} 이벤트 재현됨");
                break;
        }
    }
}
