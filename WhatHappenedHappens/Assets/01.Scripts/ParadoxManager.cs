using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// [ 패러독스 시스템의 핵심 관리 ]
// 
public class ParadoxManager : MonoBehaviour
{
    public static ParadoxManager Instance;

    private bool isRecording = false;
    private float recordTime = 0f;
    private float paradoxDuration = 10f;

    private List<ParadoxEvent> recordedEvents = new List<ParadoxEvent>();

    private Dictionary<GameObject, ObjectSnapshot> snapshotBeforeParadox;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartParadox();
        }

        if (!isRecording) return;

        recordTime += Time.deltaTime;
        if (recordTime >= paradoxDuration)
        {
            EndParadox();
        }
    }

    public void StartParadox()
    {
        isRecording = true;
        Debug.Log("[Paradox] Paradox 시작");

        recordTime = 0f;
        recordedEvents.Clear();

        snapshotBeforeParadox = TakeSnapshotOfAll(); // 모든 오브젝트의 상태 스냅샷
    }

    public void EndParadox()
    {
        isRecording = false;
        Debug.Log("[Paradox] Paradox 종료");

        RestoreSnapshot(snapshotBeforeParadox); // 스냅샷 복원

        StartCoroutine(ReplayEvents());         // 이후 이벤트 재현 시작
    }

    public void RecordEvent(GameObject target, ActionType actionType)
    {
        if (isRecording)
        {
            recordedEvents.Add(new ParadoxEvent(recordTime, target, actionType));
            Debug.Log($"[Paradox] {recordTime:F2}s: {target.name} - {actionType} 이벤트 기록됨");
        }
    }

    IEnumerator ReplayEvents()
    {
        foreach (var ev in recordedEvents.OrderBy(e => e.time))
        {
            yield return new WaitForSeconds(ev.time);
            ev.Execute();
        }
    }

    private Dictionary<GameObject, ObjectSnapshot> TakeSnapshotOfAll()
    {
        var dict = new Dictionary<GameObject, ObjectSnapshot>();
        foreach (var obj in GameObject.FindObjectsOfType<MonoBehaviour>())
        {
            if (obj is IParadoxObject paradoxObject)
            {
                dict[obj.gameObject] = paradoxObject.CreateSnapshot();
            }
        }
        return dict;
    }

    private void RestoreSnapshot(Dictionary<GameObject, ObjectSnapshot> snapshot)
    {
        foreach (var pair in snapshot)
        {
            if (pair.Key.TryGetComponent<IParadoxObject>(out var paradoxObj))
            {
                paradoxObj.RestoreSnapshot(pair.Value);
            }
        }
    }
}
