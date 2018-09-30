using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SwordScript : weaponScript {

    public float LungeForce = 3.0f;
    public float damageSphereRadius = 0.5f;
    public float damageSphereOffset = 1.0f;

    protected PhotonView _target;
    protected lookScript _playerLookScript;
    protected Rigidbody _playerBody;
    protected advancedMoveScript _playerMovement;
    protected Vector3 _lungeVector = Vector3.zero;
    protected Vector3 _lungeStartingPosition;
    protected float _maxLungeDistance;

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
                    _playerMovement = _playerLookScript.characterBody.GetComponent<advancedMoveScript>();
                }
            }
        }
    }

    protected virtual void FixedUpdate()
    {//If we can't fire next shot, then we can assume we are currently attacking.
        if(!_canFireNextShot)
        {
            if (Vector3.Distance(transform.position, _lungeStartingPosition) >= _maxLungeDistance)
            {
                ResetCanFireShot();
            }

            if(_playerBody && _playerLookScript && _target)
            {
                _playerBody.AddForce(_lungeVector, ForceMode.VelocityChange);
            }

            Vector3 OverlapSpherePos = ownersCamera.transform.position + (ownersCamera.transform.forward * damageSphereOffset);
            Collider[] colliders = Physics.OverlapSphere(OverlapSpherePos, damageSphereRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach(Collider c in colliders)
            {
                PhotonView pv = c.GetComponent<PhotonView>();
                if(pv == _target)
                {
                    pv.RPC("ApplyDamage", RpcTarget.All, damage);
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
            _lungeStartingPosition = transform.position;
            Vector3 targetDirection = _target.transform.position - ownersCamera.transform.position;
            _maxLungeDistance = targetDirection.magnitude;
            _lungeVector = targetDirection.normalized * LungeForce;
            _playerMovement.letBeGrounded = false;
        }
    }

    public override void ResetCanFireShot()
    {
        base.ResetCanFireShot();
        _target = null;
        _lungeVector = Vector3.zero;
        _playerMovement.letBeGrounded = true;
        //_playerBody.velocity = Vector3.zero;
        //End attack anim
    }
}
