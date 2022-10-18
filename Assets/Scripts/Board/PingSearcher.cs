using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PingSearcher : MonoBehaviour
{
    private MenuSystem _menuSystem;
    private AppManager _appManager;
    [SerializeField] private Transform centerTransform;
    
    private GameObject assignedPing;
    public GameObject AssignedPing { set => assignedPing = value; }
    
    [SerializeField] private float seuil;
    [SerializeField] private float timeToUnping = 2f;
    private bool watching;
    private Image _image;

    private void Awake()
    {
        watching = false;
        _image = GetComponentInChildren<Image>();
        _menuSystem = GetComponentInParent<MenuSystem>();
        _appManager = _menuSystem.appManager;
        //TODO pas besoin de script pour le script, on peut le destroy (pooling plus tard ?)
    }

    private void Update()
    {
        // mesurer l'angle pour vérifier qu'il faut utiliser le pingSearcher
        var camPosition = _appManager.CamTransform;
        var vectorToPing = assignedPing.transform.position - camPosition.position;
        var angle = Mathf.Abs(Vector3.Angle(camPosition.forward, vectorToPing));
        //PrintVar.Print(3, $"Angle: {angle}");

        if (angle < seuil)
        {
            if (watching) return;
            _image.enabled = false;
            watching = true;
            StartCoroutine(Watching(Time.time));
            return;
        }
        watching = false;
        var projectedVector = Vector3.ProjectOnPlane(vectorToPing, camPosition.forward);
        var peripheralAngle = Vector3.Angle(camPosition.right, projectedVector);
        //PrintVar.Print(4, $"PeriAngle: {peripheralAngle}");
        
        centerTransform.localEulerAngles = new Vector3(0, 0, peripheralAngle);
        _image.enabled = true;
    }

    private IEnumerator Watching(float start)
    {
        while (watching && Time.time - start < timeToUnping)
            yield return new WaitForEndOfFrame();
        if (watching)
        {
            Destroy(assignedPing);
            gameObject.SetActive(false);
        }
    }
    private void OnDisable() => assignedPing = null;
}
