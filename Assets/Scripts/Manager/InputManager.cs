using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private HoloInput holoInput;
    private AppManager _appManager;
    private Vector2 startPosition;
    private void Awake() {
        holoInput = new HoloInput();
        _appManager = GetComponent<AppManager>();
    }
    private void Start()
    {
    }
    //public void SettingUpBoard(bool state)
    //{
    //    if (state)
    //        _phoneInput.Phone.TouchPostIt.started += Setup;
    //    else
    //        _phoneInput.Phone.TouchPostIt.started -= Setup;
    //}
    public void InSession(bool state)
    {
        if (state)
        {
            holoInput.Hololens.PinchRight.performed += Ping;
            //holoInput.Phone.TouchPostIt.performed += PostIt;
        }
        else
        {
            holoInput.Hololens.PinchRight.performed -= Ping;
            //holoInput.Phone.TouchPostIt.performed -= PostIt;
        }
    }
    //private void Touching(InputAction.CallbackContext ctx)
    //    => startPosition = holoInput.Phone.TouchPosition.ReadValue<Vector2>();

    //private void Setup(InputAction.CallbackContext ctx)
    //    => StartCoroutine(_appManager.SetAnchor(_phoneInput.Phone.TouchPosition.ReadValue<Vector2>()));
    private void PostIt(InputAction.CallbackContext ctx)
    {
        //_appManager.Action(startPosition, actionType.Postit);
    }
    private void Ping(InputAction.CallbackContext ctx)
    {
        //_appManager.Action(startPosition, actionType.Ping);
    }
    public enum actionType : byte {
        Ping,
        Postit
    }
    private void OnEnable() => holoInput.Enable();
    private void OnDisable() => holoInput.Disable();
}
