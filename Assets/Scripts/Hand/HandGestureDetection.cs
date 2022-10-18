using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
    public float Cooldown; //? Cooldown before the gesture can be recognized again ?
    public bool isOnCooldown;
    public float threshold;
}
public class HandGestureDetection : MonoBehaviour
{
    [SerializeField] private HoloPlayerManager holoPlayerManager;
    
    [SerializeField] private Transform cursor;
    private LineRenderer lineRenderer;
    private Transform HandTransform;
    [SerializeField] private Transform thumbUpHeart;
    
    public XRNode handNode;
    [SerializeField] private float indexThreshold;
    [SerializeField] private float alignThreshold;
    
    private HandsSubsystem handsSubsystem;
    private Transform headPosition;
    private const int fingerCount = 26;
    private bool isPointing;
    
    public List<Gesture> gestureList;
    private Gesture previousGesture;
    [Header("RayConfig")]
    [SerializeField] private float rayWidth = 0.0005f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        
        if (HandTransform == null)
            HandTransform = transform;
        previousGesture = new Gesture();
        
        headPosition = Camera.main ? Camera.main.transform : throw new Exception("No camera found ?!");
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
#if UNITY_EDITOR
        //Allow to make more gestures, once your gestures are saved you can configure them in the inspector
        if (Input.GetKeyDown(KeyCode.Space))
            SaveGesture();
#endif
        // Query all joints in the hand.
        bool tryGetEntireHand = handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints);
        //PrintVar.print(0, $"tryGetEntireHand: {tryGetEntireHand}", $"joints? {joints == null}");
        
        //if (joints != null) //! TMP
        //    PrintVar.print(1, $"joints: {joints.Count}"
        //        , $"IndexExtended: {IndexExtended(joints)}"
        //        , $"IndexTip: {joints[(int)TrackedHandJoint.IndexTip].Position}"
        //        , $"IndexPrx: {joints[(int)TrackedHandJoint.IndexProximal].Position}"
        //        , $"Extention: {joints[(int)TrackedHandJoint.IndexProximal].Position - joints[(int)TrackedHandJoint.IndexTip].Position}"
        //        , $"Distance: {Vector3.Distance(joints[(int)TrackedHandJoint.IndexProximal].Position, joints[(int)TrackedHandJoint.IndexTip].Position)}"
        //        , $"Angle: {Vector3.Angle(joints[(int)TrackedHandJoint.IndexTip].Position - joints[(int)TrackedHandJoint.IndexProximal].Position, headPosition.position - joints[(int)TrackedHandJoint.IndexProximal].Position)}");
        
        if (handsSubsystem == null || !tryGetEntireHand || joints == null)
            return; // joints[(int)HandJointKind.IndexTip] to get the position of the index finger tip from it
        if (IndexExtended(joints))
        {
            var start = IndexHeadAligned(joints) ? headPosition.position : joints[(int)TrackedHandJoint.IndexProximal].Position;
            var end = IndexHeadAligned(joints) ? joints[(int)TrackedHandJoint.IndexProximal].Position : joints[(int)TrackedHandJoint.IndexTip].Position;
            if (!Pointing(start, end)) return;
            
            lineRenderer.SetPosition(0, joints[(int)TrackedHandJoint.IndexProximal].Position);
            lineRenderer.SetPosition(1, cursor.position);
            
            lineRenderer.enabled = true;
        }
        else
            lineRenderer.enabled = false;
        
        Gesture currentGesture = Recognize(joints);
        bool hasRecognised = !currentGesture.Equals(new Gesture());
        
        if (hasRecognised) // && !currentGesture.Equals(previousGesture))
        {
            Debug.Log($"Gesture recognized: {currentGesture.name}");
            currentGesture.onRecognized?.Invoke();
            previousGesture = currentGesture;
            StartCoroutine(SetOnCooldown(currentGesture));
        }
    }
    
    private bool IndexExtended(IReadOnlyList<HandJointPose> joints)
        => Vector3.Distance(joints[(int)TrackedHandJoint.IndexProximal].Position, 
               joints[(int)TrackedHandJoint.IndexTip].Position) > indexThreshold;
    
    private bool IndexHeadAligned(IReadOnlyList<HandJointPose> joints)
    {
        var IndexTip = joints[(int)TrackedHandJoint.IndexTip].Position;
        var IndexProx = joints[(int)TrackedHandJoint.IndexProximal].Position;
        return Vector3.Angle(IndexTip - headPosition.position, IndexProx - headPosition.position) > alignThreshold;
    }
    
    private Gesture Recognize(IReadOnlyList<HandJointPose> joints)
    {
        Gesture currentGesture = new ();
        float currentMin = Mathf.Infinity;
        foreach (var gesture in gestureList)
        {
            if (gesture.isOnCooldown)
                continue;
            float sumDistance = 0;
            bool isDiscarted = false;
            for (int i = 0; i < fingerCount; i++)
            {
                Vector3 currentData = HandTransform.InverseTransformPoint(joints[i].Position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if (distance > gesture.threshold)
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

    public bool Pointing(Vector3 start, Vector3 end)
    {
        Ray ray = new Ray(start, end - start);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return false;
        if (!hit.collider.gameObject.CompareTag("Board")) return false;
        // move cursor to hit position
        cursor.position = hit.point;
        return true;
    }

    public void PointingPing()
    {
        //if (!cursor.gameObject.activeSelf) // if cursor not already set
        //    Pointing();
        holoPlayerManager.Ping(cursor.position);
    }
    
    public void ThumbUp()
    {
        //todo holoPlayerManager.ThumbUp();
        thumbUpHeart.position = handsSubsystem.TryGetJoint(TrackedHandJoint.ThumbTip, handNode, out HandJointPose thumbTipPose) ? thumbTipPose.Position : HandTransform.position;
        thumbUpHeart.gameObject.SetActive(true);
    }
#if UNITY_EDITOR
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
#endif
}
