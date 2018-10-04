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
    protected bool _isLunging = false;

    protected virtual void Start()
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //If we can't fire next shot, then we can assume we are currently attacking.
        if(_isLunging)
        {
            if (Vector3.Distance(transform.position, _lungeStartingPosition) >= _maxLungeDistance)
            {
                ResetIsLunging();
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
                    ResetIsLunging();
                }
            }
        }

        if(!_canFireNextShot && !_isLunging)
        {
            ResetIsLunging();
        }
    }

    public override void FireShot()
    {
        if(!_canFireNextShot) { return; }

        myAnimator.SetBool("IsLunging", true);
        _canFireNextShot = false;

        if(_hitPhotonView)
        {
            _target = _hitPhotonView;
            _isLunging = true;
            _lungeStartingPosition = transform.position;
            Vector3 targetDirection = _hitPhotonView.transform.position - ownersCamera.transform.position;
            _maxLungeDistance = targetDirection.magnitude;
            _lungeVector = targetDirection.normalized * LungeForce;
            _playerMovement.letBeGrounded = false;
        }
    }

    protected virtual void ResetIsLunging()
    {
        _isLunging = false;
        _target = null;
        _lungeVector = Vector3.zero;
        _playerMovement.letBeGrounded = true;
        myAnimator.SetBool("IsLunging", false);
    }
}
