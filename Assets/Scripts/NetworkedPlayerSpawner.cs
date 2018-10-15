using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedPlayerSpawner : MonoBehaviour {

    public GameObject humanPrefab;
    public GameObject zombiePrefab;

    public PlayerController playerController;

    public enum PlayerType
    {
        SPECTATOR,
        HUMAN,
        ZOMBIE
    }
    public PlayerType myPlayerType = PlayerType.SPECTATOR;
    public Dictionary<PlayerType, bool> playerTypesAllowedToSpawn = new Dictionary<PlayerType, bool>
        {
            { PlayerType.SPECTATOR, false },
            { PlayerType.HUMAN, false },
            { PlayerType.ZOMBIE, false }
        };

    public void SpawnPlayer()
    {
        StartCoroutine(QueuePlayerSpawning());
    }

    protected void ActuallySpawnPlayer()
    {
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
        else if(myPlayerType == PlayerType.SPECTATOR)
        {
            return;
        }

        GameObject player = SpawnPoint.SpawnPlayerAtRandomPoint(prefabName, myPlayerType == PlayerType.ZOMBIE);

        DamageReciever dr = player.GetComponent<DamageReciever>();
        if (dr)
        {
            dr.enabled = true;
        }
        TeamBehavior tb = player.GetComponent<TeamBehavior>();
        if (tb) { tb.playerSpawner = this; }
        moveScript ms = player.GetComponent<moveScript>();
        if (ms)
        {
            ms.enabled = true;
            Camera c = ms.head.GetComponent<Camera>();
            lookScript ls = ms.head.GetComponent<lookScript>();
            if (c) { c.enabled = true; }
            if (ls) { ls.enabled = true; }
        }
        else
        {
            advancedMoveScript ams = player.GetComponent<advancedMoveScript>();
            if (ams)
            {
                ams.enabled = true;
                Camera c = ams.head.GetComponent<Camera>();
                lookScript ls = ams.head.GetComponent<lookScript>();
                if (c) { c.enabled = true; }
                if (ls) { ls.enabled = true; }
            }
        }

        Pawn p = player.GetComponent<Pawn>();
        playerController.TakeControlOf(p);
    }

    protected IEnumerator QueuePlayerSpawning()
    {
        //Make sure player is of allowable type.
        while (!playerTypesAllowedToSpawn[myPlayerType])
        {
            yield return null;
        }
        ActuallySpawnPlayer();
    }
}
