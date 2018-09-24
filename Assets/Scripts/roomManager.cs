﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class roomManager : MonoBehaviourPunCallbacks {

    public string roomName = "defaultRoom";

    public GameObject humanPrefab;
    public GameObject zombiePrefab;
    public Transform spawnPos;

    public GameObject controlledObject = null;

    public enum PlayerType
    {
        SPECTATOR,
        HUMAN,
        ZOMBIE
    }
    public PlayerType myPlayerType = PlayerType.SPECTATOR;

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
        StartCoroutine(WaitForTeamSelection());
    }

    public void SpawnPlayer()
    {
        if(myPlayerType == PlayerType.SPECTATOR || controlledObject) { return; }

        string prefabName = "";
        if (myPlayerType == PlayerType.HUMAN) { prefabName = humanPrefab.name; }
        else if (myPlayerType == PlayerType.ZOMBIE) { prefabName = zombiePrefab.name; }

        GameObject player = PhotonNetwork.Instantiate(prefabName, spawnPos.position, spawnPos.rotation) as GameObject;

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

        controlledObject = player;
    }

    protected IEnumerator WaitForTeamSelection()
    {
        while(myPlayerType == PlayerType.SPECTATOR)
        {
            yield return null;
        }

        SpawnPlayer();
    }
}
