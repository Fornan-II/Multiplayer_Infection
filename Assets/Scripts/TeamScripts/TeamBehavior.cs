using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TeamBehavior : MonoBehaviour {

    public roomManager myManager;

    public bool AllowsFriendlyFire = true;

    public virtual void OnKill()
    {

    }

    public virtual void OnDie()
    {

    }
}
