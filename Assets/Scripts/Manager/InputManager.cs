using UnityEngine;
using UnityEngine.InputSystem;

namespace Manager
{
    public class InputManager : MonoBehaviour
    {
        public enum ActionType : byte
        {
            Ping,
            PostIt,
        }

        private HoloInput         _holoInput;
        private HoloPlayerManager _holoPlayerManager;

        private void Awake()
        {
            _holoInput         = new HoloInput();
            _holoPlayerManager = GetComponent<HoloPlayerManager>();
        }

        private void OnEnable()
        {
            _holoInput.Enable();
        }

        private void OnDisable()
        {
            _holoInput.Disable();
        }

        public void InSession(bool state)
        {
            if (state)
            {
                _holoInput.Hololens.PinchRightTap.performed += Ping;

                //_holoInput.Hololens.PinchRightHold.performed += PostIt;
            }
            else
            {
                _holoInput.Hololens.PinchRightTap.performed -= Ping;

                // _holoInput.Hololens.PinchRightHold.performed -= PostIt;
            }
        }

        private void PostIt(InputAction.CallbackContext ctx)
        {
            Debug.Log("Post-It");
        }

        private void Ping(InputAction.CallbackContext ctx)
        {
            _holoPlayerManager.Action(ActionType.Ping);
        }
    }
}
