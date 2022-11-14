using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UI
{
    /// <summary>
    ///     Menu used to display information for the user
    /// </summary>
    public class VRMenu : MonoBehaviour
    {
        // List of MenuPanels with their respective buttons
        [SerializeField] private List<GameObject> panels;
        [SerializeField] private List<GameObject> panelsButtons;

        private PanelIndex  _activePanelIndex = PanelIndex.PlayerList;
        private CanvasGroup _canvasGroup;
        private ObjectManipulator _objectManipulator;
        
        [SerializeField] private GameObject postItPrefab;
        
        //[SerializeField] private float throwThreshold;
        //[SerializeField] private float timeToFade;
        //private float velocity;
        //private Vector3 lastPosition;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _objectManipulator = GetComponent<ObjectManipulator>();
        }

        #region throwAway

        /// <summary>
        ///     If the menu is thrown with at sufficient speed (>throwThreshold)
        ///     it will fade out and disable itself
        /// </summary>
        //public void ThrowAway()
        //{
        //    StartCoroutine(ThrowAwayRoutine());
        //}

        //private IEnumerator ThrowAwayRoutine()
        //{
        //    if (velocity < throwThreshold) yield break;
        //    _objectManipulator.enabled = false;
        //    
        //    var i = 0.0f;
//
        //    while (i < timeToFade)
        //    {
        //        _canvasGroup.alpha =  Mathf.Lerp(1f, 0f, i / timeToFade);
        //        i                  += Time.fixedDeltaTime;
//
        //        yield return null;
        //    }
        //}
        
        //private void Update()
        //{
        //    velocity = (transform.position - lastPosition).magnitude / Time.fixedDeltaTime;
        //    lastPosition = transform.position;
        //    PrintVar.print(9, $"Velocity: {velocity}");
        //}
        
        #endregion

        /// <summary>
        ///     Opens the menu, reset its inertia
        /// </summary>
        public void Open()
        {
            StopAllCoroutines();

            _objectManipulator.enabled = true;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1;
        }

        /// <summary>
        ///     Close the menu
        /// </summary>
        public void Close()
        {
            StopAllCoroutines();
            _objectManipulator.enabled = false;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        
        /// <summary>
        ///     Generates a Post-it and opens it
        /// </summary>
        public void Post_it()
        {
            Instantiate(postItPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }

        /// <summary>
        ///     Switches the active panel to the one specified by the index
        /// </summary>
        /// <param name="index"></param>
        public void SwitchPanel(int index)
        {
            SwitchPanel((PanelIndex) index);
        }

        private void SwitchPanel(PanelIndex index)
        {
            panels[(int) _activePanelIndex].SetActive(false);
            panelsButtons[(int) _activePanelIndex].SetActive(true);

            _activePanelIndex = index;

            panels[(int) _activePanelIndex].SetActive(true);
            panelsButtons[(int) _activePanelIndex].SetActive(false);
        }

        private enum PanelIndex
        {
            PlayerList = 0,
            PropsList,
            ToolList,
        }
    }
}