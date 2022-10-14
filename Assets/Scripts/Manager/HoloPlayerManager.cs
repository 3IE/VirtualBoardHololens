using System;
using Photon.Pun;
using UnityEngine;

public class HoloPlayerManager : MonoBehaviourPunCallbacks
{
    private EventManager eventManager;
    private AppManager appManager;
    public HandGestureDetection handGestureDetection;
    
    private bool Free;
    
    private void Awake()
    {
        eventManager = GetComponent<EventManager>();
        appManager = GetComponent<AppManager>();
    }
    
    public void Action(Vector2 positionOnScreen, InputManager.actionType actionType) 
    {
        if (!Free) return;
        
        //TODO get ray from hand
        Ray ray = new Ray();
        
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        
        var hitObj = hit.collider.gameObject;

        if (!hitObj.CompareTag("Board")) return;
        switch (actionType)
        {
            case InputManager.actionType.Ping: 
                Ping(hit.point);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }

    public void Ping(Vector3 position)
    {
        var ping = Instantiate(appManager.localPingPrefab,  position, appManager.BoardTransform.rotation, appManager.BoardTransform);
        // Send ping Online
        var localPos = ping.transform.localPosition;
        eventManager.SendNewPingEvent(new Vector2(localPos.x, localPos.y));
    }
}
