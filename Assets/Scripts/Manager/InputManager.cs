using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private HoloInput holoInput;
    private HoloPlayerManager holoPlayerManager;
    private void Awake()
    {
        holoInput = new HoloInput();
        holoPlayerManager = GetComponent<HoloPlayerManager>();
        
        //!TMP
        InSession(true);
    }
    
    public void InSession(bool state)
    {
        if (state)
        {
            holoInput.Hololens.PinchRightTap.performed += Ping;
            holoInput.Hololens.PinchRightHold.performed += PostIt;
        }
        else
        {
            holoInput.Hololens.PinchRightTap.performed -= Ping;
            holoInput.Hololens.PinchRightHold.performed -= PostIt;
        }
    }
    
    private void PostIt(InputAction.CallbackContext ctx)
    {
        Debug.Log($"Post-It");
    }
    
    private void Ping(InputAction.CallbackContext ctx)
        => holoPlayerManager.Action(actionType.Ping);
    
    public enum actionType : byte {
        Ping,
        Postit
    }
    
    private void OnEnable() => holoInput.Enable();
    private void OnDisable() => holoInput.Disable();
}
