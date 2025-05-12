using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [ �з����� �׼� ���� Enum ] : �߰��� �׼� ���⿡ �������ֱ� ! 
public enum ActionType 
{ 
    MoveBox,
    MoveBall
}


// [ �ð� ��� + ���� �ڵ� ���� �̺�Ʈ ���� ]
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
                Debug.Log($"[Paradox] {timeOffset}�� �� Box�� �̵���: {target.name}");
                break;
            case ActionType.MoveBall:
                target.GetComponent<Ball>().MoveUp();
                Debug.Log($"[Paradox] {timeOffset}�� �� Ball�� �̵���: {target.name}");
                break;
        }
    }
}
