using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour, IParadoxObject
{
    public void MoveRight()
    {
        transform.position += Vector3.right * 2;
    }

    public ObjectSnapshot CreateSnapshot()
    {
        return new ObjectSnapshot { position = transform.position, rotation = transform.rotation };
    }

    public void RestoreSnapshot(ObjectSnapshot snapshot)
    {
        transform.position = snapshot.position;
        transform.rotation = snapshot.rotation;
    }
}
