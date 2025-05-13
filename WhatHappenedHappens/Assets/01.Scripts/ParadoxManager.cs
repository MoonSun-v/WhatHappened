using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [ 패러독스 시스템의 핵심 관리 ]
public class ParadoxManager : MonoBehaviour
{
    public static ParadoxManager Instance;

    public GameObject player;
    public GameObject ghostPlayerPrefab;

    private Vector3 playerReturnPosition;

    private bool isRecording = false;
    private bool isReplaying = false;
    private int maxParadox = 3;

    public float recordingStartTime = 0f;
    public float recordingEndTime = 0f;
    public float replayStartTime = 0f;
    private float lastRecordTime = 0f;
    private int ghostCounter = 0;

    private List<ParadoxEvent> currentRecording = new List<ParadoxEvent>();
    private List<PlayerMovementRecord> currentPlayerRecording = new List<PlayerMovementRecord>();

    private Queue<List<ParadoxEvent>> paradoxQueue = new Queue<List<ParadoxEvent>>();
    private Queue<List<PlayerMovementRecord>> ghostQueue = new Queue<List<PlayerMovementRecord>>();


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
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
        float timePassedSinceReplay = Time.time - replayStartTime;

        // 현재 재생 중이면 남은 패러독스만 유지
        if (isReplaying && (paradoxQueue.Count > 0 || ghostQueue.Count > 0))
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

        // 오브젝트 위치 초기화 -> 녹화 종료 시점
        ResetScene();

        // 재생
        ReplayParadoxes();
    }


    // [기존 재생 패러독스를 현재 시간 이후만 남김]
    private void TrimOngoingReplays(float timePassed)
    {
        Queue<List<ParadoxEvent>> trimmedParadoxQueue = new Queue<List<ParadoxEvent>>();
        foreach (var eventList in paradoxQueue)
        {
            var trimmed = eventList.Where(ev => ev.time >= timePassed)
                                   .Select(ev => new ParadoxEvent(ev.time - timePassed, ev.target, ev.action))
                                   .ToList();
            trimmedParadoxQueue.Enqueue(trimmed);
        }
        paradoxQueue = trimmedParadoxQueue;

        Queue<List<PlayerMovementRecord>> trimmedGhostQueue = new Queue<List<PlayerMovementRecord>>();
        foreach (var movementList in ghostQueue)
        {
            var trimmed = movementList.Where(r => r.time >= timePassed)
                                      .Select(r => new PlayerMovementRecord(r.time - timePassed, r.position))
                                      .ToList();
            trimmedGhostQueue.Enqueue(trimmed);
        }
        ghostQueue = trimmedGhostQueue;

        Debug.Log($"[Paradox] 잘린 후 남은 패러독스 수: {paradoxQueue.Count}, 고스트 수: {ghostQueue.Count}");
    }


    private void ResetScene()
    {
        player.transform.position = playerReturnPosition;

        foreach (var box in FindObjectsOfType<Box>()) box.ResetPosition();
        foreach (var ball in FindObjectsOfType<Ball>()) ball.ResetPosition();
    }

    public void RecordEvent(ParadoxEvent ev)
    {
        if (isRecording)
        {
            currentRecording.Add(ev);
        }
    }

    private void ReplayParadoxes()
    {
        isReplaying = true;
        replayStartTime = Time.time;

        var paradoxList = paradoxQueue.ToList();
        var ghostList = ghostQueue.ToList();

        ghostCounter = 0;

        for (int i = 0; i < paradoxList.Count; i++)
        {
            var paradoxEvents = paradoxList[i];
            var ghostData = (i < ghostList.Count) ? ghostList[i] : null;

            if (paradoxEvents == null || ghostData == null || ghostData.Count < 2)
            {
                Debug.Log($"[Paradox] 패러독스 {i}의 데이터가 부족합니다.");
                continue;
            }

            foreach (var ev in paradoxEvents)
                StartCoroutine(DelayedExecute(ev));

            GameObject ghost = Instantiate(ghostPlayerPrefab);
            ghost.name = "GhostPlayer_" + i;
            ghost.transform.position = ghostData[0].position;

            ghostCounter++;
            StartCoroutine(ReplayGhostMovement(ghost, ghostData));
        }
    }

    private IEnumerator DelayedExecute(ParadoxEvent ev)
    {
        yield return new WaitForSeconds(ev.time);
        ev.Execute();
    }

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
            Destroy(ghost);

        ghostCounter--;
        if (ghostCounter == 0) ResetObjectsAfterPlayback();
    }

    private void ResetObjectsAfterPlayback()
    {
        foreach (var box in FindObjectsOfType<Box>()) box.ResetPosition();
        foreach (var ball in FindObjectsOfType<Ball>()) ball.ResetPosition();

        isReplaying = false;
    }
}