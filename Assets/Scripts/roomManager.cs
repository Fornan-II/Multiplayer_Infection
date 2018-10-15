﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class roomManager : MonoBehaviourPunCallbacks {

    public bool DebugLogRoomProperties = false;

    public static roomManager Self;

    public string roomName = "defaultRoom";

    public int gameSceneBuildIndex = 0;

    protected ExitGames.Client.Photon.Hashtable _roomProperties;
    public ExitGames.Client.Photon.Hashtable RoomProperties
    {
        get
        {
            return _roomProperties;
        }
        set
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(value);
        }
    }

    public bool RoomCanBeJoined
    {
        get
        {
            return PhotonNetwork.CurrentRoom.IsOpen;
        }
        set
        {
            PhotonNetwork.CurrentRoom.IsOpen = value;
        }
    }

    protected List<RoomInfo> _roomList;
    public List<RoomInfo> RoomList { get { return _roomList; } }

    public bool isConnected = false;

    private void Awake()
    {
        //Making this class a singleton
        if(Self)
        {
            Destroy(gameObject);
        }
        else
        {
            Self = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Starting connection");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Master connected to.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room joined");
        isConnected = true;

        _roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        object sceneIndex;
        if(_roomProperties.TryGetValue("int_levelBuildIndex", out sceneIndex))
        {
            gameSceneBuildIndex = (int)sceneIndex;
        }
        SceneManager.LoadScene(gameSceneBuildIndex);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Room left");
        _roomProperties = null;
        isConnected = false;
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        //Load level that room tells us to load
    }

    public void CreateRoom(string roomName, int levelBuildIndex)
    {
        gameSceneBuildIndex = levelBuildIndex;
        PhotonNetwork.CreateRoom(roomName, null, PhotonNetwork.CurrentLobby, null);
    }

    public override void OnCreatedRoom()
    {
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "int_levelBuildIndex", gameSceneBuildIndex }
            };
        RoomProperties = newProperties;
    }

    public void LeaveRoom()
    {
        if(!isConnected)
        {
            Debug.LogWarning("Can not leave room when there is no room connected to.");
            return;
        }

        Debug.Log("Leaving room...");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        _roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if(DebugLogRoomProperties) Debug.Log("Room properties updating to " + _roomProperties.ToStringFull());
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        _roomList = roomList;
    }
}
