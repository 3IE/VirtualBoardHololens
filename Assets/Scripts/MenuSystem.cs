using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour
{
    [SerializeField] private List<Transform> panels = new ();
    [SerializeField] private List<Transform> subPanels = new ();
    private PanelIndex _activePanelIndex = PanelIndex.Menu;
    private MenuIndex _activeSubpanelIndex = MenuIndex.Setup;
    
    [FormerlySerializedAs("_errorText")] [SerializeField] private TMP_Text errorText;
    [FormerlySerializedAs("_postItInputField")] [SerializeField] private TMP_InputField postItInputField;
    [SerializeField] private Slider boardDistanceSlider;
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
        foreach (Transform tr in panels[(int)PanelIndex.Menu].transform) subPanels.Add(tr);
    }

    private void SwitchPanel(PanelIndex index)
    {
        panels[(int)_activePanelIndex].gameObject.SetActive(false);
        _activePanelIndex = index;
        panels[(int)_activePanelIndex].gameObject.SetActive(true);
    }
    public void SwitchSubpanel(MenuIndex index)
    {
        SwitchPanel(PanelIndex.Menu);
        subPanels[(int)_activeSubpanelIndex].gameObject.SetActive(false);
        _activeSubpanelIndex = index;
        subPanels[(int)_activeSubpanelIndex].gameObject.SetActive(true);
    }
    public void ARSwitchPanel()
    {
        SwitchPanel(PanelIndex.GameUI);
        appManager.ARSession.SetActive(true);
    }
    public void ARSetup(bool Start)
    {
        SwitchPanel(Start ? PanelIndex.SetupUI : PanelIndex.Menu);
        appManager.ARSetup(Start);
    }
    public void PrintError(string errorText)
    {
        this.errorText.text = errorText;
        Debug.Log($"Error: {errorText}");
    }
    public IEnumerator Post_it_Prompt()
    {
        SwitchSubpanel(MenuIndex.PostItPrompt);

        while (!_buttonPost && !_buttonPostCancel)
            yield return null;
        
        if (_buttonPostCancel || postItInputField.text == "")
            postItText = string.Empty;
        else // _buttonPost == true
            postItText = postItInputField.text;
        
        // reset
        _buttonPost = _buttonPostCancel = false;
        postItInputField.text = string.Empty;
        
        ARSwitchPanel();
    }

    public void Reset() { //TODO need to disconnect from server as well here
        appManager.StopSession();
        SwitchSubpanel(MenuIndex.Join);
    }

    public void SlideBoard() => appManager.SlideBoard(boardDistanceSlider.value);
    public enum PanelIndex
    {
        Menu = 0,
        GameUI,
        SetupUI
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
