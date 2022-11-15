using System;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using Photon.Pun;
using UnityEngine;

public class PostItObject : MonoBehaviour, IPunObservable
{
    private bool              owned;
    private bool              ownedByMe;

    public bool OwnedByMe
    {
        set => owned = ownedByMe = value;

    }
    
    private bool              anchored;
    
    private Transform         camTransform;
    
    private ObjectManipulator     objectManipulatorComponent;
    private MRTKTMPInputField     inputField;
    private PhotonTransformView   photonTransformView;
    private Transform             transformComponent;
    private SphereCollider        sphereCollider;
    

    private void OnEnable()
    {
        if (Camera.main != null) 
            camTransform                   = Camera.main.transform;
        inputField                         = GetComponentInChildren<MRTKTMPInputField>();
        objectManipulatorComponent         = GetComponent<ObjectManipulator>();
        photonTransformView                = GetComponent<PhotonTransformView>();
        sphereCollider                     = GetComponentInChildren<SphereCollider>();
        transformComponent                 = GetComponent<Transform>();
        objectManipulatorComponent.enabled = true;
    }

    private void Update()
    {
        if (anchored || owned) return;
        transformComponent.LookAt(camTransform);
    }

    private void OnTriggerEnter(Collider other)
    {
        //TODO: Check if the collider is board or object to stick
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(owned);
            stream.SendNext(inputField.text);
            stream.SendNext(transformComponent.position);
            if (ownedByMe)
                stream.SendNext(transformComponent.rotation);
        }
        else
        {
            owned                       = (bool) stream.ReceiveNext();
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
