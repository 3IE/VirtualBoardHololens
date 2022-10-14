using System.Collections;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Board
{
    public class FingerMarker : MonoBehaviour
    {
        private PokeInteractor _pokeInteractor;
        private HandGestureDetection _handGestureDetection;
        private Transform _boardTransform;
        private bool _isPoking;

        private GameObject sphere;

        private void Awake()
        {
            _boardTransform = FindObjectOfType<Board>().transform;
            _pokeInteractor = GetComponentInChildren<PokeInteractor>();
            _handGestureDetection = GetComponent<HandGestureDetection>();
        }
        
        private void Start()
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetPositionAndRotation(new Vector3(-1, 1, 1), Quaternion.identity);
            sphere.transform.lossyScale.Set(0.1f, 0.1f, 0.1f);
        }
        
        //!TMP
        private void Update()
        {
            var poke = _pokeInteractor.PokeTrajectory.End;
            PrintVar.print(4, $"Distance: {_boardTransform.InverseTransformPoint(poke).y})");
            if (!_isPoking || _boardTransform.InverseTransformPoint(poke).y > 0.1f) return;
            if (!Physics.Raycast(poke, -_boardTransform.up, out var hit)) return;
            //TODO Draw from raycast hit point to the board
            //! for debug
            sphere.transform.SetPositionAndRotation(poke, Quaternion.identity);
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