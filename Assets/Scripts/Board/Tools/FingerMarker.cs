using Manager;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Board.Tools
{
    public class FingerMarker : MonoBehaviour
    {
        [SerializeField]
        private AppManager     appManager;
        //private bool           _isPoking;
        private Marker         _marker;
        private Transform      _indexTipTransform;

        private void Start()
        {
            _marker            = appManager.GetComponent<Marker>();
            _indexTipTransform = GetComponentInChildren<NearInteractionModeDetector>().transform;
        }

        /// <summary>
        /// Draw or erase from IndexTip or the palm depending 'erasing' state
        /// </summary>
        private void Update()
        {
            //if (!_isPoking) return;
            
            var indexTipBack = erasing ? new Vector3(_indexTipTransform.position.x + 0.3f, _indexTipTransform.position.y, _indexTipTransform.position.z - 0.4f) : new Vector3(_indexTipTransform.position.x, _indexTipTransform.position.y, _indexTipTransform.position.z - 0.1f);
            if (!Physics.Raycast(indexTipBack, erasing ? -_indexTipTransform.up : _indexTipTransform.forward, out RaycastHit hit,  erasing ? 1f : 0.2f, LayerMask.GetMask("Board")))
                _marker.StopDraw();
            else
            {
                PrintVar.print(2, $"Drawing at {hit.textureCoord}");
                //TODO Draw from raycast hit point to the board
                _marker.TryDraw(hit);
            }
        }

        private bool erasing;
        public void Erase()
        {
            erasing = true;
            _marker.Eraser(true);
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
