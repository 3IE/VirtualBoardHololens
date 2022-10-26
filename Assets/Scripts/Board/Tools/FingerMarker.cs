using Manager;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Board
{
    public class FingerMarker : MonoBehaviour
    {
        [SerializeField]
        private AppManager     appManager;
        //private bool           _isPoking;
        private Marker         _marker;
        private Transform      _indexTipTransform;

        public GameObject sphereDebug;

        private void Start()
        {
            _marker            = appManager.GetComponent<Marker>();
            _indexTipTransform = GetComponentInChildren<NearInteractionModeDetector>().transform;
        }

        private void Update()
        {
            //if (!_isPoking) return;
            if (erasing) return;
            
            Vector3 indexTipBack = new (_indexTipTransform.position.x, _indexTipTransform.position.y, _indexTipTransform.position.z - 0.1f);
            if (!Physics.Raycast(indexTipBack, _indexTipTransform.forward, out RaycastHit hit, 0.2f, LayerMask.GetMask("Board")))
                _marker.StopDraw();
            else
            {
                sphereDebug.transform.position = hit.point;
                PrintVar.print(2, $"Drawing at {hit.textureCoord}");
                //TODO Draw from raycast hit point to the board
                _marker.TryDraw(hit);
            }
        }

        private bool erasing = false;
        public void Erase()
        {
            erasing = true;
            _marker.Eraser(true);
            
            Vector3 indexTipBack = new (_indexTipTransform.position.x + 0.3f, _indexTipTransform.position.y, _indexTipTransform.position.z - 0.4f);
            if (!Physics.Raycast(indexTipBack, -_indexTipTransform.up, out RaycastHit hit, 1f, LayerMask.GetMask("Board")))
                _marker.StopDraw();
            else
            {
                sphereDebug.transform.position = hit.point;
                PrintVar.print(2, $"Drawing at {hit.textureCoord}");
                //TODO Draw from raycast hit point to the board
                _marker.TryDraw(hit);
            }
        }
        
        public void StopErase()
        {
            erasing = false;
            _marker.StopDraw();
            _marker.Eraser(false);
        }
        
        // public void OnActivating() => _isPoking = true;

        // public void OnDeactivating() => _isPoking = false;
    }
}
