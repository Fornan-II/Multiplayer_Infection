using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyUpButton : MonoBehaviour {

    public GameManager myGameManager;
    public Text buttonText;

    public string NotReadyString = "Ready Up";
    public string ReadyString = "Unready";

    protected bool _playerReady = false;

    private void Start()
    {
        SyncButtonText();
    }

    private void Update()
    {
        if(myGameManager.GameIsRunning)
        {
            gameObject.SetActive(false);
        }
    }

    public void PlayerReady()
    {
        _playerReady = !_playerReady;

        SyncButtonText();
        myGameManager.SetPlayerReady(_playerReady);
    }

    private void SyncButtonText()
    {
        if (_playerReady)
        {
            buttonText.text = ReadyString;
        }
        else
        {
            buttonText.text = NotReadyString;
        }
    }
}
