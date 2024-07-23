using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrab : MonoBehaviour
{
    public BaseMovement baseMovement;

    private LayerMask _layerMaskWall;
    private Collider[] _aboveLedgeBoxColliders;
    private Collider[] _underLedgeBoxColliders;

    void Start()
    { 
        _layerMaskWall = LayerMask.GetMask("Wall");
    }
    void Update()
    {
        if (!(baseMovement.jumpRising || baseMovement.isGrounded || baseMovement.ledgeGrabbed) && (baseMovement.ledgeGrabCooldown >= 0.1f))
        {
            Vector3 aboveLedgePosition =
                transform.position + new Vector3(baseMovement.mouseXDirection, 0.25f, 0);
            
            bool underLedgeOccupied = Physics.OverlapBox(
                transform.position + new Vector3(baseMovement.mouseXDirection, -0.25f, 0), new Vector3(1f, 0.5f, 2f),
                Quaternion.identity, _layerMaskWall).Length != 0;

            bool aboveLedgeVacant = Physics.OverlapBox(
                aboveLedgePosition, new Vector3(2f, 0.5f, 2f),
                Quaternion.identity, _layerMaskWall).Length == 0;
            
            Debug.Log(underLedgeOccupied + " " + aboveLedgeVacant);
            
            if (underLedgeOccupied && aboveLedgeVacant)
            {
                baseMovement.ledgeGrabbed = true;
                baseMovement.ledgePoint = aboveLedgePosition;
                baseMovement.ledgeHangDirection = baseMovement.mouseXDirection;
            }   
        }
    }

    private void OnDrawGizmos()
    {
        if (!(baseMovement.jumpRising || baseMovement.isGrounded))
        { 
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position + new Vector3(baseMovement.mouseXDirection, -0.25f, 0), new Vector3(1f, 0.5f, 2f));
            Gizmos.DrawCube(transform.position + new Vector3(baseMovement.mouseXDirection, 0.25f, 0), new Vector3(2f, 0.5f, 2f));
        } 
    }
}
