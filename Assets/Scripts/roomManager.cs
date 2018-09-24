using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class roomManager : MonoBehaviourPunCallbacks {

    public string roomName = "defaultRoom";

    public GameObject playerPrefab;
    public Transform spawnPos;

    public bool isConnected = false;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Starting connection");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Master connected to.");
        PhotonNetwork.JoinOrCreateRoom(roomName, null, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room joined");
        isConnected = true;
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos.position, spawnPos.rotation) as GameObject;

        DamageReciever dr = player.GetComponent<DamageReciever>();
        if(dr) { dr.enabled = true; }
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
    }
}
