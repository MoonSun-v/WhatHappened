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
    private bool isRecording = false;
    private float recordingStartTime;
    private ParadoxData currentParadox;
    private Queue<ParadoxData> paradoxHistory = new Queue<ParadoxData>();
    private int maxParadoxCount = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isRecording)
        {
            StartCoroutine(StartParadox());
        }
    }

    public IEnumerator StartParadox()
    {
        isRecording = true;
        currentParadox = new ParadoxData();
        recordingStartTime = Time.time;
        Debug.Log("[Recording Paradox] ----- 페러독스 녹화 시작 -----");

        yield return new WaitForSeconds(10f); // 10초간 녹화

        isRecording = false;

        // 3개 이상이면 제일 오래된 것 제거
        if (paradoxHistory.Count >= maxParadoxCount)
            paradoxHistory.Dequeue();

        paradoxHistory.Enqueue(currentParadox);
        Debug.Log($"[Recording Paradox] ----- 패러독스 녹화 종료 -----");

        // 페러독스 이전 상태로 리셋
        ResetScene();

        // 기록된 모든 패러독스 동시에 재생
        foreach (var paradox in paradoxHistory)
        {
            StartCoroutine(ReplayEvents(paradox));
        }
    }

    public void RecordEvent(GameObject target, ActionType action)
    {
        if (!isRecording) return;

        ParadoxEvent e = new ParadoxEvent
        {
            target = target,
            action = action,
            timeOffset = Time.time - recordingStartTime
        };
        currentParadox.recordedEvents.Add(e);
        Debug.Log($"[Paradox] 기록됨: {action} / {target.name} / {e.timeOffset}초");
    }

    private void ResetScene()
    {
        // 플레이어 위치 초기화
        GameObject spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
            // Debug.Log($"[Paradox] Player 위치 복원");
        }

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

    private IEnumerator ReplayEvents(ParadoxData paradox)
    {
        foreach (var e in paradox.recordedEvents)
        {
            StartCoroutine(e.Play());
        }
        yield return null;
    }
}
