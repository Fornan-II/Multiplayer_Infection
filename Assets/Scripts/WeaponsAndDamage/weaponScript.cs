using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class weaponScript : MonoBehaviour {

    public Camera ownersCamera;
    public Reticule myReticule;
    public TeamBehavior ownersTeam;
    public GameObject hitParticle;
    public int damage = 30;
    public int range = 100;
    public Rigidbody rb;
    public Collider col;
    public Animator myAnimator;

    protected bool _validHit = false;
    protected RaycastHit _hit;
    [SerializeField]
    protected PhotonView _hitPhotonView;
    protected bool _canFireNextShot = false;

    protected virtual void FixedUpdate()
    {
        if (myReticule)
        {
            Reticule.TargetType foundTargetType = Reticule.TargetType.NONE;

            Ray ray = ownersCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out _hit, range))
            {
                _validHit = true;

                _hitPhotonView = _hit.transform.GetComponent<PhotonView>();
                if(_hitPhotonView)
                {
                    foundTargetType = Reticule.TargetType.ENEMY;
                }

                TeamBehavior tb = _hit.transform.GetComponent<TeamBehavior>();
                if (tb)
                {
                    
                    System.Type ownerType = ownersTeam.GetType();
                    System.Type hitType = tb.GetType();
                    if (ownerType == hitType)
                    {
                        //Prevent player from hitting friendlies (if team behavior dictates that)
                        foundTargetType = Reticule.TargetType.FRIENDLY;
                        if(!ownersTeam.AllowsFriendlyFire)
                        {
                            _hitPhotonView = null;
                        }
                    }
                }
            }
            else
            {
                _validHit = false;
                _hitPhotonView = null;
            }

            myReticule.HasTarget(foundTargetType);
        }
    }

    public virtual void FireShot()
    {
        if(!_canFireNextShot) { return; }

        myAnimator.SetTrigger("Attack");
        _canFireNextShot = false;

        if(_validHit)
        {
            PhotonNetwork.Instantiate(hitParticle.name, _hit.point, _hit.transform.rotation);
            
            if (_hitPhotonView)
            {
                //RPCs are basically calling a method over the network,
                _hitPhotonView.RPC("ApplyDamage", RpcTarget.All, damage);
            }
        }
    }

    public virtual void ResetCanFireShot()
    {
        _canFireNextShot = true;
    }
}
