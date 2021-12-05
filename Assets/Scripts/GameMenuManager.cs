using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    #region Private Fields



    #endregion

    #region Public Methods

    public void OnClickedToolsButton()
    {
        Debug.Log("clicked tools");
    }

    public void OnClickedSaveButton()
    {
        Debug.Log("clicked save");
    }

    public void OnClickedSettingsButton()
    {
        Debug.Log("clicked settings");
    }

    public void OnClickedExitButton()
    {
        Debug.Log("clicked exit");
    }

    #endregion

    #region MonoBehaviour Callbacks

    void Start()
    {
        
    }

    #endregion
}
