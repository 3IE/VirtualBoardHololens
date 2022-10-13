using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
///     Base class for the tools writing on the board
/// </summary>
public class WritingTool : MonoBehaviour
{
    /// <summary>
    ///     Size used to know the number of pixels we should modify
    /// </summary>
    [SerializeField] protected float penSize;

    private Vector3  _initialPosition;
    private Vector3  _lastPosition;

    /// <summary>
    ///     Boolean used to know if the tool touched the board last frame
    /// </summary>
    protected bool TouchedLast;
}
