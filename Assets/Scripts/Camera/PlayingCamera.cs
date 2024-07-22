using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingCamera : MonoBehaviour
{
    public Transform Player;
    public Camera LockedCamera;
    public Camera MainCamera;

    [Range(0,1000)]public float dampSpeed;
    Vector3 CameraDistance = new Vector3(0, 0, 0);

    Vector3 CursorCameraChange = new Vector3(0, 0, 0);
    
    [Range(-180, 0)] public float zCamDist;


    private static float CameraShiftRatio = 100f;

    // ADJUST THIS FOR FUTURE RESOLUTIONS
    private float xCameraShiftLimit = Screen.width / CameraShiftRatio;
    private float yCameraShiftLimit = Screen.height / CameraShiftRatio;

    // Update is called once per frame
    void Start()
    {
        CameraDistance.z += zCamDist;
    }

    void LateUpdate()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = MainCamera.nearClipPlane + zCamDist;
        Vector3 aimHit = LockedCamera.ScreenToWorldPoint(mousePos);
        Vector3 playerPosFix = Player.position;

        if ((aimHit.x - playerPosFix.x) / 2f > xCameraShiftLimit)
            CursorCameraChange.x = xCameraShiftLimit;
        else if ((aimHit.x - playerPosFix.x) / 2f < (-1) * xCameraShiftLimit)
            CursorCameraChange.x = (-1) * xCameraShiftLimit;
        else
            CursorCameraChange.x = (aimHit.x - playerPosFix.x) / 2f;

        if ((aimHit.y - playerPosFix.y) / 2f > yCameraShiftLimit)
            CursorCameraChange.y = yCameraShiftLimit;
        else if ((aimHit.y - playerPosFix.y) / 2f < (-1) * yCameraShiftLimit)
            CursorCameraChange.y = (-1) * yCameraShiftLimit;
        else
            CursorCameraChange.y = (aimHit.y - playerPosFix.y) / 2f;
        
        LockedCamera.transform.position = Vector3.Lerp(LockedCamera.transform.position, Player.position + CameraDistance, Time.deltaTime * dampSpeed);
        MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, LockedCamera.transform.position - CursorCameraChange, Time.deltaTime * dampSpeed);
    }
}