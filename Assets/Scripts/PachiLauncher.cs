using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PachiLauncher : MonoBehaviour
{
    [Header("SpEEN")]
    [SerializeField] float _startingLauncherSpinSpeed = 10f;
    [SerializeField] float _maximumLauncherSpinSpeed = 90f;
    [SerializeField] float _accelLauncherSpinSpeed = 1f;

    [Header("FIRE")]
    [SerializeField] float _startingBallLaunchRate = 0.5f;
    [SerializeField] float _maximumBallLaunchRate = 4f;
    [SerializeField] float _accelBallLaunchRate = 0.1f;

    [Header("Power")]
    [SerializeField] float _startingBallLaunchForce = 1f;
    [SerializeField] float _maximumBallLaunchForce = 10f;
    [SerializeField] float _accelBallLaunchForce = 0.5f;

    [Header("Other")]
    [SerializeField] float _stunThreshold;

    [Header("References!")]
    [SerializeField] Transform _swivelHead;
    [SerializeField] Transform _launchPoint;
    [SerializeField] GameObject _projectile;

    float _launcherSpinSpeed;
    float _ballLaunchRate;
    float _ballLaunchForce;
    float _timeToNextBall;

    float _angle;

    [Header("Balls lol")]
    [SerializeField] int _maxBalls = 60;
    GameObject[] _balls; //Lol
    int index = 0;
    bool _itemLimitHit;

    void Start()
    {
        _ballLaunchRate = _startingBallLaunchRate;
        _launcherSpinSpeed = _startingLauncherSpinSpeed;
        _ballLaunchForce= _startingBallLaunchForce;

        _balls = new GameObject[_maxBalls];
    }

    void Update()
    {
        if(_launcherSpinSpeed < _maximumLauncherSpinSpeed)
        {
            _launcherSpinSpeed += _accelLauncherSpinSpeed * Time.deltaTime; //ALT - Do this on every shot?
        }

        _angle = _launcherSpinSpeed * Time.deltaTime;
        _swivelHead.Rotate(0, 0, _angle);

        if (_ballLaunchRate < _maximumBallLaunchRate)
        {
            _ballLaunchRate += _accelBallLaunchRate * Time.deltaTime; //ALT - Do this on every shot fired? Might make accel exponential.
        }
        if (_ballLaunchForce < _maximumBallLaunchForce)
        {
            _ballLaunchForce += _accelBallLaunchForce * Time.deltaTime; //ALT - Do this on every shot fired?
        }

        if (_timeToNextBall < 0)
        {
            ShootBall();
            _timeToNextBall = 1 / _ballLaunchRate;
        }
        else
        {
            _timeToNextBall -= Time.deltaTime;
        }
    }

    void ShootBall()
    {
        if (_itemLimitHit)
        {
            Destroy(_balls[index]);
        }
        _balls[index] = Instantiate(_projectile);
        _balls[index].transform.SetParent(transform);
        _balls[index].transform.position = _launchPoint.position;
        _balls[index].transform.rotation = _launchPoint.rotation;
        _balls[index].GetComponent<Rigidbody>().AddForce(_balls[index].transform.forward * _ballLaunchForce);
        index++;
        if(index == _maxBalls)
        {
            _itemLimitHit = true;
            index = 0;
        }
    }
}
