using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public enum BumperStates //we might want to move this to a state pattern for perf.
{
    PATROL,
    CHASE_LOCK, //Will not lock onto new players.
    CHASE_SEARCH, //Will lock onto new players.
    COOLDOWN,
    RETURN
}

public class BumperBot : MonoBehaviour
{
    [SerializeField] SplineAnimate _splineController;

    [SerializeField] BumperStates _state = BumperStates.PATROL;
    [SerializeField] Rigidbody _rb;

    [SerializeField] float _speed = 0;
    [SerializeField] float _acceleration = 2;
    [SerializeField] float _maxSpeed = 30;

    [SerializeField] float _targettingCooldownTime;
    [SerializeField] float _cooldownTargetting;

    [SerializeField] float _movementCooldownTime;
    [SerializeField] float _cooldownMovement;

    [SerializeField] float _explosionForce = 75f;
    [SerializeField] float _upwardForce = 100f;

    [SerializeField] float _returnSnapDistance = 0.1f;

    [SerializeField] float _avoidanceDistance = 5f;
    [SerializeField] float _maxAvoidanceForce = 10f;

    [SerializeField] LayerMask _nonPlayerLayers;

    [SerializeField] GameObject _targetPlayer;

    float loopPerc; //Whole Number represents number of loops completed, decimal the percentage of current loop complete.

    void Reset()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        switch (_state)
        {
            case BumperStates.PATROL:
                Accelerate();
                loopPerc = _splineController.normalizedTime;
                _splineController.maxSpeed = _speed;
                _splineController.normalizedTime = loopPerc;
                transform.position = _splineController.transform.position;
                transform.rotation = _splineController.transform.rotation;
                break;
            case BumperStates.CHASE_LOCK:
                Accelerate();
                Chase(_targetPlayer.transform.position, Vector3.up * 2);
                Avoid();
                CooldownTargetting();
                break;
            case BumperStates.CHASE_SEARCH:
                Accelerate();
                Chase(_targetPlayer.transform.position, Vector3.up * 2);
                Avoid();
                break;
            case BumperStates.COOLDOWN:
                CooldownMovement();
                break;
            case BumperStates.RETURN:
                Accelerate();
                Chase(_splineController.transform.position, Vector3.zero);
                CheckOnSpline();
                break;
        }
    }

    /// <summary>
    /// Increases the velocity of the bumper by acceleration, up to the max speed.
    /// </summary>
    void Accelerate()
    {
        float accel = _acceleration;
        if (_speed > 20)
        {
            accel *= 0.3f;
        }
        _speed += accel * Time.deltaTime;
        _speed = Mathf.Clamp(_speed, 0, _maxSpeed);
    }
    
    /// <summary>
    /// Points the transform towards the target and sets velocity to speed.
    /// </summary>
    /// <param name="target"></param>
    void Chase(Vector3 target, Vector3 offset)
    {
        transform.LookAt(target + offset);
        _rb.velocity = transform.forward * _speed;
    }

    void Avoid()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _avoidanceDistance, _nonPlayerLayers))
        {
            // Calculate the avoidance force based on the distance to the obstacle
            float distance = hit.distance;
            float force = Mathf.Lerp(_maxAvoidanceForce, 0f, distance / _avoidanceDistance);
            Vector3 avoidanceDirection = Vector3.Cross(transform.up, hit.normal);
            Vector3 avoidanceForce = avoidanceDirection * force;

            // Apply the avoidance force to the rigidbody
            _rb.AddForce(avoidanceForce);
        }
    }

    /// <summary>
    /// Processes the cooldown upon hitting a target before it can begin moving again.
    /// </summary>
    void CooldownMovement()
    {
        _cooldownMovement -= Time.deltaTime;
        if(_cooldownMovement <= 0)
        {
            _state = BumperStates.RETURN;
        }
    }

    /// <summary>
    /// Processes the cooldown for targetting.
    /// </summary>
    void CooldownTargetting()
    {
        _cooldownTargetting -= Time.deltaTime;
        if(_cooldownTargetting <= 0.0f)
        {
            _state = BumperStates.CHASE_SEARCH;
        }
    }

    void CheckOnSpline()
    {
        if((_splineController.transform.position - transform.position).magnitude < _returnSnapDistance)
        {
            _state = BumperStates.PATROL;
            _splineController.Play();
        }
    }

    /// <summary>
    /// When a player enters this trigger, they should be targetted by the bumper.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (_state == BumperStates.PATROL || _state == BumperStates.CHASE_SEARCH)
        {
            if (other.CompareTag("Player") && other.gameObject != _targetPlayer)
            {
                _targetPlayer = other.gameObject;
                _state = BumperStates.CHASE_LOCK;
                _cooldownTargetting = _targettingCooldownTime;
                _splineController.Pause();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            GetComponent<AssetAudioController>().YippeeSFX(); //not actually yippee but same thing
            Rigidbody otherRB = collision.rigidbody;
            //collision.rigidbody.AddExplosionForce(_speed * _explosionForce, collision.contacts[0].point, 10, _upwardForce);
            Vector3 hitDirection = (otherRB.worldCenterOfMass - GetComponent<Rigidbody>().worldCenterOfMass).normalized;
            hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z);

            otherRB.AddForce(Vector3.up * _upwardForce, ForceMode.Impulse);
            otherRB.AddForce(hitDirection * (_explosionForce + GetComponent<Rigidbody>().velocity.magnitude * 5), ForceMode.Impulse);

            collision.gameObject.GetComponentInParent<TpMovement>().HitStun();
        }
        if (_state == BumperStates.RETURN)
        {
            return;
        }
        _speed = 0;
        _rb.velocity = Vector3.zero;
        if (_state == BumperStates.PATROL)
        {
            return;
        }
        _cooldownMovement = _movementCooldownTime;
        _state = BumperStates.COOLDOWN;
        _targetPlayer = null;
    }
}
