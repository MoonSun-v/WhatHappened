using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// [ 패러독스 시스템의 핵심 관리 ]
// 
public class ParadoxManager : MonoBehaviour
{
    public static ParadoxManager Instance;
    
    public GameObject player;
    public GameObject ghostPlayerPrefab;

    private Vector3 playerReturnPosition;

    private bool isRecording = false;
    private int maxParadox = 3;

    public float recordingStartTime = 0f;
    public float recordingEndTime = 0f;
    public float lastRecordTime = 0f;

    private List<ParadoxEvent> currentRecording = new List<ParadoxEvent>();
    private List<PlayerMovementRecord> currentPlayerRecording = new List<PlayerMovementRecord>();

    private Queue<List<ParadoxEvent>> paradoxQueue = new Queue<List<ParadoxEvent>>();
    private Queue<List<PlayerMovementRecord>> ghostQueue = new Queue<List<PlayerMovementRecord>>();


    private void Awake()
    {
        if (Instance == null)   Instance = this;
        else                    Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRecording();
        }

        if (isRecording)
        {
            float elapsed = Time.time - recordingStartTime;

            if (elapsed - lastRecordTime >= 0.1f)
            {
                currentPlayerRecording.Add(new PlayerMovementRecord(elapsed, player.transform.position));
                lastRecordTime = elapsed;
            }

            if (elapsed >= 10f)
            {
                StopRecording();
            }
        }
    }

    public void StartRecording()
    {
        float timePassedSinceReplay = Time.time - recordingEndTime;

        // 녹화 도중이면 기존 패러독스 재생의 남은 데이터
        if (paradoxQueue.Count > 0 || ghostQueue.Count > 0)
        {
            Debug.Log($"[Paradox] 기존 패러독스 {timePassedSinceReplay:F2}s 지점부터 잘라냄");
            TrimOngoingReplays(timePassedSinceReplay);
        }

        Debug.Log("[Paradox] 녹화 시작");
        isRecording = true;
        recordingStartTime = Time.time;
        lastRecordTime = 0f;

        currentRecording.Clear();
        currentPlayerRecording.Clear();

        playerReturnPosition = player.transform.position;
    }

    private void TrimOngoingReplays(float timePassed)
    {
        // ParadoxEvent 잘라내기
        Queue<List<ParadoxEvent>> trimmedParadoxQueue = new Queue<List<ParadoxEvent>>();
        foreach (var eventList in paradoxQueue)
        {
            var trimmed = eventList.Where(ev => ev.time >= timePassed)
                                   .Select(ev => new ParadoxEvent(ev.time - timePassed, ev.target, ev.action))
                                   .ToList();
            trimmedParadoxQueue.Enqueue(trimmed);
        }
        paradoxQueue = trimmedParadoxQueue;

        // 고스트 플레이어 기록 잘라내기
        Queue<List<PlayerMovementRecord>> trimmedGhostQueue = new Queue<List<PlayerMovementRecord>>();
        foreach (var movementList in ghostQueue)
        {
            var trimmed = movementList.Where(record => record.time >= timePassed)
                                      .Select(record => new PlayerMovementRecord(record.time - timePassed, record.position))
                                      .ToList();

            // 고스트가 최소 두 개 이상 있어야 Lerp가 되므로 최소 길이 보장
            if (trimmed.Count >= 2)  trimmedGhostQueue.Enqueue(trimmed);
        }
        ghostQueue = trimmedGhostQueue;

        Debug.Log($"[Paradox] 잘린 후 남은 패러독스 수: {paradoxQueue.Count}, 고스트 수: {ghostQueue.Count}");
    }

    // [ 이벤트 저장 ]
    public void RecordEvent(ParadoxEvent ev)
    {
        if (isRecording)
        {
            currentRecording.Add(ev);
            // Debug.Log($"[Paradox] 이벤트 기록됨: {ev.action} at {ev.time}s");
        }
    }

    private void StopRecording()
    {
        isRecording = false;
        Debug.Log("[Paradox] 녹화 종료");

        if (paradoxQueue.Count >= maxParadox)
        {
            paradoxQueue.Dequeue();
            ghostQueue.Dequeue();
        }

        paradoxQueue.Enqueue(new List<ParadoxEvent>(currentRecording));
        ghostQueue.Enqueue(new List<PlayerMovementRecord>(currentPlayerRecording));

        recordingEndTime = Time.time;

        ResetScene();
        ReplayParadoxes(); 
    }

    // [ 녹화 종료 시 : 오브젝트 위치 초기화 ]
    private void ResetScene()
    {
        player.transform.position = playerReturnPosition;

        foreach (var box in FindObjectsOfType<Box>()) box.ResetPosition();

        foreach (var ball in FindObjectsOfType<Ball>()) ball.ResetPosition();
    }

    // [ 패러독스 재생 후 : 오브젝트 위치 초기화 ]
    private IEnumerator ResetObjectsAfterPlayback(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var box in FindObjectsOfType<Box>()) box.ResetPosition();

        foreach (var ball in FindObjectsOfType<Ball>()) ball.ResetPosition();
    }

    // [ 패러독스 재생 ]
    private void ReplayParadoxes()
    {
        var paradoxList = paradoxQueue.ToList();
        var ghostList = ghostQueue.ToList();

        float maxDuration = 10f;

        for (int i = 0; i < paradoxList.Count; i++)
        {
            var paradoxEvents = paradoxList[i];
            var ghostData = (i < ghostList.Count) ? ghostList[i] : null;

            if (paradoxEvents == null || ghostData == null || ghostData.Count < 2)
                continue;

            foreach (var ev in paradoxEvents)
            {
                StartCoroutine(DelayedExecute(ev));
            }

            GameObject ghost = Instantiate(ghostPlayerPrefab);
            ghost.name = "GhostPlayer_" + i;
            ghost.transform.position = ghostData[0].position;
            StartCoroutine(ReplayGhostMovement(ghost, ghostData));
        }

        StartCoroutine(ResetObjectsAfterPlayback(maxDuration));
    }

    // [ 패러독스 이벤트 실행 ]
    private IEnumerator DelayedExecute(ParadoxEvent ev)
    {
        yield return new WaitForSeconds(ev.time);
        ev.Execute();
    }

    // [ 고스트 이동 ]
    private IEnumerator ReplayGhostMovement(GameObject ghost, List<PlayerMovementRecord> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            float waitTime = data[i].time - data[i - 1].time;
            Vector3 start = data[i - 1].position;
            Vector3 end = data[i].position;

            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                if (ghost == null) yield break;

                ghost.transform.position = Vector3.Lerp(start, end, elapsed / waitTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (ghost != null)
                ghost.transform.position = end;
        }

        if (ghost != null)
        {
            Destroy(ghost);
            // Debug.Log($"[Paradox] 고스트 플레이어 {ghost.name} 파괴됨");
        }
    }
}
