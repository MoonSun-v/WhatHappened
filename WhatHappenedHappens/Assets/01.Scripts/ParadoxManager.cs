using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// [ �з����� �ý����� �ٽ� ���� ]
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

        // ��ȭ �����̸� ���� �з����� ����� ���� ������
        if (paradoxQueue.Count > 0 || ghostQueue.Count > 0)
        {
            Debug.Log($"[Paradox] ���� �з����� {timePassedSinceReplay:F2}s �������� �߶�");
            TrimOngoingReplays(timePassedSinceReplay);
        }

        Debug.Log("[Paradox] ��ȭ ����");
        isRecording = true;
        recordingStartTime = Time.time;
        lastRecordTime = 0f;

        currentRecording.Clear();
        currentPlayerRecording.Clear();

        playerReturnPosition = player.transform.position;
    }

    private void TrimOngoingReplays(float timePassed)
    {
        // ParadoxEvent �߶󳻱�
        Queue<List<ParadoxEvent>> trimmedParadoxQueue = new Queue<List<ParadoxEvent>>();
        foreach (var eventList in paradoxQueue)
        {
            var trimmed = eventList.Where(ev => ev.time >= timePassed)
                                   .Select(ev => new ParadoxEvent(ev.time - timePassed, ev.target, ev.action))
                                   .ToList();
            trimmedParadoxQueue.Enqueue(trimmed);
        }
        paradoxQueue = trimmedParadoxQueue;

        // ��Ʈ �÷��̾� ��� �߶󳻱�
        Queue<List<PlayerMovementRecord>> trimmedGhostQueue = new Queue<List<PlayerMovementRecord>>();
        foreach (var movementList in ghostQueue)
        {
            var trimmed = movementList.Where(record => record.time >= timePassed)
                                      .Select(record => new PlayerMovementRecord(record.time - timePassed, record.position))
                                      .ToList();

            // ��Ʈ�� �ּ� �� �� �̻� �־�� Lerp�� �ǹǷ� �ּ� ���� ����
            if (trimmed.Count >= 2)  trimmedGhostQueue.Enqueue(trimmed);
        }
        ghostQueue = trimmedGhostQueue;

        Debug.Log($"[Paradox] �߸� �� ���� �з����� ��: {paradoxQueue.Count}, ��Ʈ ��: {ghostQueue.Count}");
    }

    // [ �̺�Ʈ ���� ]
    public void RecordEvent(ParadoxEvent ev)
    {
        if (isRecording)
        {
            currentRecording.Add(ev);
            // Debug.Log($"[Paradox] �̺�Ʈ ��ϵ�: {ev.action} at {ev.time}s");
        }
    }

    private void StopRecording()
    {
        isRecording = false;
        Debug.Log("[Paradox] ��ȭ ����");

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

    // [ ��ȭ ���� �� : ������Ʈ ��ġ �ʱ�ȭ ]
    private void ResetScene()
    {
        player.transform.position = playerReturnPosition;

        foreach (var box in FindObjectsOfType<Box>()) box.ResetPosition();

        foreach (var ball in FindObjectsOfType<Ball>()) ball.ResetPosition();
    }

    // [ �з����� ��� �� : ������Ʈ ��ġ �ʱ�ȭ ]
    private IEnumerator ResetObjectsAfterPlayback(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var box in FindObjectsOfType<Box>()) box.ResetPosition();

        foreach (var ball in FindObjectsOfType<Ball>()) ball.ResetPosition();
    }

    // [ �з����� ��� ]
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

    // [ �з����� �̺�Ʈ ���� ]
    private IEnumerator DelayedExecute(ParadoxEvent ev)
    {
        yield return new WaitForSeconds(ev.time);
        ev.Execute();
    }

    // [ ��Ʈ �̵� ]
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
            // Debug.Log($"[Paradox] ��Ʈ �÷��̾� {ghost.name} �ı���");
        }
    }
}
