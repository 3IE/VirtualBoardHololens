using UI;
using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField] private VRMenu menu;
    
    [SerializeField] private float holdTime;
    private float currentHoldTime;
    
    private Transform camPosition;
    [SerializeField]
    private Transform HandFaceTransform;
    
    private void Start()
    {
        if (Camera.main != null) camPosition = Camera.main.transform;
    }

    private void Update()
    {
        PrintVar.print(8, $"Angle: {Mathf.Round(Vector3.Angle(HandFaceTransform.forward, camPosition.position - HandFaceTransform.position))}");
        if (Mathf.Round(Vector3.Angle(HandFaceTransform.forward, camPosition.position - HandFaceTransform.position)) < 30)
        {
            currentHoldTime += Time.deltaTime;
            if (currentHoldTime >= holdTime)
            {
                menu.Open();
                currentHoldTime = 0;
            }
        }
        else
            currentHoldTime = 0;
    }
}
