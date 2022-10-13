using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private AppManager appManager;
    
    private void Start()
    {
        appManager = GetComponent<AppManager>();
    }

    public void SendNewPingEvent(Vector2 position)
    {
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        PhotonNetwork.RaiseEvent((byte) Utils.EventCode.SendNewPing, position, raiseEventOptions, SendOptions.SendReliable);
    }
    public void SendNewPositionEvent()
    {
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

        PhotonNetwork.RaiseEvent((byte) Utils.EventCode.SendNewPosition, appManager.CamTransform.position - appManager.BoardTransform.position, raiseEventOptions, SendOptions.SendUnreliable);
    }
}
