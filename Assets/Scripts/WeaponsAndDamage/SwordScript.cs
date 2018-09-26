using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SwordScript : weaponScript {

    public float LungeForce = 3.0f;
    public float LungeTimeOut = 1.0f;
    public float damageSphereRadius = 0.5f;
    public float damageSphereOffset = 1.0f;

    protected PhotonView _target;
    protected float _lungeTimer = 0.0f;
    protected lookScript _playerLookScript;
    protected Rigidbody _playerBody;

    private void Start()
    {
        if(ownersCamera)
        {
            _playerLookScript = ownersCamera.GetComponent<lookScript>();
            if(_playerLookScript)
            {
                if(_playerLookScript.characterBody)
                {
                    _playerBody = _playerLookScript.characterBody.GetComponent<Rigidbody>();
                }
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if(_lungeTimer > 0.0f)
        {
            _lungeTimer -= Time.fixedDeltaTime;
            if(_lungeTimer <= 0.0f)
            {
                ResetCanFireShot();
            }
        }

        //If we can't fire next shot, then we can assume we are currently attacking.
        if(!_canFireNextShot)
        {
            if(_playerBody && _playerLookScript && _target)
            {
                Vector3 targetDir = _target.transform.position - ownersCamera.transform.position;
                _playerBody.AddForce(ownersCamera.transform.forward * LungeForce, ForceMode.VelocityChange);
                //float angle = Vector3.Angle(ownersCamera.transform.forward, targetDir);
                //_playerLookScript.smoothMouse = targetDir;
            }

            Vector3 OverlapSpherePos = ownersCamera.transform.position + (ownersCamera.transform.forward * damageSphereOffset);
            Collider[] colliders = Physics.OverlapSphere(OverlapSpherePos, damageSphereRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach(Collider c in colliders)
            {
                PhotonView pv = c.GetComponent<PhotonView>();
                if(pv == _target)
                {
                    pv.RPC("ApplyDamage", RpcTarget.All, damage);
                    Debug.Log("Hit!");
                    ResetCanFireShot();
                }
            }
        }
    }

    public override void FireShot()
    {
        if(!_canFireNextShot) { return; }

        //Start anim

        RaycastHit hit;
        Ray ray = ownersCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if(Physics.Raycast(ray, out hit, range))
        {
            _target = hit.transform.GetComponent<PhotonView>();

            TeamBehavior tb = hit.transform.GetComponent<TeamBehavior>();

            //No lunge or applied damage if target is on player's team.
            if (tb && !ownersTeam.AllowsFriendlyFire && _target)
            {
                System.Type ownerType = ownersTeam.GetType();
                System.Type hitType = tb.GetType();
                if (ownerType == hitType) { _target = null; }
            }
        }
        
        if(_target)
        {
            _canFireNextShot = false;
            _lungeTimer = LungeTimeOut;
        }
    }

    public override void ResetCanFireShot()
    {
        base.ResetCanFireShot();
        _target = null;
        _lungeTimer = 0.0f;
        //End attack anim
    }
}
