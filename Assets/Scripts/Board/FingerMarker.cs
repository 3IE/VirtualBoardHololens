using Manager;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Board
{
    public class FingerMarker : MonoBehaviour
    { 
        [SerializeField]
        private AppManager     appManager;
        private Transform      _boardTransform;
        private bool           _isPoking;
        private Marker         _marker;
        private PokeInteractor _pokeInteractor;
        private NearInteractionModeDetector _nearInteractionModeDetector;

        private GameObject sphere;

        private void Start()
        {
            _marker         = appManager.GetComponent<Marker>();
            _boardTransform = appManager.BoardTransform;
            _pokeInteractor = GetComponentInChildren<PokeInteractor>();
            _nearInteractionModeDetector = GetComponentInChildren<NearInteractionModeDetector>();
        }

        private void Update()
        {
            if (!_isPoking) return;

            Vector3 poke = _pokeInteractor.PokeTrajectory.End;
            PrintVar.print(4, $"Distance: {_boardTransform.InverseTransformPoint(poke).y})");
            if (_boardTransform.InverseTransformPoint(poke).y > 0.1f) return;
            if (!Physics.Raycast(poke, -_boardTransform.up, out RaycastHit hit)) 
                PrintVar.print(2, "Not drawing");
            else
            {
                PrintVar.print(2, "Drawing");
                //TODO Draw from raycast hit point to the board
                _marker.TryDraw(hit);
            }
        }

        public void OnActivating()
        {
            _isPoking = true;
        }

        public void OnDeactivating()
        {
            _isPoking = false;
        }
    }
}
