using System;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Photon.Pun;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Manager
{
    public class AppManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private MenuSystem menuSystem;
        private                  Camera     cam;

        [SerializeField] private PingSearcher pingSearcher;

        [SerializeField] private float        refreshRate = 0.2f;
        private                  EventManager _eventManager;
        private                  RaycastHit   _hit;

        private InputManager      _inputManager;
        private HoloPlayerManager _playerManager;

        private Vector2 _touchPos;

        public Transform CamTransform
        {
            get
            {
                return cam.transform;
            }
        }

        public Transform BoardTransform { get; private set; }

        private void Start()
        {
            cam            = Camera.main;
            _inputManager  = GetComponent<InputManager>();
            _eventManager  = GetComponent<EventManager>();
            _playerManager = GetComponent<HoloPlayerManager>();
            BoardTransform = board.transform;
        }

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
            Vector3 currentRotation = BoardTransform.rotation.eulerAngles;
            BoardTransform.rotation.eulerAngles.Set(0, currentRotation.y, 0);
            BoardTransform.GetComponent<ObjectManipulator>().enabled = false;
            menuSystem.SwitchPanel(MenuSystem.MenuIndex.Join);
        }

        #endregion

        #region prefabs

        [Header("Prefabs")]

        //[SerializeField] private GameObject pingBall;
        public GameObject postItPrefab;

        [FormerlySerializedAs("VRAvatarPrefab")]
        public GameObject vrAvatarPrefab;

        [FormerlySerializedAs("ARAvatarPrefab")]
        public GameObject arAvatarPrefab;

        public GameObject localPingPrefab;
        public GameObject onlinePingPrefab;
        public GameObject board;

        #endregion

        #region InRoom

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            StartSession();
        }

        public void StartSession()
        {
            if (!PhotonNetwork.InRoom) return;

            menuSystem.TurnOffMenu();
            board.SetActive(true);
            _playerManager.handGestureDetection.enabled = true;

            _inputManager.InSession(true);
        }

        public void StopSession()
        {
            _inputManager.InSession(false);

            if (board != null)
                board.SetActive(false);
        }

        private void OnlinePing(Vector2 position)
        {
            GameObject ping = Instantiate(onlinePingPrefab, new Vector3(0, -10, 0), BoardTransform.rotation,
                                          board.transform);

            ping.transform.localPosition = new Vector3(position.x, position.y, 0);
            pingSearcher.AssignedPing    = ping;
            pingSearcher.gameObject.SetActive(true);
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
            GameObject postIt = Instantiate(postItPrefab, position, BoardTransform.rotation,
                                            board.transform);

            postIt.GetComponentInChildren<TMP_Text>().text           = text;
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

        private void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            //print($"Code received: {photonEvent.Code}\t{photonEvent.CustomData}");
            switch (eventCode)
            {
                case < 10:
                    break;

                case < 20:
                    OnPlayerEvent((EventCode) eventCode, photonEvent.CustomData);
                    break;

                case < 30:
                    OnToolEvent((EventCode) eventCode, photonEvent.CustomData);
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

        #endregion

        #region PLAYER_EVENTS

        private void OnPlayerEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case EventCode.SendNewPostIt:
                    var     dataArr   = (object[]) data;
                    var     postItPos = (Vector2) dataArr[0];
                    var     text      = (string) dataArr[1];
                    Vector3 boardPos  = board.transform.position;

                    Post_it_Instantiate(
                                        new Vector3(boardPos.x + postItPos.x, boardPos.y + postItPos.y, boardPos.z)
                                      , text, Color.cyan); // On verra plus tard pour que la couleur varie en fc du joueur
                    break;

                case EventCode.SendNewPing:
                    OnlinePing((Vector2) data);
                    break;

                default:
                    throw new ArgumentException($"Invalid Code: {eventCode}");
            }
        }

        #endregion

        #region TOOL_EVENTS

        private void OnToolEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case EventCode.Marker:
                case EventCode.Eraser:
                    Debug.Log("Marker or Eraser");

                    Board.Board.Instance.AddModification(new Modification(data));
                    break;

                case EventCode.Texture:
                    Board.Board.Instance.texture.LoadImage(data as byte[]);
                    break;

                default:
                    throw new ArgumentException("Unknown event code");
            }
        }

        #endregion
        
        #region OBJECT_EVENTS

        private static void OnObjectEvent(EventCode eventCode, object data)
        {
            switch (eventCode)
            {
                case EventCode.SendNewObject:
                    Shape.ReceiveNewObject(data as object[]);
                    break;

                case EventCode.SendDestroy:
                    Shape.ReceiveDestroy((int) data);
                    break;

                case EventCode.SendTransform:
                    Shape.ReceiveTransform(data as object[]);
                    break;

                case EventCode.SendOwnership:
                    Shape.ReceiveOwnership(data as object[]);
                    break;

                case EventCode.SendCounter:
                    Shape.ReceiveCounter((int) data);
                    break;

                default:
                    throw new ArgumentException("Invalid event code");
            }
        }

        #endregion
    }
}
