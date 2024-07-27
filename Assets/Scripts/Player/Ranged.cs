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

    private Vector3 _playerGunRadiusOffset;

	Vector3 aimLocation;

    void Start()
    {
        baseMovement = this.GetComponent<BaseMovement>();
        
        // MANAGING IK CONSTRAINT DATA FOR HANDGUN-RIG, THEN REBUILDING RIG
        rigBuilder.Build();
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 82f;
        
        float origin_relative_mousex = mousePosition.x - (Screen.width / 2f);
        
        aimLocation = playerCamera.ScreenToWorldPoint(mousePosition);
        aimLocation.z = 0f;

        leftHandIK.weight = 1f;

        if (baseMovement._sliding)
        {
            // gun radial origin when sliding to the left
            if (origin_relative_mousex < 0f)
            {
                _playerGunRadiusOffset = new Vector3(6f, 6f, 0f);
            }
            // gun radial origin when sliding to the right
            else
            {
                _playerGunRadiusOffset = new Vector3(-6f, 6f, 0f);
            }
        }
        // gun radial origin when crouching/walking
        else if (baseMovement._crouchingGrounded || baseMovement._crouchingCeilingLocked)
        {
            _playerGunRadiusOffset = new Vector3(0f, 7.5f, 0f);
        }
        // gun radial origin whenever else player is aiming
        else if (!baseMovement.isGrounded)
		{
            if (baseMovement.ledgeGrabbed)
            {
                _playerGunRadiusOffset = new Vector3(-3f * baseMovement.ledgeHangDirection, 9.5f, 0f);
                leftHandIK.weight = 0f;
            }
            else
            {
                _playerGunRadiusOffset = new Vector3(0f, 13.5f, 0f);
            }
		}
		else
        {
            _playerGunRadiusOffset = new Vector3(0f, 15f, 0f);
        }        
        
        // SETTING LOCATION FOR PISTOL GRIP, ROTATING AROUND PLAYER BY CURSOR. RADIUS IS 1f.
        float theta = Mathf.Atan2(aimLocation.y - transform.position.y - _playerGunRadiusOffset.y, aimLocation.x - transform.position.x - _playerGunRadiusOffset.x);
        float newX = Mathf.Cos(theta);
        float newY = Mathf.Sin(theta);
        
        Vector3 handPoint = new Vector3((4.5f * newX) + _playerGunRadiusOffset.x, (4.5f * newY) + _playerGunRadiusOffset.y, 0f);
        // Vector3 elbowPoint = handPoint / 2f;
        
        if (origin_relative_mousex < 0f)
        {
            handPoint.z = 1.5f;
            leftHandTarget.eulerAngles = new Vector3((-1) * theta * 180f / Mathf.PI, 90f, 180f);
        }
        else
        {
            handPoint.z = -1.5f;
            leftHandTarget.eulerAngles = new Vector3((-1) * theta * 180f / Mathf.PI, 90f, 0f);
        }
        
        leftHandTarget.position = handPoint + transform.position;
        
        // rightHandTarget.position = handPoint + transform.position;
        // leftElbowTarget.position = elbowPoint + transform.position;
        // rightElbowTarget.position = elbowPoint + transform.position;
        // handRigPistol.position = handPoint + transform.position;

        
        if (Mathf.Pow(
                (Mathf.Pow(aimLocation.y - transform.position.y - _playerGunRadiusOffset.y, 2f) +
                 Mathf.Pow(aimLocation.x - transform.position.x - _playerGunRadiusOffset.x, 2f)), 0.5f)
            > 1f)
        {
            // Debug.Log(aimLocation);
            // Debug.DrawRay(aimLocation, aimLocation + new Vector3(0f, 1f, 0f), Color.red, 0.05f, false);
        }
        else
        {
            // Debug.Log(handPoint);
            // Debug.DrawRay(handPoint, handPoint + new Vector3(0f, 1f, 0f), Color.red, 0.05f, false);
        }
        
        // RIG LAYER [DIS/EN]ABLING BASED ON MOVEMENT & MOUSE INPUTS
        List<RigLayer> rigLayers = rigBuilder.layers;
        if ((!Input.GetMouseButton(1)) || ((baseMovement.running) && (Input.GetMouseButton(1))))
        {
            rigLayers[0].active = false;
            rigLayers[1].active = false;
            handRigPistol.gameObject.SetActive(false);
        }
        else
        {
            rigLayers[0].active = true;
            rigLayers[1].active = true;
            handRigPistol.gameObject.SetActive(true);
        }
    }

	void OnDrawGizmos()
	{
		// Gizmos.color = new Color(1, 0, 0, 0.5f);
        // Gizmos.DrawCube(aimLocation, new Vector3(5, 5, 5));
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