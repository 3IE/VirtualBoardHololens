﻿using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using EventCode = Utils.EventCode;

namespace Shapes
{
    // TODO: Find a way to know which action is used on the shape
    public abstract class Shape : MonoBehaviour
    {
        private static int _defaultLayer;
        private static int _shapesLayer;

        private static readonly int Create1  = Shader.PropertyToID("_Creating");
        private static readonly int Modify1  = Shader.PropertyToID("_Modifying");
        private static readonly int Destroy1 = Shader.PropertyToID("_Destroying");

        /// <summary>
        ///     Holds a list of the existing shapes with their <see cref="_id" /> as a key
        /// </summary>
        protected static readonly Dictionary<int, Shape> Shapes = new();

        private static Transform _boardTransform;

        private bool _created;
        private bool _deleting;

        private int _id;

        private Vector3 _initialScale;
        private bool    _isOwner;

        private bool _locked;

        private Rigidbody _rigidbody;

        protected int DefaultMask;
        protected int DefaultPlayerMask;

        protected RaycastHit[] Hits;
        internal  float        InitialDistance;

        /// <summary>
        ///     Material of the object
        /// </summary>
        protected Material Mat;

        internal bool Moving;
        internal bool Resizing;

        internal bool Rotating;

        /// <summary>
        ///     Id corresponding to the type of this shape
        /// </summary>
        protected internal byte ShapeId;

        protected Transform Transform;

        public static int Counter { get; private set; }

        /// <summary>
        ///     Returns the number of shapes currently existing
        /// </summary>
        /// <returns> number of shapes </returns>
        public static int NumberOfShapes()
        {
            return Shapes.Count;
        }

        #region Unity

        private void Awake()
        {
            _rigidbody    = GetComponent<Rigidbody>();
            _defaultLayer = LayerMask.NameToLayer("Static Shapes");
            _shapesLayer  = LayerMask.NameToLayer("Shapes");
            _id           = Counter++;
            Transform     = transform;

            Hits              = new RaycastHit[20];
            Mat               = GetComponent<Renderer>().material;
            DefaultMask       = LayerMask.GetMask("Default", "Static Shapes");
            DefaultPlayerMask = LayerMask.GetMask("Default", "Static Shapes");

            Shapes.Add(_id, this);

            Freeze();

            _created = true;

            _boardTransform ??= Board.Board.Instance.transform;
        }

        private void Update()
        {
            if (Moving)
                Move();
            else if (Rotating)
                Rotate();
            else if (Resizing)
                Resize();
        }

        #endregion

        #region Material

        private void Create()
        {
            Mat.SetFloat(Create1, 1);
        }

        private void Modify()
        {
            Mat.SetFloat(Modify1, 1);
        }

        internal void Delete()
        {
            Mat.SetFloat(Destroy1, 1);
        }

        internal void CallDestroy(bool sendSignal)
        {
            Shapes.Remove(_id);

            if (sendSignal)
                SendDestroy();

            Destroy(gameObject);
        }

        private void StopAction()
        {
            Mat.SetFloat(Create1,  0);
            Mat.SetFloat(Modify1,  0);
            Mat.SetFloat(Destroy1, 0);
        }

        internal void CreateAction()
        {
            StopAction();
            Create();

            _created = false;
        }

        internal void StopCreateAction()
        {
            StopAction();
            _created = true;

            SendNewObject();
        }

        #endregion

        #region Selection

        /// <summary>
        ///     Is called when an interactor hovers over this shape
        /// </summary>
        /// TODO: Add a way to know if the shape is hovered or not
        public void OnHoverEnter()
        {
            if (_deleting)
                Delete();
        }

        /// <summary>
        ///     Is called when an interactor stops hovering over this shape
        /// </summary>
        public void OnHoverExit()
        {
            if (_deleting)
                Mat.SetFloat(Destroy1, 0);
        }

        /// <summary>
        ///     Is called when an interactor selects this shape
        /// </summary>
        public void OnSelect()
        {
            if (_deleting)
                return;

            if (ShapeSelector.Instance.currentShape is not null
                && !ReferenceEquals(ShapeSelector.Instance.currentShape, this))
                return;

            if (!_isOwner)
            {
                if (_locked)
                    return;

                ShapeSelector.Instance.currentShape = this;

                _isOwner = true;
                _locked  = true;

                SendOwnership();
            }

            Modify();

            UpdateAction();
        }

        /// <summary>
        ///     Is called when an interactor lets go of this shape
        /// </summary>
        public void OnDeselect()
        {
            if (_deleting)
                return;

            ShapeSelector.Instance.currentShape = null;

            UpdateActionDeselect();
            SendOwnership();
        }

        private void UpdateActionDeselect()
        {
            Moving   = false;
            Rotating = false;
            Resizing = false;
            _isOwner = false;
            _locked  = false;

            gameObject.layer = _defaultLayer;

            StopAction();
            Freeze();
        }

        private void UpdateAction()
        {
            Moving   = true;
            Resizing = false; // TODO

            Unfreeze();

            if (Moving)
                gameObject.layer = _shapesLayer;

            if (!Resizing) return;

            InitialDistance = Vector3.Distance(Vector3.zero, Vector3.one); // TODO: Replace with the distance between the two fingers selecting the shape

            if (InitialDistance == 0)
                InitialDistance = 1;

            _initialScale = transform.localScale;
        }

        private void Freeze()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void Unfreeze()
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
        }

        internal static void DeletionMode(bool value)
        {
            foreach (Shape shape in Shapes.Values)
                shape._deleting = value;
        }

        #endregion

        #region Networking

