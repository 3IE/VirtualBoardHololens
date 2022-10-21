using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilize : MonoBehaviour
{
    private Transform camTransform;

    private void Awake()
    {
        camTransform = Camera.main ? Camera.main.transform : null;
    }

    public void StabilizeBoard()
    {
        transform.LookAt(camTransform);
        var transformRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, transformRotation.y + 180, transformRotation.z);
    }
}
