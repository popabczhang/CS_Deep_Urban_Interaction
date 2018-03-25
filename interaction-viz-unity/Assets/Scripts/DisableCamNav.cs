using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisableCamNav : MonoBehaviour {

    public CamMaxMouse camMaxMouse;

    void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        camMaxMouse.enabled = false;
    }

    void OnMouseExit()
    {
        Debug.Log("OnMouseExit");
        camMaxMouse.enabled = true;
    }

}
