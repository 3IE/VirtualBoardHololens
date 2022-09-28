using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuSystem : MonoBehaviour
{
    [SerializeField] private List<Transform> panels = new ();
    private MenuIndex _activePanelIndex = MenuIndex.Setup;
    
    [FormerlySerializedAs("_postItInputField")] [SerializeField] private TMP_InputField postItInputField;
    [FormerlySerializedAs("_postItText")] public string postItText;

    #region buttonBool

    public bool ButtonSetup { get; set; }
    public bool ButtonSetupCancel { get; set; }
    private bool _buttonPost;
    private bool _buttonPostCancel;

    // Setters used by menu buttons
    public bool ButtonPost { set => _buttonPost = value; }
    public bool ButtonPostCancel { set => _buttonPostCancel = value; }

    #endregion
    
    public AppManager appManager;
    
    private void Start()
    {
        foreach (Transform tr in gameObject.transform) panels.Add(tr);
    }

    public void TurnOffMenu() => panels[(int)_activePanelIndex].gameObject.SetActive(false);
    public void SwitchPanel(int index) => SwitchPanel((MenuIndex) index);
    public void SwitchPanel(MenuIndex index)
    {
        panels[(int)_activePanelIndex].gameObject.SetActive(false);
        _activePanelIndex = index;
        panels[(int)_activePanelIndex].gameObject.SetActive(true);
    }
    public IEnumerator Post_it_Prompt()
    {
        SwitchPanel(MenuIndex.PostItPrompt);

        while (!_buttonPost && !_buttonPostCancel)
            yield return null;
        
        if (_buttonPostCancel || postItInputField.text == "")
            postItText = string.Empty;
        else // _buttonPost == true
            postItText = postItInputField.text;
        
        // reset
        _buttonPost = _buttonPostCancel = false;
        postItInputField.text = string.Empty;
        
        TurnOffMenu();
    }

    public void Reset() { //TODO need to disconnect from server as well here
        appManager.StopSession();
        SwitchPanel(MenuIndex.Join);
    }
    
    public enum MenuIndex
    {
        Join = 0,
        Joining,
        Disconnected,
        PostItPrompt,
        Option,
        Setup
    }
}
