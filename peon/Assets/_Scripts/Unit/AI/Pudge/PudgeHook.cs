using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Game.Unit
{
    class PudgeHook : MonoCached
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

        private bool _isHook;

        private Unit _unit;

        private List<GameObject> _cells;
        private GameObject _head;

        private Coroutine _hook;

        [Space]

        [SerializeField] private AudioSource _hookSound;
        [SerializeField] private AudioClip _hookFly;
        [SerializeField] private AudioClip _hookHit;
        [SerializeField] private AudioClip _hookReturn;

        public void StopHook()
        {
            StopCoroutine(_hook);
            foreach (var cell in _cells) Destroy(cell);
            Destroy(_head);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _hookCooldownTimer = 0f;

            InvokeRepeating(nameof(FindToHook), 1, .5f);
        }

        private void FindToHook()
        {
            if (!_unit.enabled) return;

            if (_hookCooldownTimer <= Time.time && !_isHook)
            {
                var objects = Physics.OverlapSphere(transform.position, _rangeToUseHook, _layerMask);
                var units = new List<Unit>();
                var pos = transform.position;
                foreach (var obj in objects)
                {
                    var Epos = obj.transform.position;
                    if (Vector3.Distance(pos, Epos) <= _minRangeToUseHook) continue;

                    var dir = Epos - pos;
                    var hits = Physics.RaycastAll(pos, dir, _maxHookDistance, _layerMask);
                    if (hits[1].collider == obj)
                    {
                        var unit = obj.GetComponent<Unit>();

                        if (unit == null || unit.IsHooked) continue;

                        units.Add(unit);
                    }
                }

                Unit closest = null;
                float distance = Mathf.Infinity;
                foreach (var u in units)
                {
                    var Upos = u.transform.position;
                    if (closest != null && Vector3.Distance(pos, Upos) < distance)
                    {
                        closest = u;
                        distance = Vector3.Distance(pos, Upos);
                    }

                    if(closest == null)
                    {
                        closest = u;
                        distance = Vector3.Distance(pos, Upos);
                    }
                }

                if (closest != null)
                {
                    _cells = new List<GameObject>();
                    _hookCooldownTimer = Time.time + _hookCooldown;

                    if (closest.TryGetComponent<BansheeShoutAttack>(out var bsa)) bsa.StopShout();
                    if (closest.TryGetComponent<PudgeHook>(out var ph)) ph.StopHook();

                    _hook = StartCoroutine(Hook(closest.transform.position));
                }
            }
        }

        protected override void OnTick()
        {
            if (!_unit.enabled) return;

            _unit.UnitMovement.BlockMovement = _isHook;
            _unit.UnitAttack.InAttack = _isHook;
        }

        private IEnumerator Hook(Vector3 pos)
        {
            _isHook = true;
            _unit.UnitAttack.enabled = false;
            _unit.Animator.SetTrigger("Hook");

            _cells.Clear();

            _head = Instantiate(_hookHead, transform.position + new Vector3(0, .5f, 0), Quaternion.LookRotation(pos - transform.position, Vector3.up));
            _cells.Add(Instantiate(_hookCell, _head.transform.position, Quaternion.LookRotation(pos - transform.position, Vector3.up)));

            _hookSound.clip = _hookFly;
            _hookSound.Play();

            Unit victim = null;
            while (true)
            {
                yield return null;

                _head.transform.position = Vector3.MoveTowards(_head.transform.position, pos, _hookSpeed * Time.deltaTime);

                if (Vector3.Distance(_cells[_cells.Count - 1].transform.position, _head.transform.position) > _distanceToSpawnCell)
                    _cells.Add(Instantiate(_hookCell, _head.transform.position, Quaternion.LookRotation(pos - transform.position, Vector3.up)));
                

                var objects = Physics.OverlapSphere(_head.transform.position, _hookHeadRangeToGrab);
                foreach (var obj in objects)
                {
                    var unit = obj.GetComponent<Unit>();
                    if (unit != null && unit != _unit && unit.enabled)
                    {
                        victim = unit;

                        _hookSound.Stop();
                        _hookSound.PlayOneShot(_hookHit);

                        break;
                    }
                }

                if (victim != null) break;
                if (Vector3.Distance(_head.transform.position, transform.position) >= _maxHookDistance || Vector3.Distance(_head.transform.position, pos) <= .1f)
                    break;
                
            }

            _hookSound.clip = _hookFly;
            _hookSound.Play();

            while (true)
            {
                yield return null;

                _head.transform.position = Vector3.MoveTowards(_head.transform.position, transform.position, _hookBackwardSpeed * Time.deltaTime);

                var cells = new List<GameObject>(_cells);
                foreach (var cell in _cells)
                {
                    cell.transform.position = Vector3.MoveTowards(cell.transform.position, transform.position, _hookBackwardSpeed * Time.deltaTime);

                    if (Vector3.Distance(cell.transform.position, transform.position) <= _distanceToSpawnCell)
                    {
                        cells.Remove(cell);
                        Destroy(cell);
                    }

                }
                _cells = new List<GameObject>(cells);

                if (victim != null)
                {
                    victim.IsHooked = true;
                    victim.UnitMovement.BlockMovement = true;
                    victim.transform.position = _head.transform.position;
                }

                if (Vector3.Distance(_head.transform.position, transform.position) <= 1.5f)
                {
                    if(victim != null)
                        victim.IsHooked = false;
                    victim = null;
                }
                    
                if (Vector3.Distance(_head.transform.position, transform.position) <= .1f)
                    break;
                
            }

            foreach (var item in _cells)
                Destroy(item);
            

            Destroy(_head);

            _hookSound.Stop();
            _hookSound.PlayOneShot(_hookReturn);

            _unit.UnitAttack.enabled = true;
            _isHook = false;
        }
    }
}
