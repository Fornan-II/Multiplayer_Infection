using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DestroyEscapingObjects : MonoBehaviour {

    protected virtual void OnTriggerExit(Collider other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if(pv)
        {
            pv.RPC("ApplyDamage", RpcTarget.All, 666);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}