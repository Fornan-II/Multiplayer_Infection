using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillProjectileScript : MonoBehaviour {

    public float LifeSpan = 0.2f;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, LifeSpan);
	}
}
