using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Subsystems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
    public float Cooldown; //? Cooldown before the gesture can be recognized again ?
    public bool isOnCooldown;
}
public class HandGestureDetection : MonoBehaviour
{
    [SerializeField] private HoloPlayerManager holoPlayerManager;
    
    [SerializeField] private Transform cursor;
    public Transform HandTransform;
    [SerializeField] private Transform thumbUpHeart;
    
    private HandsSubsystem handsSubsystem;
    //private MRTKRayInteractor rayInteractor;
    
    public XRNode handNode;
    private const int fingerCount = 26;
    
    public float threshold = 0.1f;
    public List<Gesture> gestureList;
    private Gesture previousGesture;

    private void Awake()
    {
        //rayInteractor = GetComponentInChildren<MRTKRayInteractor>();
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
        
        if (hasRecognised && !currentGesture.Equals(previousGesture) && !currentGesture.isOnCooldown)
        {
            Debug.Log($"Gesture recognized: {currentGesture.name}");
            currentGesture.onRecognized?.Invoke();
            previousGesture = currentGesture;
            StartCoroutine(SetOnCooldown(currentGesture));
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

    private IEnumerator SetOnCooldown(Gesture gesture)
    {
        gesture.isOnCooldown = true;
        yield return new WaitForSeconds(gesture.Cooldown);
        gesture.isOnCooldown = false;
    }
    
    public void Pointing()
    {
        if (!handsSubsystem.TryGetJoint(TrackedHandJoint.IndexTip, handNode, out HandJointPose indexTip)
            || !handsSubsystem.TryGetJoint(TrackedHandJoint.IndexProximal, handNode, out HandJointPose indexBase))
            return;
        Ray ray = new Ray(indexBase.Position, indexTip.Position - indexBase.Position);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (!hit.collider.gameObject.CompareTag("Board")) return;
        // move cursor to hit position
        cursor.position = hit.point;
    }
    
    public void PointingPing()
    {
        if (!cursor.gameObject.activeSelf) // if cursor not already set
            Pointing();
        holoPlayerManager.Ping(cursor.position);
    }
    
    public void ThumbUp()
    {
        //todo holoPlayerManager.ThumbUp();
        thumbUpHeart.position = handsSubsystem.TryGetJoint(TrackedHandJoint.ThumbTip, handNode, out HandJointPose thumbTipPose) ? thumbTipPose.Position : HandTransform.position;
        thumbUpHeart.gameObject.SetActive(true);
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
