using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PhoneInput _phoneInput;
    private AppManager _appManager;
    private Vector2 startPosition;
    private void Awake() {
        _phoneInput = new PhoneInput();
        _appManager = GetComponent<AppManager>();
    }
    private void Start()
    {
        _phoneInput.Phone.TouchPing.started += Touching;
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
            _phoneInput.Phone.TouchPing.performed += Ping;
            _phoneInput.Phone.TouchPostIt.performed += PostIt;
        }
        else
        {
            _phoneInput.Phone.TouchPing.performed -= Ping;
            _phoneInput.Phone.TouchPostIt.performed -= PostIt;
        }
    }
    private void Touching(InputAction.CallbackContext ctx)
        => startPosition = _phoneInput.Phone.TouchPosition.ReadValue<Vector2>();

    //private void Setup(InputAction.CallbackContext ctx)
    //    => StartCoroutine(_appManager.SetAnchor(_phoneInput.Phone.TouchPosition.ReadValue<Vector2>()));
    private void PostIt(InputAction.CallbackContext ctx)
        => _appManager.Action(startPosition, actionType.Postit);
    private void Ping(InputAction.CallbackContext ctx)
        => _appManager.Action(startPosition, actionType.Ping);
    public enum actionType : byte {
        Ping,
        Postit
    }
    private void OnEnable() => _phoneInput.Enable();
    private void OnDisable() => _phoneInput.Disable();
}
