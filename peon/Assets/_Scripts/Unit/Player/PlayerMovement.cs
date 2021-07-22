using Photon.Pun;
using UnityEngine;

namespace Game.Unit
{
    class PlayerMovement : UnitMovement
    {
        [Space]

        public Camera MainCamera;

        [Space]

        [SerializeField] private int _dashMaxStock;

        private int _dashCurrentStock;
        public int DashCurrentStock
        {
            get
            {
                return _dashCurrentStock;
            }
            private set
            {
                _dashCurrentStock = value;
                ChargeChanged?.Invoke(value);
            }
        }
        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;

        [SerializeField] private float _dashRefreshTime;
        private float _dashRefreshTimer;
        [SerializeField] private float _dashCooldownTime;
        private float _dashCooldownTimer;

        [Space]

        [SerializeField] private float _dashTime;
        [SerializeField] private float _dashSpeedMultiplier;

        private float _dashAttackTimeOut;
        private int _dashAttackCount = 0;
        private float _currentDashTime;
        private Vector3 _currentDashDirection;
        private bool _inDash;
        public bool InDash { get { return _inDash; } }

        [Space]
        [SerializeField] private int _dashAttackTimes;
        [SerializeField] private float _dashAttackAngle;
        [SerializeField] private float _dashAttackRange;
        [SerializeField] private float _dashAttackDamage;
        [SerializeField] private float _dashAttackKnockback;
        private bool _inDashAttack;

        [Space]
        [SerializeField] private ParticleSystem _dashAttackEffect;
        [SerializeField] private float _dashAttackEffectSpeed;
        [SerializeField] private float _dashAttackEffectLifeTime;

        private Vector3 _vectorToAttackRotate;

        private Vector3 _impact;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _dash;
        [SerializeField] private AudioClip[] _dashAttack;
        [SerializeField] private AudioClip[] _dashHit;

        public override void AddImpulse(Vector3 direction, bool stan = false, float stanTime = 1f)
        {
            _impact += direction;
            if (stan) Unit.UnitHealth.Stan(stanTime);
        }
        private void Start()
        {
            DashCurrentStock = _dashMaxStock;
            _dashRefreshTimer = Time.time + _dashRefreshTime;

            _vectorToAttackRotate = Vector3.zero;
            _inDashAttack = false;
        }

        private void Update()
        {
            if (!Unit.PhotonView.IsMine) return;

            if(Unit.UnitHealth.InStan)
            {
                _currentDashTime = 0;
                BlockMovement = true;
            }

            _impact = Vector3.Lerp(_impact, Vector3.zero, 5 * Time.deltaTime);

            var roll = false;
            if (Unit.UnitAttack is PlayerAxeAttack pa) roll = pa.InRoll && pa.InAttack;

            if (!BlockMovement)
                Unit.Animator.SetBool("Walk", Unit.Controller.velocity.magnitude > 1f);
            else
                Unit.Animator.SetBool("Walk", false);

            var vectorInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            if (!BlockMovement && !_inDashAttack && Input.GetKeyDown(KeyCode.Space)) Dash(vectorInput, roll);

            Unit.Animator.SetBool("DashAttack", _inDashAttack);

            if (_inDash)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    _inDash = false;
                    Unit.PhotonView.RPC(nameof(ResetDash), RpcTarget.All);
                    _inDashAttack = true;

                    var main = _dashAttackEffect.main;
                    main.simulationSpeed *= _dashAttackEffectSpeed;

                    _dashAttackEffect.Play();
                    _audioSource.PlayOneShot(_dashAttack[Random.Range(0, _dashAttack.Length)]);
                }
            }

            if (_inDashAttack)
            {
                Blocking = true;

                float offset = _dashTime / _dashAttackTimes;
                if (_dashAttackTimeOut + offset <= Time.time)
                {
                    _dashAttackCount++;
                    _dashAttackTimeOut = Time.time;
                    DashDamage();
                }
                if (_dashAttackCount >= _dashAttackTimes || _dashAttackTimeOut + offset * _dashAttackTimes <= Time.time)
                {
                    _inDashAttack = false;
                    Blocking = false;
                    _dashAttackCount = 0;
                    _dashAttackEffect.Stop();
                }
            }

            if (_dashRefreshTimer < Time.time)
            {
                if (DashCurrentStock < _dashMaxStock) DashCurrentStock += 1;
                _dashRefreshTimer = Time.time + _dashRefreshTime;
            }

