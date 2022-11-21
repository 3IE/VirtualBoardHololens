using System;
using ExitGames.Client.Photon;
using Manager;
using UnityEngine;
using Utils;

namespace Refactor
{
    public class AppManagerV2 : AppManager
    {
        [SerializeField] private GameObject  boardSetterGameObject;
        private                  BoardSetter _boardSetter;

        protected override void Awake()
        {
            base.Awake();

            _boardSetter = boardSetterGameObject.GetComponent<BoardSetter>();
        }

        protected override void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            //print($"Code received: {photonEvent.Code}\t{photonEvent.CustomData}");
            switch (eventCode)
            {
                case < 10:
                    break;

                case < 20:
                    OnPlayerEvent((EventCode) eventCode, photonEvent.CustomData, photonEvent);
                    break;

                case < 30:
                    OnToolEvent((EventCode) eventCode, photonEvent.CustomData, photonEvent);
                    break;

                case < 60:
                    OnObjectEvent((EventCode) eventCode, photonEvent.CustomData);
                    break;

                case >= 200:
                    break;

                default:
                    throw new ArgumentException($"Invalid Code: {eventCode}");
            }
        }

        private void OnToolEvent(EventCode eventCode, object data, EventData photonEvent)
        {
            switch (eventCode)
            {
                case EventCode.Texture:
                    _boardSetter.SetTexture(data as object[]);
                    break;

                case EventCode.MarkerGrab:
                    Players[photonEvent.Sender].ReceiveMarkerGrab(data);
                    break;

                case EventCode.MarkerPosition:
                    Players[photonEvent.Sender].ReceiveMarkerPosition(data);
                    break;

                case EventCode.MarkerColor:
                    Players[photonEvent.Sender].ReceiveMarkerColor(data);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }
        }
    }
}
