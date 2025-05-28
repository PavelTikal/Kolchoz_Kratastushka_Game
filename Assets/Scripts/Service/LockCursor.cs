using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCursor : MonoBehaviour
{
    void Start()
    {
        // zamkne kurzor
        //esc pro odemknuti
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
