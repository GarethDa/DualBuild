using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingChip : MonoBehaviour
{
    [SerializeField] Animator _animator;

    [Tooltip("How long will the platform wobble for?")]
    [SerializeField] float _wobbleTime = 3f;
    [Tooltip("The velocity at which the platform will fall.")]
    [SerializeField] float _fallSpeed = 1f;
    [Tooltip("Can only the player trigger the platforms?")]
    [SerializeField] bool _onlyPlayer = false;

    float _wobbleCounter; //How long the platform has wobbled for.
    bool _triggered; //Has the platform begun to wobble?
    bool _falling; //Has the platform begun to fall.

    float x, z; //The X and Z positions shouldn't chance, so store them here.

    private void Start()
    {
        x = transform.position.x;
        z = transform.position.z;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_onlyPlayer && !collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        if (!_falling)
        {
            _triggered = true;
            _animator.Play("PlatformWobble");
        }
    }

    void Update()
    {
        if(_triggered )
        {
            _wobbleCounter += Time.deltaTime;

            if(_wobbleCounter > _wobbleTime)
            {
                _falling = true;
                _triggered = false;
            }
        }
        else if (_falling)
        {
            transform.position = new Vector3(x, transform.position.y - _fallSpeed * Time.deltaTime, z);
        }
    }
}
