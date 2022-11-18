using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using Photon.Pun;
using UnityEngine;

public class PostItObject : MonoBehaviour, IPunObservable
{
    [SerializeField] private bool              owned;
    [SerializeField] private bool              ownedByMe;
    [SerializeField] private bool              anchored;

    public bool OwnedByMe
    {
        set => owned = ownedByMe = value;
    }
    
    private Transform         camTransform;
    
    private ObjectManipulator     objectManipulatorComponent;
    private MRTKTMPInputField     inputField;
    private PhotonTransformView   photonTransformView;
    private Transform             transformComponent;
    //private SphereCollider        sphereCollider;
    private Material              cylinderMaterial;

    private void OnEnable()
    {
        if (Camera.main != null) 
            camTransform                   = Camera.main.transform;
        inputField                         = GetComponentInChildren<MRTKTMPInputField>();
        objectManipulatorComponent         = GetComponent<ObjectManipulator>();
        photonTransformView                = GetComponent<PhotonTransformView>();
        //sphereCollider                     = GetComponentInChildren<SphereCollider>();
        transformComponent                 = GetComponent<Transform>();
        cylinderMaterial                   = GetComponentsInChildren<Renderer>()[0].material;
        cylinderMaterial.color             = Color.black;
        objectManipulatorComponent.enabled = true;
    }

    private void Update()
    {
        if (anchored || owned) return;
        transformComponent.LookAt(camTransform);
    }

    public void OnGrab()
    {
        OwnedByMe = true;
        transformComponent.SetParent(null);
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     //TODO: Check if the collider is board or object to stick
    //     if (!Physics.Raycast(transformComponent.position, -transformComponent.forward, out var hit, 1f)) 
    //         return;
    //     if (!hit.collider.CompareTag("Board") && !hit.collider.CompareTag("Board")) 
    //         return;
    //         
    //     cylinderMaterial.color = Color.green;
    // }
    
    public void OnReleasing()
    {
        OwnedByMe = false;
        if (!Physics.Raycast(transformComponent.position, -transformComponent.forward, out var hit, 0.3f)) 
            return;
        Debug.Log("Hit: " + hit.collider.name);
        if (!hit.collider.CompareTag("Board") && !hit.collider.CompareTag("Object")) 
            return;
        anchored = true;
        transformComponent.SetParent(hit.collider.transform);
        //photonTransformView.enabled = false;
        transformComponent.position = hit.point;
        transformComponent.rotation = Quaternion.LookRotation(hit.normal,
            Vector3.Angle(transformComponent.up, Vector3.up) > 30f ?
                transformComponent.up 
                : Vector3.up);
    }
    
    //private void OnTriggerExit(Collider other)
    //{
    //    cylinderMaterial.color = Color.black;
    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(owned);
            stream.SendNext(anchored);
            stream.SendNext(inputField.text);
            stream.SendNext(transformComponent.position);
            if (ownedByMe)
                stream.SendNext(transformComponent.rotation);
        }
        else
        {
            owned                       = (bool) stream.ReceiveNext();
            anchored                    = (bool) stream.ReceiveNext();
            inputField.text             = (string) stream.ReceiveNext();
            transformComponent.position = (Vector3) stream.ReceiveNext();
            if (owned)
            {
                photonTransformView.m_SynchronizeRotation = true;
                transformComponent.rotation               = (Quaternion) stream.ReceiveNext();
            }
        }
    }
}
