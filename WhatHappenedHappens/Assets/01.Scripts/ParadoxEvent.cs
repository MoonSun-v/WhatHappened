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
    public float timeOffset;
    public GameObject target;
    public ActionType action;

    public IEnumerator Play()
    {
        yield return new WaitForSeconds(timeOffset);
        Execute();
    }

    public void Execute()
    {
        switch (action)
        {
            case ActionType.MoveBox:
                target.GetComponent<Box>().MoveRight();
                Debug.Log($"[Paradox] {timeOffset}초 후 Box가 이동함: {target.name}");
                break;
            case ActionType.MoveBall:
                target.GetComponent<Ball>().MoveUp();
                Debug.Log($"[Paradox] {timeOffset}초 후 Ball이 이동함: {target.name}");
                break;
        }
    }
}
