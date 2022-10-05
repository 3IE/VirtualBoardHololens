using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbUpHeart : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _maxHeight = 1.5f;

    private void Awake() => cam = Camera.main;

    private void OnEnable()
    {
        GetComponentInChildren<Transform>().LookAt(cam.transform);
        StartCoroutine(Ascend());
    }
    
    private IEnumerator Ascend()
    {
        // ascend using lerp
        float t = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * _maxHeight;
        while (t < 1)
        {
            t += Time.deltaTime * _speed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
