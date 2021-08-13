using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.Unit.Player
{
    class AxeThrowAttack : MonoCached.MonoCached
    {
        [Header("Regular")]
        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;
        [SerializeField] private float _speed;

        [Header("Roll")]
        [SerializeField] private float _damageRoll;
        [SerializeField] private float _knockbackRoll;
        [SerializeField] private float _speedRoll;

        [Space]

        [SerializeField] private int _maxThrowCharges;

        [Space]

        [SerializeField] private float _axeCreateOffset;

        private int _currentThrowCharges;
        public int CurrentThrowCharges
        {
            get
            {
                return _currentThrowCharges;
            }
            private set
            {
                _currentThrowCharges = value;
                ChargeChanged?.Invoke(value);
            }
        }

        [SerializeField] private float _chargeTimeToRegen;

        [SerializeField] private float _maxFlightDistance;

        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;

        [SerializeField] private ParticleSystem _disposeEffect;

        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;

        private Unit _unit;
        private Animator _animator;
        private Camera _mainCamera;
        private Collider _collider;
        private PhotonView _photonView;

        [SerializeField] private AudioClip[] _hit;

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _animator = _unit.Animator;
            _mainCamera = _unit.Camera;
            _collider = _unit.GetComponentInChildren<Collider>();
            _photonView = _unit.PhotonView;

            _attackCooldownTimer = 0;
            CurrentThrowCharges = _maxThrowCharges;

            InvokeRepeating(nameof(RegenCharges), 0, _chargeTimeToRegen);
        }

        private void RegenCharges() 
        { 
            if (CurrentThrowCharges < _maxThrowCharges) CurrentThrowCharges += 1; 
        }


        protected override void OnTick()
        {
            if (!_photonView.IsMine) return;

            if (!_unit.enabled) return;

            if (CurrentThrowCharges > 0 && _unit.CurrentState != UnitState.Dash)
            {
                if (Input.GetMouseButtonDown(1) && _attackCooldownTimer + _attackCooldown < Time.time)
                {
                    var roll = false;
                    if (_unit.UnitAttack is PlayerAxeAttack pa) roll = pa.InRoll && pa.InAttack;

                    if (roll || !_unit.UnitAttack.InAttack)
                    {
                        Attack();
                    }
                    
                }
            }
        }

        private void Attack()
        {
            _unit.UnitAttack.InAttack = true;

            CurrentThrowCharges -= 1;
            _attackCooldownTimer = Time.time;

            _animator.SetInteger("AttackNum", 2);
            _animator.SetTrigger("Attack");

            StartCoroutine(AxeCreate());
            StartCoroutine(AttackCheck());
        }

        private IEnumerator AxeCreate()
        {
            yield return new WaitForSeconds(_axeCreateOffset);

            var roll = false;
            if (_unit.UnitAttack is PlayerAxeAttack pa) roll = pa.InRoll && pa.InAttack;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 toPoint = hit.point - transform.position;
                toPoint.y = 0;
                toPoint.Normalize();
                var rotation = Quaternion.LookRotation(toPoint, Vector3.up);

                _photonView.RPC(nameof(CreateAxe), RpcTarget.AllViaServer, rotation.x, rotation.y, rotation.z, rotation.w, roll);
            }
        }

        [PunRPC]
        private void CreateAxe(float rx, float ry, float rz, float rw, bool roll)
        {
            var rotation = new Quaternion(rx, ry, rz, rw);
            if (!roll)
                Axe.Create(_collider, transform.position + new Vector3(0, 1, 0), rotation, _speed, _maxFlightDistance, _damage, _knockback, _disposeEffect, _hit);
            else
                Axe.Create(_collider, transform.position + new Vector3(0, 1, 0), rotation, _speedRoll, _maxFlightDistance, _damageRoll, _knockbackRoll, _disposeEffect, _hit, roll);
        }

        private IEnumerator AttackCheck()
        {
            yield return new WaitForSeconds(.4f);

            _unit.UnitAttack.InAttack = false;
        }
    }
}
