using System.Collections;
using Manager;
using UnityEngine;
using UnityEngine.UI;

public class PingSearcher : MonoBehaviour
{
    [SerializeField] private Transform centerTransform;

    [SerializeField] private float      seuil;
    [SerializeField] private float      timeToUnping = 2f;
    private                  AppManager _appManager;
    private                  Image      _image;
    private                  MenuSystem _menuSystem;

    private GameObject assignedPing;
    private bool       watching;

    public GameObject AssignedPing
    {
        set
        {
            assignedPing = value;
        }
    }

    private void Awake()
    {
        watching    = false;
        _image      = GetComponentInChildren<Image>();
        _menuSystem = GetComponentInParent<MenuSystem>();
        _appManager = _menuSystem.appManager;

        //TODO pas besoin de script pour le script, on peut le destroy (pooling plus tard ?)
    }

    private void Update()
    {
        // mesurer l'angle pour vérifier qu'il faut utiliser le pingSearcher
        Transform camPosition  = _appManager.CamTransform;
        Vector3   vectorToPing = assignedPing.transform.position - camPosition.position;
        float     angle        = Mathf.Abs(Vector3.Angle(camPosition.forward, vectorToPing));

        //PrintVar.Print(3, $"Angle: {angle}");

        if (angle < seuil)
        {
            if (watching) return;

            _image.enabled = false;
            watching       = true;
            StartCoroutine(Watching(Time.time));
            return;
        }

        watching = false;
        Vector3 projectedVector = Vector3.ProjectOnPlane(vectorToPing, camPosition.forward);
        float   peripheralAngle = Vector3.Angle(camPosition.right, projectedVector);

        //PrintVar.Print(4, $"PeriAngle: {peripheralAngle}");

        centerTransform.localEulerAngles = new Vector3(0, 0, peripheralAngle);
        _image.enabled                   = true;
    }

    private void OnDisable()
    {
        assignedPing = null;
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
}
