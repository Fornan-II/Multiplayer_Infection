using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TeamBehavior : MonoBehaviour {

    public NetworkedPlayerSpawner playerSpawner;
    public bool AllowsFriendlyFire = true;

    public virtual void OnKill()
    {

    }

    public virtual void OnDie()
    {

    }
}
