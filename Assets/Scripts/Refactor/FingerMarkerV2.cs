using UnityEngine;

namespace Refactor
{
    public class FingerMarkerV2 : MonoBehaviour
    {
        private MarkerSync _marker;

        private void Start()
        {
            _marker = MarkerSync.LocalInstance;
        }

        public void Erase()
        {
            _marker.Erase();
        }

        public void StopErase()
        {
            _marker.StopErasing();
        }
    }
}
