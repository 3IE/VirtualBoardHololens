using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
}
public class HandGestureDetection : MonoBehaviour
{
    [SerializeField] private HoloPlayerManager holoPlayerManager;
    
    [SerializeField] private Transform cursor;
    public Transform HandTransform;
    private HandsSubsystem handsSubsystem;
    public XRNode handNode;
    private const int fingerCount = 26;
    
    public List<Gesture> gestureList;
    public float threshold = 0.1f;
    private Gesture previousGesture;

    private void Awake()
    {
        if (HandTransform == null)
            HandTransform = transform;
        previousGesture = new Gesture();
    }

    protected void OnEnable()
    {
        Debug.Assert(handNode is XRNode.LeftHand or XRNode.RightHand, $"HandVisualizer has an invalid XRNode ({handNode})!");

        handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsSubsystem>();

        if (handsSubsystem == null)
            StartCoroutine(EnableWhenSubsystemAvailable());
    }
    private IEnumerator EnableWhenSubsystemAvailable()
    {
        yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
        OnEnable();
    }
    
    private void Update()
    {
        //! TMP: Allow to make more gestures
        if (Input.GetKeyDown(KeyCode.Space))
            SaveGesture();
        //! TMP
        
        // Query all joints in the hand.
        if (handsSubsystem == null || !handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
            return;
        
        Gesture currentGesture = Recognize(joints);
        bool hasRecognised = !currentGesture.Equals(new());
        
        if (hasRecognised && !currentGesture.Equals(previousGesture))
        {
            Debug.Log($"Gesture recognized: {currentGesture.name}");
            currentGesture.onRecognized?.Invoke();
            previousGesture = currentGesture;
        }
    }

    private Gesture Recognize(IReadOnlyList<HandJointPose> joints)
    {
        Gesture currentGesture = new ();
        float currentMin = Mathf.Infinity;
        foreach (var gesture in gestureList)
        {
            float sumDistance = 0;
            bool isDiscarted = false;
            for (int i = 0; i < fingerCount; i++)
            {
                Vector3 currentData = HandTransform.InverseTransformPoint(joints[i].Position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if (distance > threshold)
                {
                    isDiscarted = true;
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarted && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }

    public void PointingPing()
    {
        Ray ray = new Ray(HandTransform.position, cursor.position - HandTransform.position);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.collider.gameObject.CompareTag("Board"))
            holoPlayerManager.Ping(hit.point);
    }
    
    public void SaveGesture()
    {
        Debug.Log("Try get new gesture"); 
        if (!handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
            return;
        List<Vector3> data = 
            joints.Select(j => HandTransform.InverseTransformPoint(j.Position)).ToList();

        gestureList.Add(new Gesture {name = "NewGesture", fingerDatas = data});
        Debug.Log("New gesture saved");
    }
}
