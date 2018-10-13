using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class roomManager : MonoBehaviourPunCallbacks {

    public static roomManager Self;

    public string roomName = "defaultRoom";

    public GameObject humanPrefab;
    public GameObject zombiePrefab;

    public PlayerController playerController;

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

    public enum PlayerType
    {
        SPECTATOR,
        HUMAN,
        ZOMBIE
    }
    public PlayerType myPlayerType = PlayerType.SPECTATOR;

    public bool isConnected = false;

    private void Awake()
    {
        //Making this class a singleton
        /*if(Self)
        {
            Destroy(gameObject);
        }
        else
        {
            Self = this;
            DontDestroyOnLoad(gameObject);
        }*/
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
        PhotonNetwork.JoinOrCreateRoom(roomName, null, PhotonNetwork.CurrentLobby);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room joined");
        isConnected = true;

        RoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        StartCoroutine(WaitForTeamSelection());
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Room left");
        isConnected = false;

        //PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName, null, PhotonNetwork.CurrentLobby, null);
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
        myPlayerType = PlayerType.SPECTATOR;
        playerController.TakeControlOf(null);
    }

    #region Stuff that should be moved to another class
    public void SpawnPlayer()
    {
        if(myPlayerType == PlayerType.SPECTATOR || !isConnected) { return; }

        string prefabName = "";
        if (myPlayerType == PlayerType.HUMAN)
        {
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                        {
                            { "bool_IsHuman", true }
                        };
            PhotonNetwork.SetPlayerCustomProperties(newProperties);
            prefabName = humanPrefab.name;
        }
        else if (myPlayerType == PlayerType.ZOMBIE)
        {
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
                        {
                            { "bool_IsHuman", false }
                        };
            PhotonNetwork.SetPlayerCustomProperties(newProperties);
            prefabName = zombiePrefab.name;
        }

        GameObject player = SpawnPoint.SpawnPlayerAtRandomPoint(prefabName, myPlayerType == PlayerType.ZOMBIE);

        DamageReciever dr = player.GetComponent<DamageReciever>();
        if(dr)
        {
            dr.enabled = true;
            dr.myManager = this;
        }
        TeamBehavior tb = player.GetComponent<TeamBehavior>();
        if(tb) { tb.myManager = this; }
        moveScript ms = player.GetComponent<moveScript>();
        if(ms)
        {
            ms.enabled = true;
            Camera c = ms.head.GetComponent<Camera>();
            lookScript ls = ms.head.GetComponent<lookScript>();
            if(c) { c.enabled = true; }
            if(ls) { ls.enabled = true; }
        }
        else
        {
            advancedMoveScript ams = player.GetComponent<advancedMoveScript>();
            if(ams)
            {
                ams.enabled = true;
                Camera c = ams.head.GetComponent<Camera>();
                lookScript ls = ams.head.GetComponent<lookScript>();
                if (c) { c.enabled = true; }
                if (ls) { ls.enabled = true; }
            }
        }

        //No idea if this'll work or cause more sync errors
        Pawn p = player.GetComponent<Pawn>();
        playerController.TakeControlOf(p);
    }

    protected IEnumerator WaitForTeamSelection()
    {
        while(myPlayerType == PlayerType.SPECTATOR)
        {
            yield return null;
        }

        SpawnPlayer();
    }
#endregion

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        _roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        _roomList = roomList;
    }
}
