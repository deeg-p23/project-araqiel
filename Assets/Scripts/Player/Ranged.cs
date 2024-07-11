using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

// temp packages for tmpro ui objects
using TMPro;
using UnityEngine.UIElements;

public class Ranged : MonoBehaviour
{
    public Camera playerCamera;
    public Transform handRigPistol;
    private BaseMovement baseMovement;

    public TwoBoneIKConstraint leftHandIK;
    public TwoBoneIKConstraint rightHandIK;
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform leftElbowTarget;
    public Transform rightElbowTarget;
    public RigBuilder rigBuilder;

    private float _playerGunRadiusOffset;

    void Start()
    {
        baseMovement = this.GetComponent<BaseMovement>();
        
        // MANAGING IK CONSTRAINT DATA FOR HANDGUN-RIG, THEN REBUILDING RIG
        rigBuilder.Build();
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 18f;
        
        Vector3 aimLocation = playerCamera.ScreenToWorldPoint(mousePosition);
        aimLocation.z = 0f;

        if (baseMovement._crouchingGrounded || baseMovement._crouchingCeilingLocked)
        {
            _playerGunRadiusOffset = 1.25f;
        }
        else
        {
            _playerGunRadiusOffset = 2.5f;
        }
        
        
        // SETTING LOCATION FOR PISTOL GRIP, ROTATING AROUND PLAYER BY CURSOR. RADIUS IS 1f.
        float theta = Mathf.Atan2(aimLocation.y - transform.position.y - _playerGunRadiusOffset, aimLocation.x - transform.position.x);
        float newX = Mathf.Cos(theta);
        float newY = Mathf.Sin(theta);
        
        Vector3 handPoint = new Vector3(0.8f * newX, (0.8f * newY) + _playerGunRadiusOffset, 0f);
        // Vector3 elbowPoint = handPoint / 2f;
        
        float origin_relative_mousex = mousePosition.x - (Screen.width / 2f);
        
        Debug.Log((-1) * theta);

        if (origin_relative_mousex < 0f)
        {
            handPoint.z = 0.25f;
            leftHandTarget.eulerAngles = new Vector3((-1) * theta * 180f / Mathf.PI, 90f, 180f);
        }
        else
        {
            handPoint.z = -0.25f;
            leftHandTarget.eulerAngles = new Vector3((-1) * theta * 180f / Mathf.PI, 90f, 0f);
        }
        
        leftHandTarget.position = handPoint + transform.position;
        
        // rightHandTarget.position = handPoint + transform.position;
        // leftElbowTarget.position = elbowPoint + transform.position;
        // rightElbowTarget.position = elbowPoint + transform.position;
        // handRigPistol.position = handPoint + transform.position;

        
        if (Mathf.Pow(
                (Mathf.Pow(aimLocation.y - transform.position.y - _playerGunRadiusOffset, 2f) +
                 Mathf.Pow(aimLocation.x - transform.position.x, 2f)), 0.5f)
            > 1f)
        {
            // Debug.Log(aimLocation);
            Debug.DrawRay(aimLocation, aimLocation + new Vector3(0f, 1f, 0f), Color.red, 0.05f, false);
        }
        else
        {
            // Debug.Log(handPoint);
            Debug.DrawRay(handPoint, handPoint + new Vector3(0f, 1f, 0f), Color.red, 0.05f, false);
        }
    }

    void LateUpdate()
    {
        leftHandIK.data.target = leftHandTarget;
        rightHandIK.data.target = rightHandTarget;
        leftHandIK.data.hint = leftElbowTarget;
        rightHandIK.data.hint = rightElbowTarget;
    }
    
    private void OnAnimatorMove() {}
}