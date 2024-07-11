using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// temp packages for tmpro ui objects
using TMPro;
using UnityEngine.UIElements;

public class Ranged : MonoBehaviour
{
    public Camera playerCamera;
    public Transform handRigPistol;

    void Start()
    {

    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 aimLocation = playerCamera.WorldToScreenPoint(mousePosition);
        aimLocation.z = 0;
        Debug.DrawLine(aimLocation, aimLocation + new Vector3(1f, 1f, 1f), Color.red, 0.05f);
    }
}