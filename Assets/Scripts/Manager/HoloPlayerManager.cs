using System;
using Microsoft.MixedReality.Toolkit.Input;
using Photon.Pun;
using UnityEngine;

public class HoloPlayerManager : MonoBehaviourPunCallbacks
{
    private EventManager eventManager;
    private AppManager appManager;
    public HandGestureDetection handGestureDetection;
    [SerializeField] private MRTKRayInteractor RayInteractor;

    
    private void Awake()
    {
        eventManager = GetComponent<EventManager>();
        appManager = GetComponent<AppManager>();
    }
    
    public void Action(InputManager.actionType actionType) 
    {
        Debug.DrawRay(RayInteractor.rayOriginTransform.position, RayInteractor.rayOriginTransform.forward * 10, Color.red, 5);
        
        if (!Physics.Raycast(RayInteractor.rayOriginTransform.position, RayInteractor.rayOriginTransform.forward, out var hit)) return;
        if (!hit.collider.CompareTag("Board")) return;
        
        switch (actionType)
        {
            case InputManager.actionType.Ping:
                Debug.Log($"Ping");
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
