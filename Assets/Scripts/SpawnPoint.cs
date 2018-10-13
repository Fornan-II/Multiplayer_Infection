using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPoint : MonoBehaviour {

    protected static List<SpawnPoint> AllSpawnPoints = new List<SpawnPoint>();
    protected static List<SpawnPoint> ValidSpawns = new List<SpawnPoint>();
    protected static List<SpawnPoint> PreferredSpawns = new List<SpawnPoint>();

    protected static float HostileCheckRadius = 30.0f;

    public enum SpawnPointState
    {
        INVALID,
        VALID,
        PREFERRED
    }
    [SerializeField]
    protected SpawnPointState _currentState;
    public SpawnPointState CurrentState
    {
        get { return _currentState; }
        set
        {
            //If no change in value, don't do anything.
            if(_currentState == value) { return; }

            //Remove this object from old spawn lists
            if(_currentState == SpawnPointState.PREFERRED)
            {
                PreferredSpawns.Remove(this);
            }
            if(_currentState == SpawnPointState.VALID)
            {
                ValidSpawns.Remove(this);
            }

            //Add this object to new spawn lists
            if(value == SpawnPointState.PREFERRED)
            {
                PreferredSpawns.Add(this);
            }
            if(value == SpawnPointState.VALID)
            {
                ValidSpawns.Add(this);
            }

            //Update _currentState
            _currentState = value;
        }
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        AllSpawnPoints.Add(this);
        CurrentState = SpawnPointState.PREFERRED;
	}

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, HostileCheckRadius);
    }

    protected virtual GameObject SpawnPlayer(string prefabLocation)
    {
        //Debug.Log("Spawning player at " + name);
        CurrentState = SpawnPointState.INVALID;
        return PhotonNetwork.Instantiate(prefabLocation, transform.position, transform.rotation) as GameObject;
    }

    public virtual void UpdateSpawnState()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Player");

        //Check if someone is standing on top of the spawnpoint.
        if (Physics.CheckSphere(transform.position, 2.0f, layerMask, QueryTriggerInteraction.Ignore))
        {
            CurrentState = SpawnPointState.INVALID;
            return;
        }

        //Check if there are any humans near the spawn point.
        Collider[] foundPlayers = Physics.OverlapSphere(transform.position, HostileCheckRadius, layerMask, QueryTriggerInteraction.Ignore);
        foreach(Collider c  in foundPlayers)
        {
            TeamBehavior tb = c.GetComponent<TeamBehavior>();
            if(tb)
            {
                if(tb is TeamHuman)
                {
                    CurrentState = SpawnPointState.VALID;
                    return;
                }
            }
        }

        //No one standing on spawnpoint and no nearby humans; this is a preferred spawnpoint.
        CurrentState = SpawnPointState.PREFERRED;
    }

    public static GameObject SpawnPlayerAtRandomPoint(string prefabLocation, bool UsePreferredSpawns = true)
    {
        foreach(SpawnPoint sp in AllSpawnPoints)
        {
            sp.UpdateSpawnState();
        }

        SpawnPoint chosenSpawn;

        if(!UsePreferredSpawns)
        {
            int maxIndex = PreferredSpawns.Count + ValidSpawns.Count;
            int selectedIndex = Random.Range(0, maxIndex);

            if (selectedIndex < PreferredSpawns.Count)
            {
                chosenSpawn = PreferredSpawns[selectedIndex];
            }
            else
            {
                chosenSpawn = ValidSpawns[selectedIndex - PreferredSpawns.Count];
            }
        }
        else if(PreferredSpawns.Count > 0)
        {
            chosenSpawn = PreferredSpawns[Random.Range(0, PreferredSpawns.Count)];
        }
        else if (ValidSpawns.Count > 0)
        {
            chosenSpawn = ValidSpawns[Random.Range(0, ValidSpawns.Count)];
        }
        else if(AllSpawnPoints.Count > 0)
        {
            chosenSpawn = ValidSpawns[Random.Range(0, ValidSpawns.Count)];
        }
        else
        {
            Debug.LogWarning("No spawnpoints in scene! Spawning player at (0, 0, 0).");
            return PhotonNetwork.Instantiate(prefabLocation, Vector3.zero, Quaternion.identity) as GameObject;
        }

        return chosenSpawn.SpawnPlayer(prefabLocation);
    }

    public static void ClearSpawnPointList()
    {
        PreferredSpawns.Clear();
        ValidSpawns.Clear();
        AllSpawnPoints.Clear();
    }

    public static void RefreshSpawnPointList()
    {
        ClearSpawnPointList();
        AllSpawnPoints.AddRange(GameObject.FindObjectsOfType<SpawnPoint>());

        foreach (SpawnPoint sp in AllSpawnPoints)
        {
            sp.UpdateSpawnState();
        }
    }
}
