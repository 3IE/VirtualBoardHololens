using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Shapes;
using UnityEngine;

public class PostItObject : Shape
{
    private void Start()
    {
        GetComponent<ObjectManipulator>().enabled = true;
    }

    public void OnEndEdit()
    {
        
    }

    protected override bool CheckForCollision(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    protected override int CheckCast()
    {
        throw new System.NotImplementedException();
    }

    protected override Vector3 GetPositionFromHit(RaycastHit hit)
    {
        throw new System.NotImplementedException();
    }

    protected override void UpdateSize()
    {
        throw new System.NotImplementedException();
    }
}
