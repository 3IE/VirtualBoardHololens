using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Renderer _renderer;

    public const byte NewTextureEventCode = 1;

    private void Start() {
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable() => PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    private void OnDisable() => PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;

    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode != NewTextureEventCode) return;
        
        object[] data = (object[]) photonEvent.CustomData;
        Texture2D newTexture = (Texture2D) data[0];

        _renderer.material.mainTexture = Resize(newTexture, _renderer.material.mainTexture.width, _renderer.material.mainTexture.height);
    }

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);

        Texture2D result = new Texture2D(targetX, targetY);
        
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        
        return result;
    }
}