        /// <summary>
        ///     Sends a signal to create an object with these parameters
        /// </summary>
        private void SendNewObject()
        {
            Debug.Log("Sending created object");

            var data = new object[] { Transform.position - _boardTransform.position, Transform.rotation, ShapeId };

            SendData(EventCode.SendNewObject, data);
        }

        /// <summary>
        ///     Receives a signal to create an object with these parameters
        /// </summary>
        /// <param name="data"> the transform and id of the object </param>
        public static void ReceiveNewObject(object[] data)
        {
            Debug.Log("Receiving created object");

            var position = (Vector3) data[0];
            var rotation = (Quaternion) data[1];
            var shapeId  = (byte) data[2];

            Shape shape = ShapeSelector.Instance.CreateObject(shapeId, true);
            shape.transform.position = position + _boardTransform.position;
            shape.transform.rotation = rotation;

            shape._locked  = true;
            shape._isOwner = true;
            shape._created = true;
        }

        /// <summary>
        ///     Sends the new transform of the object to the other clients
        /// </summary>
        private void SendTransform()
        {
            if (!_isOwner || !_locked || !_created) return;

            object[] data = { Transform.position - _boardTransform.position, Transform.rotation, Transform.localScale, _id };
            SendData(EventCode.SendTransform, data);
        }

        /// <summary>
        ///     Gets the new transform of the object from the server and applies it to the object.
        /// </summary>
        public static void ReceiveTransform(object[] data)
        {
            Vector3 position = (Vector3) data[0] + _boardTransform.position;
            var     rotation = (Quaternion) data[1];
            var     scale    = (Vector3) data[2];
            var     id       = (int) data[3];

            Transform shape = Shapes[id].transform;

            shape.position   = position;
            shape.rotation   = rotation;
            shape.localScale = scale;
        }

        /// <summary>
        ///     Tells the other clients to destroy this object
        /// </summary>
        private void SendDestroy()
        {
            Debug.Log("Sending destroy");

            SendData(EventCode.SendDestroy, _id);
        }

        /// <summary>
        ///     Destroy the object with the corresponding id
        /// </summary>
        public static void ReceiveDestroy(int id)
        {
            Shapes[id].CallDestroy(false);
        }

        /// <summary>
        ///     Warns the other clients that this object can't be modified
        /// </summary>
        private void SendOwnership()
        {
            object[] data = { _isOwner, _id };
            SendData(EventCode.SendOwnership, data);
        }

        /// <summary>
        ///     Update this object so that it can't be modified until told otherwise
        /// </summary>
        public static void ReceiveOwnership(object[] data)
        {
            var owned = (bool) data[0];
            var id    = (int) data[1];

            Shapes[id]._locked = owned;
        }

        /// <summary>
        ///     Sets the counter for the creation of new objects
        /// </summary>
        public static void ReceiveCounter(int data)
        {
            Counter = data;
        }

        private static void SendData(EventCode eventCode, object data = null)
        {
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.RaiseEvent((byte) eventCode, data, raiseEventOptions,
                                     SendOptions.SendReliable);
        }

        #endregion

        #region Physics

        /// <summary>
        ///     Moves the object along the ray of the controller selecting it
        /// </summary>
        /// TODO: Make it so that the object can move with the fingers or through a button
        private void Move()
        {
            SendTransform();

            /*
            if (Physics.Raycast(Interactors[0].transform.position, Interactors[0].transform.forward,
                                out RaycastHit hit, InitialDistance, DefaultMask))
            {
                Vector3 position = GetPositionFromHit(hit);

                if (!CheckForCollision(position))
                {
                    Transform.position = position;
                    InitialDistance    = hit.distance;
                    return;
                }
            }

            int size = CheckCast();

            var positionFound = false;

            for (int i = size - 1; i >= 0 && !positionFound; i--)
            {
                Vector3 position = GetPositionFromHit(Hits[i]);

                if (CheckForCollision(position))
                    continue;

                Transform.position = position;

                //initialDistance = hit.distance;
                positionFound = true;
            }

            if (!positionFound)
            {
                Transform.position = Interactors[0].transform.position
                                     + Interactors[0].transform.forward * InitialDistance;
            }
            */

            SendTransform();
        }


        /// <summary>
        ///     Resizes the object according to the distance between the two controllers
        /// </summary>
        /// TODO: Make it so that the object can be resized with the fingers or through a button
        private void Resize()
        {
            throw new NotImplementedException();

            /*
            Transform.localScale =
                _initialScale
                / InitialDistance
                * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);

            UpdateSize();

            SendTransform();
            */
        }

        /// <summary>
        ///     Rotates the object according to the angle of the current controller
        /// </summary>
        /// TODO: Make it so that the object move with the fingers or through a button
        protected virtual void Rotate()
        {
            throw new NotImplementedException();

            //SendTransform();
        }

        #endregion

        #region Abstract

        /// <summary>
        ///     Checks if the object is still valid at a specific position
        /// </summary>
        protected abstract bool CheckForCollision(Vector3 position);

        /// <summary>
        ///     Applies a raycast corresponding to the shape of the object
        /// </summary>
        /// <returns> The number of hits </returns>
        protected abstract int CheckCast();

        /// <summary>
        ///     Gets the position of the object from the hit
        /// </summary>
        /// <param name="hit"> point the raycast touched </param>
        /// <returns> the position of the object </returns>
        protected abstract Vector3 GetPositionFromHit(RaycastHit hit);

        /// <summary>
        ///     Updates the size of the object
        /// </summary>
        protected abstract void UpdateSize();

        #endregion
    }
}
