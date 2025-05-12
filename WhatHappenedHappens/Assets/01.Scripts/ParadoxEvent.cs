using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [ 패러독스 액션 관리 Enum ] : 추가할 액션 여기에 정의해주기 ! 
public enum ActionType 
{ 
    MoveBox,
    MoveBall
}


// [ 시간 기록 + 이후 자동 실행 이벤트 정의 ]
// 
public class ParadoxEvent 
{
    public float time;
    public GameObject target;
    public ActionType action;

    public ParadoxEvent(float time, GameObject target, ActionType action)
    {
        this.time = time;
        this.target = target;
        this.action = action;
    }

    public void Execute()
    {
        switch (action)
        {
            case ActionType.MoveBox:
                target.GetComponent<Box>().MoveRight();
                Debug.Log($"[Paradox] 박스 이동 실행 at {time}s");
                break;
            case ActionType.MoveBall:
                target.GetComponent<Ball>().MoveUp();
                Debug.Log($"[Paradox] 공 이동 실행 at {time}s");
                break;
        }
    }
}
