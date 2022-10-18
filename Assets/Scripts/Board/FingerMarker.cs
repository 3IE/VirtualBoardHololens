using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Board
{
    public class FingerMarker : MonoBehaviour
    {
        private PokeInteractor _pokeInteractor;
        private Transform _boardTransform;
        private Marker _marker;
        private bool _isPoking;

        private GameObject sphere;

        private void Awake()
        {
            _boardTransform = FindObjectOfType<Board>().transform;
            _marker = _boardTransform.GetComponent<Marker>();
            _pokeInteractor = GetComponentInChildren<PokeInteractor>();
        }
        
        private void Update()
        {
            if (!_isPoking) return;
            
            var poke = _pokeInteractor.PokeTrajectory.End;
            PrintVar.print(4, $"Distance: {_boardTransform.InverseTransformPoint(poke).y})");
            if (_boardTransform.InverseTransformPoint(poke).y > 0.1f) return;
            if (!Physics.Raycast(poke, -_boardTransform.up, out var hit)) return;
            //TODO Draw from raycast hit point to the board
            _marker.TryDraw(hit);
        }

        public void OnActivating() => _isPoking = true;

        public void OnDeactivating() => _isPoking = false;
    }
}