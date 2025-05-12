using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementRecord
{
    public float time;
    public Vector3 position;

    public PlayerMovementRecord(float time, Vector3 position)
    {
        this.time = time;
        this.position = position;
    }
}
