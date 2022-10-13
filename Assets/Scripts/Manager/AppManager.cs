using System.Collections.Generic;
using ExitGames.Client.Photon;
using System.Collections;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Photon.Realtime;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class AppManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private MenuSystem _menuSystem;
    [SerializeField] private Camera cam;
    public Transform CamTransform => cam.transform;
    private Vector2 touchpos;
    private RaycastHit hit;

    [SerializeField] private PingSearcher _pingSearcher;

    #region prefabs

    [Header("Prefabs")]
    //[SerializeField] private GameObject pingBall;
    public GameObject postItPrefab;
    public GameObject VRAvatarPrefab;
    public GameObject ARAvatarPrefab;
    public GameObject localPingPrefab;
    public GameObject onlinePingPrefab;
    public GameObject board;
    
    #endregion
    
    public Transform BoardTransform { get; private set; }
    
    private InputManager _inputManager;
    private EventManager _eventManager;
    private HoloPlayerManager _playerManager;

    private Dictionary<int, GameObject> PlayerList;
    [SerializeField] private float _refreshRate = 0.2f;

    private void Start()
    {
        _inputManager = GetComponent<InputManager>();
        _eventManager = GetComponent<EventManager>();
        _playerManager = GetComponent<HoloPlayerManager>();
        BoardTransform = board.transform;
    }
    
    #region SetupAR

    //public IEnumerator SetAnchor(Vector2 positionOnScreen)
    //{
    //    var ray = cam.ScreenPointToRay(positionOnScreen);
    //    if (!Physics.Raycast(ray, out hit, 1000f, 1 << 3)) // 3: ARPlane
    //        yield break;
    //    
    //    //_inputManager.SettingUpBoard(false);
    //    
    //    ARPlane arPlane = hit.collider.gameObject.GetComponent<ARPlane>();
    //    Quaternion anchorRotation = Quaternion.LookRotation(new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z), Vector3.up);
    //    ARAnchor arAnchor = _arAnchorManager.AttachAnchor(arPlane, new Pose(hit.point, anchorRotation));
    //    var boardVisualiser = Instantiate(boardVisualiserPrefab, hit.point, anchorRotation, arAnchor.transform);
    //    PrintVar.print(0, $"BoardVisu: {boardVisualiser.transform.position}");
    //    setupButtons.SetActive(true);
    //    RaycastHit holoHit = new ();
    //    while (!_menuSystem.ButtonSetup && !_menuSystem.ButtonSetupCancel)
    //    {
    //        // animation
    //        ray = new Ray(cam.transform.position, cam.transform.forward);
    //        if (Physics.Raycast(ray, out holoHit, 1000f, LayerMask.GetMask( "Holo"))) // 6: Holo
    //        {
    //            PrintVar.print(1, $"holoHit: {holoHit.point}");
    //            holoBoardPrefab.SetActive(true);
    //            holoBoardPrefab.transform.SetPositionAndRotation(holoHit.point, boardVisualiser.transform.rotation);
    //        }
    //        PrintVar.print(2, $"Planes: {_arAnchorManager.trackables.count}");
    //        yield return null;
    //    }
    //    Destroy(boardVisualiser);
    //    holoBoardPrefab.SetActive(false);
    //    
    //    if (_menuSystem.ButtonSetupCancel) //?  || holoHit.collider == null
    //    {
    //        Destroy(arAnchor);
    //        //_inputManager.SettingUpBoard(true);
    //    }
    //    else // ButtonSetup == true
    //    {
    //        board.transform.SetPositionAndRotation(holoHit.point, boardVisualiser.transform.rotation);
    //        board.transform.SetParent(arAnchor.transform);
    //        
    //        board.SetActive(true);
    //        yield return new WaitForSeconds(1f);
    //        board.SetActive(false);
    //        
    //        BoardTransform.rotation = board.transform.rotation;
    //        _arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
    //        
    //        //foreach (var plane in _arAnchorManager.trackables)
    //        //    if (plane.trackableId != arPlane.trackableId)
    //        //        plane.gameObject.SetActive(false);
    //        ARSetup(false);
    //        _menuSystem.SwitchSubpanel(MenuSystem.MenuIndex.Join);
    //    }
    //    setupButtons.SetActive(false);
    //    _menuSystem.ButtonSetup = _menuSystem.ButtonSetupCancel = false;
    //}
    
    public void SetBoard()
    {
        var currentRotation = BoardTransform.rotation.eulerAngles;
        BoardTransform.rotation.eulerAngles.Set(0, currentRotation.y, 0);
        BoardTransform.GetComponent<ObjectManipulator>().enabled = false;
        _menuSystem.SwitchPanel(MenuSystem.MenuIndex.Join);
    }

    #endregion

    #region InRoom

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        StartSession();
    }

    public void StartSession() {
        if (!PhotonNetwork.InRoom) return;
        
        _menuSystem.TurnOffMenu();
        board.SetActive(true);
        _playerManager.handGestureDetection.enabled = true;
        
        PlayerList = new Dictionary<int, GameObject>();
        
        GetAllPlayerInRoom();
        PhotonNetwork.RaiseEvent((byte) Utils.EventCode.SendNewPlayerIn,  CamTransform.position - BoardTransform.position, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
        
        InvokeRepeating(nameof(SendNewPosition) , _refreshRate, _refreshRate);
        _inputManager.InSession(true);
    }
    public void StopSession() {
        _inputManager.InSession(false);
        CancelInvoke();
        if (board != null)
            board.SetActive(false);
    }

    private void SendNewPosition() => _eventManager.SendNewPositionEvent();
    
    private void OnlinePing(Vector2 position)
    {
        var ping = Instantiate(onlinePingPrefab, new Vector3(0, -10, 0), BoardTransform.rotation, board.transform);
        ping.transform.localPosition = new Vector3(position.x, position.y, 0);
        _pingSearcher.AssignedPing = ping;
        _pingSearcher.gameObject.SetActive(true);
    }
    
    //private IEnumerator Post_it(Vector3 position)
    //{
    //    Free = false;
    //    yield return PingBall(position);
    //    yield return _menuSystem.Post_it_Prompt();
    //    if (_menuSystem.postItText != "") {
    //        var postIt = Post_it_Instantiate(position, _menuSystem.postItText,Color.yellow);
    //        var localPos = postIt.transform.localPosition;
    //        SendNewPostItEvent(new Vector2(localPos.x, localPos.y), _menuSystem.postItText);
    //    }
    //    Free = true;
    //}
    private GameObject Post_it_Instantiate(Vector3 position, string text, Color color)
    {
        var postIt = Instantiate(postItPrefab,  position, BoardTransform.rotation, board.transform);
        postIt.GetComponentInChildren<TMP_Text>().text = text;
        postIt.GetComponentInChildren<Renderer>().material.color = color;
        return postIt;
    }
    
    // Pool a ball at a position, then 1sec later pull it back
    //private IEnumerator PingBall(Vector3 position)
    //{
    //    pingBall.transform.position = position;
    //    pingBall.SetActive(true);
    //    yield return new WaitForSeconds(1);
    //    pingBall.SetActive(false);
    //    // pingBall.transform.position = new Vector3(0, 0, 0); // Pas forcément important
    //}
    //public static void SendNewPostItEvent(Vector2 position, string text)
    //{
    //    // We send the whole texture
    //    object[] content = { position, text };

    //    // We send the data to every other person in the room
    //    var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

    //    // We send the event
    //    PhotonNetwork.RaiseEvent((byte) Event.EventCode.SendNewPostIt, content, raiseEventOptions, SendOptions.SendReliable);
    //}
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        AddNewPlayerIn(newPlayer);
    }
    public void GetAllPlayerInRoom()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            if (PhotonNetwork.LocalPlayer.ActorNumber != player.ActorNumber)
                AddNewPlayerIn(player);
    }
    protected void AddNewPlayerIn(Player player)
    {
        // get device, id and username from player
        //print($"Custom properties:{player.CustomProperties}");
        //PrintVar.Print(2, $"Nb Player In Room: {PhotonNetwork.CurrentRoom.Players.Count}");
        bool AR = ((byte) player.CustomProperties["Device"]) > 0;
        var username = player.NickName;
        var id = player.ActorNumber;
        var prefab = AR ? ARAvatarPrefab : VRAvatarPrefab;
        var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        PlayerList[id] = obj;
        obj.GetComponentInChildren<TMP_Text>().text = username;
        print($"AddNewPlayer:\nobj:{obj}\nusername:{username}\nCustomProp:{player.CustomProperties}");
    }
    private void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        //print($"Code received: {photonEvent.Code}\t{photonEvent.CustomData}");
        switch ((Utils.EventCode)eventCode)
        {
            case Utils.EventCode.SendNewPostIt:
                var data = (object[]) photonEvent.CustomData;
                var postItPos = (Vector2) data[0];
                var text = (string) data[1];
                var boardPos = board.transform.position;
                Post_it_Instantiate(
                    new Vector3(boardPos.x + postItPos.x, boardPos.y + postItPos.y, boardPos.z)
                    , text, Color.cyan); // On verra plus tard pour que la couleur varie en fc du joueur
                break;
            case Utils.EventCode.SendNewPosition:
                if (!PlayerList.ContainsKey(photonEvent.Sender))
                    return;
                MoveOverTime(photonEvent.Sender, (Vector3) photonEvent.CustomData);
                break;
            case Utils.EventCode.SendNewPing:
                OnlinePing((Vector2) photonEvent.CustomData);
                break;
        }
    }
    private void MoveOverTime(int id, Vector3 position)
    {
        position += BoardTransform.position;
        var gameObj = PlayerList[id];
        //PrintVar.Print(3, $"Moving {id} from {gameObj.transform.position} to {position}");
        StartCoroutine(MoveOverTimeLerp(gameObj.transform, gameObj.transform.position, position, _refreshRate));
    }
    private IEnumerator MoveOverTimeLerp(Transform obj, Vector3 start, Vector3 end, float time)
    {
        float i = 0.0f;
        while (i < time)
        {
            obj.position = Vector3.Lerp(start, end, i / time);
            i += Time.fixedDeltaTime;
            yield return null;
        }
        obj.LookAt(BoardTransform);
    }
    
    #endregion

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
}
