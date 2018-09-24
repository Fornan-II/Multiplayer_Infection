using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamZombie : TeamBehavior {

    private void Awake()
    {
        AllowsFriendlyFire = false;
    }
}
