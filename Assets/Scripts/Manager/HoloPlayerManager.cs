using System;
using Microsoft.MixedReality.Toolkit.Input;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager
{
    public class HoloPlayerManager : MonoBehaviourPunCallbacks
    {
        private AppManager           _appManager;
        public  HandGestureDetection handGestureDetection;

        [FormerlySerializedAs("RayInteractor")]
        [SerializeField]
        private MRTKRayInteractor rayInteractor;


        private void Awake()
        {
            _appManager   = GetComponent<AppManager>();
        }

        public void Action(InputManager.ActionType actionType)
        {
            Debug.DrawRay(rayInteractor.rayOriginTransform.position, rayInteractor.rayOriginTransform.forward * 10, Color.red,
                          5);

            if (!Physics.Raycast(rayInteractor.rayOriginTransform.position, rayInteractor.rayOriginTransform.forward, out var hit)) return;
            if (!hit.collider.CompareTag("Board")) return;

            switch (actionType)
            {
                case InputManager.ActionType.Ping:
                    Debug.Log($"Ping");
                    Ping(hit.point);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }

        public void Ping(Vector3 position)
        {
            var ping = Instantiate(_appManager.localPingPrefab, position, _appManager.BoardTransform.rotation,
                                   _appManager.BoardTransform);

            // Send ping Online
            var localPos = ping.transform.localPosition;
            EventManager.SendNewPingEvent(new Vector2(localPos.x, localPos.y));
        }
    }
}
