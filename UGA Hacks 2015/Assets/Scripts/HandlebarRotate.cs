﻿using UnityEngine;
using System.Collections;

public class HandlebarRotate : MonoBehaviour
{

    public float speed = 0.0f;
    public float angle = 0.0f;

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate (0, speed, 0, Space.Self);
        transform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}