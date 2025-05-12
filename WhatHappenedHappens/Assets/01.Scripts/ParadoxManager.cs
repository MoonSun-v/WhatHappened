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
        Debug.Log("[Recording Paradox] ----- �䷯���� ��ȭ ���� -----");

        yield return new WaitForSeconds(10f); // 10�ʰ� ��ȭ

        isRecording = false;

        // 3�� �̻��̸� ���� ������ �� ����
        if (paradoxHistory.Count >= maxParadoxCount)
            paradoxHistory.Dequeue();

        paradoxHistory.Enqueue(currentParadox);
        Debug.Log($"[Recording Paradox] ----- �з����� ��ȭ ���� -----");

        // �䷯���� ���� ���·� ����
        ResetScene();

        // ��ϵ� ��� �з����� ���ÿ� ���
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
        Debug.Log($"[Paradox] ��ϵ�: {action} / {target.name} / {e.timeOffset}��");
    }

    private void ResetScene()
    {
        // �÷��̾� ��ġ �ʱ�ȭ
        GameObject spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
            // Debug.Log($"[Paradox] Player ��ġ ����");
        }

        // �ڽ� �ʱ� ��ġ�� �ǵ�����
        foreach (var box in FindObjectsOfType<Box>())
        {
            box.ResetPosition();
        }

        // �� �ʱ� ��ġ�� �ǵ�����
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
