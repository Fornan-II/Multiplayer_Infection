using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class weaponScript : MonoBehaviour {

    public Camera ownersCamera;
    public TeamBehavior ownersTeam;
    public GameObject hitParticle;
    public int damage = 30;
    public int range = 100;

    private void Update()
    {
        if(Input.GetButton("Fire1"))
        {
            FireShot();
        }
    }

    public void FireShot()
    {
        RaycastHit hit;
        Ray ray = ownersCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if(Physics.Raycast(ray, out hit, range))
        {
            GameObject par = PhotonNetwork.Instantiate(hitParticle.name, hit.point, hit.transform.rotation);
            PhotonView pv = hit.transform.GetComponent<PhotonView>();
            //Check for friendly fire
            bool letDamage = true;
            TeamBehavior tb = hit.transform.GetComponent<TeamBehavior>();
            if(tb)
            {
                if(!tb.AllowsFriendlyFire)
                {
                    System.Type ownerType = ownersTeam.GetType();
                    System.Type hitType = tb.GetType();
                    if(ownerType == hitType)
                    {
                        letDamage = false;
                    }
                }
            }
            if (pv && letDamage)
            {
                //RPCs are basically calling a method over the network,
                pv.RPC("ApplyDamage", RpcTarget.All, damage);
                Debug.Log("Hit!");
            }
        }
    }
}
