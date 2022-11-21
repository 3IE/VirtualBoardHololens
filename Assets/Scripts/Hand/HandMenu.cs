using UI;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField] private VRMenu menu;

    [SerializeField] private float holdTime;

    [SerializeField] private Transform HandFaceTransform;

    private Transform camPosition;
    private float     currentHoldTime;

    private void Start()
    {
        if (Camera.main != null) camPosition = Camera.main.transform;
    }

    private void Update()
    {
        if (Mathf.Round(Vector3.Angle(HandFaceTransform.forward, camPosition.position - HandFaceTransform.position)) < 30)
        {
            currentHoldTime += Time.deltaTime;

            if (currentHoldTime >= holdTime)
            {
                OpenMenu();
                currentHoldTime = 0;
            }
        }
        else
            currentHoldTime = 0;
    }

    private void OpenMenu()
    {
        //? teleport to hand
        menu.transform.SetPositionAndRotation(HandFaceTransform.position, HandFaceTransform.rotation);
        menu.transform.localPosition -= menu.transform.right * 0.2f;

        menu.Open();
    }
}
