using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class roomManager : MonoBehaviourPunCallbacks {

    public bool DebugLogRoomProperties = false;

    public static roomManager Self;

    protected string _roomName = "";

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

    public bool RoomIsVisible
    {
        get
        {
            return PhotonNetwork.CurrentRoom.IsVisible;
        }
        set
        {
            PhotonNetwork.CurrentRoom.IsVisible = value;
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
        if(_roomName != "")
        {
            JoinRoom(_roomName);
        }
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
        PhotonNetwork.LoadLevel(gameSceneBuildIndex);
        //SceneManager.LoadScene(gameSceneBuildIndex);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Room left");

        _roomProperties = null;
        isConnected = false;
        TypedLobby lobby = PhotonNetwork.CurrentLobby;
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby(lobby);
    }

    public void JoinRoom(string room)
    {
        _roomName = room;
        PhotonNetwork.JoinRoom(_roomName);
    }

    public void CreateRoom(string room, int levelBuildIndex)
    {
        gameSceneBuildIndex = levelBuildIndex;
        _roomName = room;
        PhotonNetwork.CreateRoom(_roomName, null, PhotonNetwork.CurrentLobby, null);
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
        SpawnPoint.ClearSpawnPointList();
        PhotonNetwork.LoadLevel("LobbyScene");
        //SceneManager.LoadScene("LobbyScene");
    }

    public void ForceRefreshRoomList()
    {
        PhotonNetwork.GetCustomRoomList(PhotonNetwork.CurrentLobby, null);
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
