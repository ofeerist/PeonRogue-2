using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Unit
{
    class NecromancerThrow : MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private int _throws;
        [SerializeField] private float _angle;
        [SerializeField] private bool _clamped;

        [Space]

        [SerializeField] private float _minThrowDelay;
        [SerializeField] private float _maxThrowDelay;
        private float _currentThrowDelay;
        private float _throwDelayTimer;

        [SerializeField] private float _detectDistance;

        [Space]

        [SerializeField] private float _rotateSpeed;

        [Space]

        [SerializeField] private float _prjCreateOffset;
        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;
        [SerializeField] private float _speed;
        [SerializeField] private float _maxFlightDistance;
        [SerializeField] private float _damageRange;
        [SerializeField] private ParticleSystem _disposeEffect;

        private Unit _unit;
        private Animator _animator;
        private Unit _currentTarget;

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _animator = GetComponentInChildren<Animator>();
            _throwDelayTimer = 0;

            InvokeRepeating(nameof(FindTarget), 1, .5f);
        }

        private void FindTarget()
        {
            if (_throwDelayTimer <= Time.time)
            {
                var _transform = transform;

                var objects = Physics.OverlapSphere(_transform.position, _detectDistance, _layerMask);

                Collider closest = null;
                var minDistance = Mathf.Infinity;
                for (int i = 0; i < objects.Length; i++)
                {
                    if (Vector3.Distance(_transform.position, objects[i].transform.position) < minDistance)
                    {
                        closest = objects[i];
                    }
                }

                Throw(closest.GetComponent<Unit>(), _transform);
            }
        }

        protected override void OnTick()
        {
            if (!_unit.enabled) return;
            if (_unit.UnitMovement.Blocking) return;

            if(_currentTarget != null)
            {
                var toPoint = _currentTarget.transform.position - transform.position;
                var rotation = Quaternion.LookRotation(toPoint, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, _rotateSpeed * Time.deltaTime);
            }
        }

        private void Throw(Unit unit, Transform transform)
        {
            _unit.UnitAttack.InAttack = true;
            _currentThrowDelay = Random.Range(_minThrowDelay, _maxThrowDelay);
            _throwDelayTimer = Time.time + _currentThrowDelay;
            _animator.SetTrigger("Attack");

            _currentTarget = unit;

            var toPoint = unit.transform.position - transform.position;
            var rotation = Quaternion.LookRotation(toPoint, Vector3.up);

            if (_throws == 1)
            {
                StartCoroutine(PrjCreate(rotation));
            }
            else 
            {
                if (_clamped) SpawnClampFan(rotation, _angle, _throws);
                else SpawnFan(rotation, _angle, _throws);
            }

            
            StartCoroutine(StopAttack(_animator.GetCurrentAnimatorClipInfo(0).Length));
        }

        public void SpawnClampFan(Quaternion rotation, float angle, int throws)
        {
            var outsideRotation = rotation * Quaternion.Euler(0, -(angle / 2), 0);

            var offsetAngle = angle / (throws - 1);
            for (int i = 0; i < throws; i++)
            {
                var r = outsideRotation * Quaternion.Euler(0, offsetAngle * i, 0);
                StartCoroutine(PrjCreate(r));
            }
        }

        public void SpawnFan(Quaternion rotation, float angle, int throws)
        {
            if (throws % 2 == 1) StartCoroutine(PrjCreate(rotation));

            int throwPerSide;
            if (throws % 2 == 0) throwPerSide = _throws / 2;
            else throwPerSide = (throws - 1) / 2;

            var side = new int[] { 1, -1 };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j <= throwPerSide; j++)
                {
                    var addAngle = angle * j * side[i] - (throws % 2 == 0 && j == 1 ? (angle / 2) * side[i] : 0);
                    StartCoroutine(PrjCreate(rotation * Quaternion.Euler(0, addAngle, 0)));
                }
            }
        }

        private IEnumerator StopAttack(float time)
        {
            yield return new WaitForSeconds(time);
            _unit.UnitAttack.InAttack = false;
            _currentTarget = null;
        }

        private IEnumerator PrjCreate(Quaternion rotation)
        {
            yield return new WaitForSeconds(_prjCreateOffset);

            NecromancerProjectile.Create(transform.position + new Vector3(0, .4f, 0), rotation, _speed, _maxFlightDistance, _damage, _knockback, _damageRange, _disposeEffect);
        }
    }
}
