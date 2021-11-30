using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] GameObject ObjectToTrack;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(0, ObjectToTrack.transform.position.y + 2.0f, -10);
    }
}
