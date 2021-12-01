using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] GameObject ObjectToTrack;
    [SerializeField] float trackDistance = 18.0f;
    [SerializeField] float cameraBottom = 2.0f;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(0, ObjectToTrack.transform.position.y, -trackDistance);

        if (transform.position.y <= cameraBottom)
        {
            transform.position = new Vector3(0, cameraBottom, -trackDistance);
        }
    }
}
