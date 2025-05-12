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
    private GameObject ghostPlayerInstance;

    private bool isRecording = false;
    public float recordingStartTime = 0f;
    public float lastRecordTime = 0f;

    private List<ParadoxEvent> currentRecording = new List<ParadoxEvent>();
    private List<PlayerMovementRecord> currentPlayerRecording = new List<PlayerMovementRecord>();
    private List<PlayerMovementRecord> playerReplayData = new List<PlayerMovementRecord>();

    private Queue<List<ParadoxEvent>> paradoxQueue = new Queue<List<ParadoxEvent>>();
    private Queue<List<PlayerMovementRecord>> ghostQueue = new Queue<List<PlayerMovementRecord>>();
    private int maxParadox = 3;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        Debug.Log("[Paradox] 녹화 시작");
        isRecording = true;
        recordingStartTime = Time.time;
        lastRecordTime = 0f;
        currentRecording.Clear();
        currentPlayerRecording.Clear();
        playerReturnPosition = player.transform.position;
    }

    public void RecordEvent(ParadoxEvent ev)
    {
        if (isRecording)
        {
            currentRecording.Add(ev);
            Debug.Log($"[Paradox] 이벤트 기록됨: {ev.action} at {ev.time}s");
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

        ResetScene();
        ReplayParadoxes();
    }

    private void ResetScene()
    {
        player.transform.position = playerReturnPosition;

        // 박스 초기 위치로 되돌리기
        foreach (var box in FindObjectsOfType<Box>())
        {
            box.ResetPosition();
        }

        // 공 초기 위치로 되돌리기
        foreach (var ball in FindObjectsOfType<Ball>())
        {
            ball.ResetPosition();
        }
    }

    private IEnumerator ResetObjectsAfterPlayback(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Debug.Log("[Paradox] 재생 완료 - 오브젝트 위치 초기화");

        foreach (var box in FindObjectsOfType<Box>())
            box.ResetPosition();

        foreach (var ball in FindObjectsOfType<Ball>())
            ball.ResetPosition();
    }

    private void ReplayParadoxes()
    {
        int ghostIndex = 0;
        float maxDuration = 10f; // 패러독스는 항상 10초간 녹화되므로 고정

        foreach (var paradoxEvents in paradoxQueue)
        {
            foreach (var ev in paradoxEvents)
            {
                StartCoroutine(DelayedExecute(ev));
            }

            // 고스트 생성 및 이동 실행
            var ghostData = ghostQueue.ToArray()[ghostIndex];
            GameObject ghost = Instantiate(ghostPlayerPrefab);
            ghost.name = "GhostPlayer_" + ghostIndex;
            ghost.transform.position = ghostData[0].position;
            StartCoroutine(ReplayGhostMovement(ghost, ghostData));
            ghostIndex++;
        }

        // 전체 재생 완료 후 초기화 코루틴 실행
        StartCoroutine(ResetObjectsAfterPlayback(maxDuration));
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
        {
            Destroy(ghost);
            // Debug.Log($"[Paradox] 고스트 플레이어 {ghost.name} 파괴됨");
        }
    }
}
