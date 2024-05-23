using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorUI : MonoBehaviour
{    
    public Texture2D EmptyTexture;
    public Image PlayerCursor;

    void Start()
    {
        Cursor.SetCursor(EmptyTexture, Vector2.zero, CursorMode.Auto);
    }

    void Update()
    {
        PlayerCursor.rectTransform.position = Input.mousePosition;
    }
}
