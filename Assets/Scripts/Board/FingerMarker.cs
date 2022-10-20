using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Board
{
    public class FingerMarker : MonoBehaviour
    {
        private Transform      _boardTransform;
        private bool           _isPoking;
        private Marker         _marker;
        private PokeInteractor _pokeInteractor;

        private GameObject sphere;

        private void Awake()
        {
            _boardTransform = FindObjectOfType<Board>().transform;
            _marker         = _boardTransform.GetComponent<Marker>();
            _pokeInteractor = GetComponentInChildren<PokeInteractor>();
        }

        private void Update()
        {
            if (!_isPoking) return;

            Vector3 poke = _pokeInteractor.PokeTrajectory.End;
            PrintVar.print(4, $"Distance: {_boardTransform.InverseTransformPoint(poke).y})");
            if (_boardTransform.InverseTransformPoint(poke).y > 0.1f) return;
            if (!Physics.Raycast(poke, -_boardTransform.up, out RaycastHit hit)) return;

            //TODO Draw from raycast hit point to the board
            _marker.TryDraw(hit);
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
