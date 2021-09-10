using System;
using _Scripts.Level;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Campaign.Ocean
{
    public class ShipMovement : MonoBehaviour
    {
        [SerializeField] private Transform _parentMovement;
        [SerializeField] private Transform _ship;
        [SerializeField] private Transform _water;

        [Space]
        
        [SerializeField] private float _speed;
        [SerializeField] private float _rotationSpeed;

        [Space]
        
        [SerializeField] private float _forwardAngle;
        [SerializeField] private float _anglePerState;
        [SerializeField] private float _maxStates;
        [SerializeField] private float _maxDifference;

        public float MaxDifference
        {
            set => _maxDifference = value;
        }

        private int _currentState;
        private int _movement;

        [Space]
        
        [SerializeField] private Image[] _images;
        [SerializeField] private float _deltaRythm;
        [SerializeField] private float _fadeSpeed;
        [SerializeField] private double _offsetToStart;

        public int[] _lastStates = new int[3];
        private float _lastRythmTime;
        private bool _clicked;
        private int _missCount;
        private float _missed;

        [Space] 
        
        [SerializeField] private float _zOffset;
        [SerializeField] private float _xMaxDispersion;
        [SerializeField] private float _spawnInterval;
        [SerializeField] private RandomInt _spawnCount;
        [SerializeField] private float _lifeTime;
        [SerializeField] private OceanObstacle[] _spawnPrefabs;
        
        private void Start()
        {
            _movement = 0;

            Observable.Interval(TimeSpan.FromSeconds(_spawnInterval)).Subscribe(x =>
            {
                var position = _parentMovement.position;
                for (int i = 0; i < _spawnCount.GetValue(); i++)
                {
                    var gm = Instantiate(_spawnPrefabs[Random.Range(0, _spawnPrefabs.Length)],
                        new Vector3(position.x + Random.Range(-_xMaxDispersion, _xMaxDispersion), 0,
                            position.z + _zOffset), Quaternion.Euler(0, Random.Range(0, 360), 0));

                    if (gm.Validate())
                        Observable.Timer(TimeSpan.FromSeconds(_lifeTime)).Subscribe(z => { Destroy(gm.gameObject); }).AddTo(gm);
                    else
                        Destroy(gm.gameObject);
                }
            }).AddTo(this);
            
            Observable.Timer(TimeSpan.FromSeconds(_offsetToStart)).Subscribe(z =>
            {
                Observable.Interval(TimeSpan.FromSeconds(_deltaRythm)).Subscribe(x =>
                {
                    _lastRythmTime = Time.time;
                    
                    foreach (var image in _images)
                    {
                        image.color = UnityEngine.Color.white;
                    }      
                }).AddTo(this);
            }).AddTo(this);

            Observable.EveryUpdate().Subscribe(x =>
            {
                foreach (var image in _images)
                {
                    image.color = UnityEngine.Color.Lerp(image.color, UnityEngine.Color.clear,
                        _fadeSpeed * Time.deltaTime);
                }

                var a = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                        Input.GetKeyDown(KeyCode.Mouse0);
                
                var d = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ||
                        Input.GetKeyDown(KeyCode.Mouse1);
                
                _movement = a ? -1 : d ? 1 : 0;

                if (_movement != 0)
                {
                    if (_lastRythmTime + _maxDifference >= Time.time ||
                        _lastRythmTime + _deltaRythm - _maxDifference < Time.time)
                    {
                        _lastStates[0] = _lastStates[1];
                        _lastStates[1] = _lastStates[2];
                        _lastStates[2] = _movement;

                        _missCount = 0;
                        _clicked = true;
                    }
                    else
                    {

                        _missed = Time.time;
                        _missCount++;
                        foreach (var image in _images)
                        {
                            image.color = UnityEngine.Color.red;
                        }
                        
                    }
                }
                else
                {
                    if(_missed < _lastRythmTime)
                        if (_lastRythmTime + _maxDifference < Time.time &&
                            _lastRythmTime + _deltaRythm - _maxDifference >= Time.time)
                        {
                            if (!_clicked)
                            {
                                _missed = Time.time;
                                _missCount++;
                                
                                foreach (var image in _images)
                                {
                                    image.color = UnityEngine.Color.red;
                                }
                            }
                            else
                            {
                                _missed = Time.time;
                                _clicked = false;
                            }
                        }
                }

                if (_missCount == 1)
                {
                    _lastStates[0] = _lastStates[1];
                    _lastStates[1] = _lastStates[2];
                    _lastStates[2] = 0;

                    _missCount = 0;
                }
                
            }).AddTo(this);
            
            Observable.EveryFixedUpdate().Subscribe(x =>
            {
                var rotation = _ship.rotation;

                var state = _lastStates[0];
                if (state == _lastStates[1] && _lastStates[1] == _lastStates[2])
                {
                    _lastStates[0] = 2; 
                    _lastStates[1] = 2; 
                    _lastStates[2] = 2; 
                    
                    switch (state)
                    {
                        case 1 when _currentState < _maxStates:
                            _currentState += _currentState >= 0 ? 1 : 2;
                            break;
                        case -1 when _currentState > -_maxStates:
                            _currentState -= _currentState <= 0 ? 1 : 2;
                            break;
                        
                        case 0 when _currentState < 0:
                            _currentState++;
                            break;
                        case 0 when _currentState > 0:
                            _currentState--;
                            break;
                    }
                } 
                
                rotation = Quaternion.Slerp(rotation,
                    Quaternion.Euler(0, _forwardAngle + _anglePerState * _currentState, 0),
                    _rotationSpeed * Time.fixedDeltaTime);
                _ship.rotation = rotation;

                var dir = Quaternion.AngleAxis(rotation.eulerAngles.y, Vector3.up) * Vector3.right;
                var move = -dir * _speed * Time.fixedDeltaTime;
                
                _parentMovement.position += move;
                
                var waterPos = _water.position;
                waterPos += move;
                _water.position = waterPos;
                
            }).AddTo(this);
        }
    }
}