            if ((Unit.UnitAttack.InAttack && !roll) || !Unit.enabled || Blocking || Unit.UnitHealth.InStan) BlockMovement = true;
            else BlockMovement = false;
        }
        private void FixedUpdate()
        {
            if (!Unit.PhotonView.IsMine) return;

            var impact = _impact;

            var roll = false;
            if (Unit.UnitAttack is PlayerAxeAttack pa) roll = pa.InRoll && pa.InAttack;

            float verticalInput = Input.GetAxis("Vertical");
            float horizontalInput = Input.GetAxis("Horizontal");
            var vectorInput = new Vector3(horizontalInput, 0, verticalInput).normalized;

            var _transform = transform;

            if (!BlockMovement)
                Unit.Controller.SimpleMove(vectorInput * Speed + impact);

            if (Unit.Controller.isGrounded && !Unit.UnitAttack.InAttack)
            {
                _vectorToAttackRotate = Vector3.zero;

                Vector3 lookPos = vectorInput;
                lookPos.y = 0;

                if (Unit.Controller.velocity.magnitude > 0)
                {
                    if (lookPos != Vector3.zero)
                        _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(lookPos, Vector3.up), _rotateSpeed * Time.deltaTime);
                }
            }
            else if (Unit.UnitAttack.InAttack)
            {
                if (_vectorToAttackRotate == Vector3.zero)
                {
                    Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Vector3 toPoint = hit.point - transform.position;
                        toPoint.y = 0;
                        toPoint.Normalize();
                        _vectorToAttackRotate = toPoint;
                    }
                }
                else
                {
                    _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_vectorToAttackRotate, Vector3.up), _rotateSpeed * 3 * Time.deltaTime);
                }
            }

            if (_inDash)
            {
                if (_currentDashTime <= Time.time)
                {
                    _inDash = false;
                    Blocking = false;
                    Physics.IgnoreLayerCollision(11, 8, false);
                }

                Unit.Controller.SimpleMove((roll ? _dashSpeedMultiplier * 2 : _dashSpeedMultiplier) * Speed * _currentDashDirection);
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_currentDashDirection, Vector3.up), _dashSpeedMultiplier * _rotateSpeed * Time.deltaTime);
            }

            if (_inDashAttack)
            {
                Unit.Controller.SimpleMove(_dashSpeedMultiplier * Speed * _currentDashDirection);
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_currentDashDirection, Vector3.up), _dashSpeedMultiplier * _rotateSpeed * Time.deltaTime);
            }
        }

        private void DashDamage()
        {
            var _transform = transform;
            var objects = Physics.OverlapSphere(_transform.position, _dashAttackRange);
            foreach (var obj in objects)
            {
                var unit = obj.GetComponent<Unit>();
                if (unit != null && unit.UnitHealth != null && unit != Unit && obj.GetComponent
                    <Unit>().enabled)
                {
                    var posTo = (unit.transform.position - _transform.position).normalized;
                    var dot = Vector3.Dot(posTo, _transform.forward);
                    if (dot >= Mathf.Cos(_dashAttackAngle))
                    {
                        unit.UnitHealth.TakeDamage(_dashAttackDamage);
                        _audioSource.PlayOneShot(_dashHit[Random.Range(0, _dashHit.Length)]);

                        posTo.y = 0;
                        unit.UnitMovement.AddImpulse((posTo) * _dashAttackKnockback);
                    }
                }
            }
        }

        private void Dash(Vector3 input, bool roll)
        {
            if (_dashCooldownTimer > Time.time) return;

            if (DashCurrentStock <= 0) return;

            _audioSource.PlayOneShot(_dash);

            Physics.IgnoreLayerCollision(11, 8);

            if(!roll) Unit.PhotonView.RPC(nameof(TriggerDash), RpcTarget.All);

            DashCurrentStock -= 1;

            if (input != Vector3.zero)
            { _currentDashDirection = input; }
            else
            {
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 toPoint = hit.point - transform.position;
                    toPoint.y = 0;
                    toPoint.Normalize();
                    _currentDashDirection = toPoint;
                }
            }

            _inDash = true;
            Blocking = true;

            _currentDashTime = Time.time + _dashTime;
            _dashCooldownTimer = Time.time + _dashCooldownTime;

            _dashAttackTimeOut = Time.time;
        }

        [PunRPC]
        private void TriggerDash()
        {
            Unit.Animator.SetTrigger("Dash");
        }

        [PunRPC]
        private void ResetDash()
        {
            Unit.Animator.ResetTrigger("Dash");
        }

        private void OnDrawGizmos()
        {
            var _transform = transform;

            Gizmos.color = Color.magenta;

            var vectorTo1 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((_dashAttackAngle + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((_dashAttackAngle + _transform.eulerAngles.y) * Mathf.Deg2Rad));
            Gizmos.DrawLine(_transform.position, vectorTo1);

            var vectorTo2 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((-_dashAttackAngle + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((-_dashAttackAngle + _transform.eulerAngles.y) * Mathf.Deg2Rad));
            Gizmos.DrawLine(_transform.position, vectorTo2);

            for (float j = 1; j <= 10; j += .2f)
            {
                vectorTo1 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((_dashAttackAngle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((_dashAttackAngle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                vectorTo2 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(vectorTo1, vectorTo2);

                vectorTo1 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((-_dashAttackAngle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((-_dashAttackAngle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                vectorTo2 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((-_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((-_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(vectorTo1, vectorTo2);

                if (j + .2f > 10)
                {
                    vectorTo1 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    vectorTo2 = new Vector3(_transform.position.x + _dashAttackRange * Mathf.Sin((-_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _dashAttackRange * Mathf.Cos((-_dashAttackAngle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    Gizmos.DrawLine(vectorTo1, vectorTo2);
                }
            }
        }
    }
}
