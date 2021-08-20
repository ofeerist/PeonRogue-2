using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Unit.AI.Banshee;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.AI.Pudge
{
    public class HookPudge : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private GameObject _hookCell;
        [SerializeField] private GameObject _hookHead;
        [SerializeField] private float _distanceToSpawnCell;
        [SerializeField] private float _maxHookDistance;

        [Space]

        [SerializeField] private float _hookSpeed;
        [SerializeField] private float _hookBackwardSpeed;

        [Space]

        [SerializeField] private float _hookHeadRangeToGrab;
        [SerializeField] private float _rangeToUseHook;
        [SerializeField] private float _minRangeToUseHook;

        [Space]

        [SerializeField] private float _hookCooldown;
        private float _hookCooldownTimer;

        private Unit _unit;

        private List<GameObject> _cells = new List<GameObject>();
        private GameObject _head;

        private Coroutine _hook;

        [Space]

        [SerializeField] private AudioSource _hookSound;
        [SerializeField] private AudioClip _hookFly;
        [SerializeField] private AudioClip _hookHit;
        [SerializeField] private AudioClip _hookReturn;
        
        private PhotonView _photonView;
        
        private readonly Collider[] _results = new Collider[6];
        private static readonly int Hook1 = Animator.StringToHash("Hook");

        private readonly SerialDisposable _serialDisposable = new SerialDisposable();
        
        private Vector3 _targetPosition;
        private Vector3 _currentPosition;
        
        private Unit _victim;
        private readonly RaycastHit[] _hits = new RaycastHit[3];

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }
        
        public void SetData(float maxHookDistance, float speed, float backwardSpeed, float maxRangeToUse, float minRangeToUse, float cooldown)
        {
            _maxHookDistance = maxHookDistance;
            _hookSpeed = speed;
            _hookBackwardSpeed = backwardSpeed;
            _rangeToUseHook = maxRangeToUse;
            _minRangeToUseHook = minRangeToUse;
            _hookCooldown = cooldown;
        }
        public void StopHook()
        {
            StopCoroutine(_hook);
            foreach (var cell in _cells) Destroy(cell);
            Destroy(_head);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();
            _hookCooldownTimer = 0f;

            if (!PhotonNetwork.IsMasterClient) return;

            var _transform = transform;

            // Find Target
            Observable.Interval(TimeSpan.FromSeconds(.5f)).Subscribe(x =>
            {
                if (_unit.CurrentState != UnitState.Default || _hookCooldownTimer > Time.time) return;
                
                var size = Physics.OverlapSphereNonAlloc(transform.position, _rangeToUseHook, _results, _layerMask);
                
                var position = _transform.position;
                for (int i = 0; i < size; i++)
                {
                    var objPosition = _results[i].transform.position;
                    if (Vector3.Distance(objPosition, position) > _minRangeToUseHook)
                    {
                        var dir = (objPosition - position).normalized;
                        var hitsCount = Physics.RaycastNonAlloc(position, dir, _hits, _maxHookDistance);
                        if (hitsCount < 2)
                        {
                            _unit.CurrentState = UnitState.Attack;
                            _hookCooldownTimer = Time.time + _hookCooldown;
                            _photonView.RPC(nameof(Hook), RpcTarget.AllViaServer, position.x, position.y, position.z,
                                objPosition.x, objPosition.y, objPosition.z);
                        }
                    } 
                }

            }).AddTo(this);
        }

        [PunRPC]
        private void Hook(float cx, float cy, float cz, float ex, float ey, float ez)
        {
            _currentPosition = new Vector3(cx, cy, cz);
            _targetPosition = new Vector3(ex, ey, ez);
            
            _unit.Animator.SetTrigger(Hook1);

            _cells.Clear();

            var rotation = Quaternion.LookRotation(_targetPosition - _currentPosition, Vector3.up);
            _head = Instantiate(_hookHead, _currentPosition + new Vector3(0, .5f, 0), rotation);
            _cells.Add(Instantiate(_hookCell, _head.transform.position, rotation));
            
            _hookSound.PlayOneShot(_hookFly);
            
            _serialDisposable.Disposable = Observable.FromMicroCoroutine(HookFly).Subscribe(x =>
            {
                _serialDisposable.Disposable = Observable.FromMicroCoroutine(BackwardFly).Subscribe(z =>
                {
                    foreach (var item in _cells)
                        Destroy(item);
                    Destroy(_head);

                    _hookSound.Stop();
                    _hookSound.PlayOneShot(_hookReturn);

                    _unit.CurrentState = UnitState.Default;
                });
            });
        }

        private IEnumerator HookFly()
        {
            while (true)
            {
                yield return null;

                _head.transform.position = Vector3.MoveTowards(_head.transform.position, _targetPosition, _hookSpeed * Time.deltaTime);

                if (Vector3.Distance(_cells[_cells.Count - 1].transform.position, _head.transform.position) > _distanceToSpawnCell)
                    _cells.Add(Instantiate(_hookCell, _head.transform.position, Quaternion.LookRotation(_targetPosition - _currentPosition, Vector3.up)));
                

                var size = Physics.OverlapSphereNonAlloc(_head.transform.position, _hookHeadRangeToGrab, _results);
                for (int i = 0; i < size; i++)
                {
                    var unit = _results[i].GetComponent<Unit>();
                    if (unit != null && unit != _unit && unit.enabled && unit.CanBeHooked)
                    {
                        _victim = unit;
                        unit.CurrentState = UnitState.Hooked;
                        
                        _hookSound.Stop();
                        _hookSound.PlayOneShot(_hookHit);

                        break;
                    }
                }
                
                if (_victim != null) break;
                if (Vector3.Distance(_head.transform.position, _currentPosition) >= _maxHookDistance || Vector3.Distance(_head.transform.position, _targetPosition) <= .1f)
                    break;
            }
        }

        private IEnumerator BackwardFly()
        {
            while (true)
            {
                yield return null;

                var pos = _head.transform.position = Vector3.MoveTowards(_head.transform.position, _currentPosition, _hookBackwardSpeed * Time.deltaTime);

                var cells = new List<GameObject>(_cells);
                foreach (var cell in _cells)
                {
                    cell.transform.position = Vector3.MoveTowards(cell.transform.position, _currentPosition, _hookBackwardSpeed * Time.deltaTime);

                    if (Vector3.Distance(cell.transform.position, _currentPosition) <= _distanceToSpawnCell)
                    {
                        cells.Remove(cell);
                        Destroy(cell);
                    }

                }
                _cells = new List<GameObject>(cells);

                if (_victim != null)
                {
                    _victim.SetPosition(_head.transform.position);
                }

                if (Vector3.Distance(pos, _currentPosition) <= 1.5f)
                {
                    if (_victim != null) _victim.CurrentState = UnitState.Default;
                    _victim = null;
                }
                    
                if (Vector3.Distance(pos, _currentPosition) <= .1f)
                    break;
            }
        }
    }
}