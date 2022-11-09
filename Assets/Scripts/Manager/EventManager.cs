using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EventCode = Utils.EventCode;

namespace Manager
{
    public class EventManager : MonoBehaviour
    {
        public static void SendNewPingEvent(Vector2 position)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) EventCode.SendNewPing, position, raiseEventOptions,
                                     SendOptions.SendReliable);
        }
    }
}
