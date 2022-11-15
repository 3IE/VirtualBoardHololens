using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shapes
{
    public class ShapeSelector : MonoBehaviour
    {
        internal const byte CubeId        = 0;
        internal const byte CylinderId    = 1;
        internal const byte SphereId      = 2;
        private const  byte CustomShapeId = 3; // Find a way to determine id based on obj file

        /// <summary>
        ///     shape currently held by the player
        /// </summary>
        public Shape currentShape;

        [SerializeField] private InputActionReference createReference;
        [SerializeField] private InputActionReference destroyReference;
        [SerializeField] private InputActionReference changeDistance;

        [SerializeField] private Transform        shapesParent;
        [SerializeField] private List<GameObject> shapes;

        //[Range(0.5f, 5f)] [SerializeField] private float velocity = 0.5f;

        [SerializeField] private Material testMaterial;

        private bool _creating;

        private byte _index;

        /// <summary>
        ///     Instance of the class
        /// </summary>
        public static ShapeSelector Instance { get; private set; }

        private void Awake()
        {
            //createReference.action.started  += CreateObject;
            //createReference.action.canceled += StopCreateObject;

            //destroyReference.action.started  += DeleteObject;
            //destroyReference.action.canceled += StopDeleteObject;

            //changeDistance.action.performed += ChangeDistance;

            Instance = this;
        }

        private void Start()
        {
            _index = CubeId;
        }

        private void CreateObject(InputAction.CallbackContext ctx)
        {
            if (Shape.NumberOfShapes() >= 25)
                return;

            if (currentShape is not null && !currentShape.Resizing)
            {
                currentShape.Rotating = true;
                currentShape.Moving   = false;
                return;
            }

            _creating = true;

            CreateObject(_index, false);
        }

        public Shape CreateObject(byte id, bool received)
        {
            GameObject prefab = GetShape(id);

            GameObject obj;

            if (id >= CustomShapeId)
            {
                obj = prefab
                    ? prefab
                    : throw new ArgumentNullException(nameof(prefab));

                obj.transform.SetParent(shapesParent);
            }
            else
                obj = Instantiate(prefab, shapesParent);

            currentShape = obj.GetComponent<Shape>();

            if (!received)
                currentShape.CreateAction();

            return currentShape;
        }

        private void StopCreateObject(InputAction.CallbackContext ctx)
        {
            if (currentShape is null)
                return;

            if (currentShape.Rotating)
            {
                currentShape.OnDeselect();
                return;
            }

            _creating = false;

            currentShape.StopCreateAction();
            currentShape = null;
        }

        private void DeleteObject(InputAction.CallbackContext ctx)
        {
            if (_creating)
                return;

            Shape.DeletionMode(true);

            //TODO: find a way to know which shape is selected

            /*
            if (Physics.Raycast(leftInteractor.transform.position, leftInteractor.transform.forward,
                                out RaycastHit hit, 100f, LayerMask.GetMask("Static Shapes")))
                hit.collider.GetComponent<Shape>().Delete();
            */
        }

        private void StopDeleteObject(InputAction.CallbackContext ctx)
        {
            if (_creating)
                return;

            Shape.DeletionMode(false);

            //TODO: find a way to know which shape is selected

            /*
            if (Physics.Raycast(leftInteractor.transform.position, leftInteractor.transform.forward,
                                out RaycastHit hit, 100f, LayerMask.GetMask("Static Shapes")))
                hit.collider.GetComponent<Shape>().CallDestroy(true);
            */
        }

        private void ChangeDistance(InputAction.CallbackContext obj)
        {
            if (currentShape is null)
                return;

            //float value = obj.ReadValue<Vector2>().y * Time.deltaTime;

            //currentShape.InitialDistance =
            //    Mathf.Clamp(currentShape.InitialDistance + value * velocity, 0.5f, 100f);
        }

        #region SELECTOR

        /// <summary>
        ///     Sets the currently spawnable shape to be a cube
        /// </summary>
        public void SelectCube()
        {
            _index = CubeId;
        }

        /// <summary>
        ///     Sets the currently spawnable shape to be a cylinder
        /// </summary>
        public void SelectCylinder()
        {
            _index = CylinderId;
        }

        /// <summary>
        ///     Sets the currently spawnable shape to be a sphere
        /// </summary>
        public void SelectSphere()
        {
            _index = SphereId;
        }

        /// <summary>
        ///     Selects a custom shape.
        /// </summary>
        /// TODO: Find a way to select custom shape via UI
        /// TODO: Find a way to determine id based on obj file (load at start and attributes at that time?)
        public void SelectCustomShape()
        {
            _index = CustomShapeId;
        }

        /// <summary>
        ///     Returns the shape corresponding to <c>shapeId</c>
        /// </summary>
        /// <param name="shapeId"> id of the shape we are searching for </param>
        /// <returns> corresponding shape </returns>
        private GameObject GetShape(byte shapeId)
        {
            return shapeId < CustomShapeId ? shapes[shapeId] : CustomShape.Create(shapeId, testMaterial);
        }

        /// <summary>
        ///     Returns the shape corresponding to <see cref="_index" />
        /// </summary>
        /// <returns> corresponding shape </returns>
        private GameObject GetShape()
        {
            return GetShape(_index);
        }

        #endregion
    }
}
