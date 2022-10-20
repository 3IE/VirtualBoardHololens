using Photon.Pun;
using UnityEngine;
using DeviceType = Utils.DeviceType;

namespace Refactor
{
    public class PlayerEntity : MonoBehaviour, IPunObservable
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        private Transform _boardTransform;

        private bool _isAr;

        private void Awake()
        {
            _boardTransform = Board.Board.Instance.transform;
            
            DontDestroyOnLoad(gameObject);
        }

        public void SetDevice(DeviceType deviceType)
        {
            _isAr = deviceType is not DeviceType.VR or DeviceType.HoloLens;
        }

        public void ReplaceHandsTransforms(Transform newLeftHandTransform, Transform newRightHandTransform)
        {
            Destroy(leftHandTransform.gameObject);
            Destroy(rightHandTransform.gameObject);

            if (!_isAr)
                return;

            this.leftHandTransform  = newLeftHandTransform;
            this.rightHandTransform = newRightHandTransform;
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo messageInfo)
        {
            Vector3 boardPosition = _boardTransform.position;
            
            if (stream.IsWriting)
            {
                stream.SendNext(playerTransform.position - boardPosition);
                stream.SendNext(playerTransform.rotation);

                if (_isAr)
                    return;

                stream.SendNext(leftHandTransform.position - boardPosition);
                stream.SendNext(leftHandTransform.rotation);
                
                stream.SendNext(rightHandTransform.position - boardPosition);
                stream.SendNext(rightHandTransform.rotation);
            }
            else
            {
                playerTransform.position = (Vector3) stream.ReceiveNext() + boardPosition;
                playerTransform.rotation = (Quaternion) stream.ReceiveNext();

                if (_isAr)
                    return;
                
                leftHandTransform.position = (Vector3) stream.ReceiveNext() + boardPosition;
                leftHandTransform.rotation = (Quaternion) stream.ReceiveNext();
                
                rightHandTransform.position = (Vector3) stream.ReceiveNext() + boardPosition;
                rightHandTransform.rotation = (Quaternion) stream.ReceiveNext();
            }
        }
    }
}