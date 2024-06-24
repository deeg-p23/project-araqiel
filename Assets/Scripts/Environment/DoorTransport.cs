using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DoorTransport : MonoBehaviour
{
    public DoorTransport pairedDoor;
    public GameObject player;

    float doorTop;
    float doorBottom;
    float doorLeft;
    float doorRight;
    
    // Start is called before the first frame update
    void Start()
    {
        doorTop = this.gameObject.transform.position.y + (this.gameObject.transform.localScale.y / 2);
        doorBottom = this.gameObject.transform.position.y - (this.gameObject.transform.localScale.y / 2);
        doorLeft = this.gameObject.transform.position.x - (this.gameObject.transform.localScale.x / 2);
        doorRight = this.gameObject.transform.position.x + (this.gameObject.transform.localScale.x / 2);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPos = player.transform.position;
        if (((playerPos.x > doorLeft) && (playerPos.x < doorRight)) &&
            ((playerPos.y > doorBottom) && (playerPos.y < doorTop)))
        {
            Debug.Log("door in-range");
        }
    }
}
