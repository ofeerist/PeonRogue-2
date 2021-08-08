using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _Scripts.Unit.Player
{
    class PlayerAxeAttack : UnitAttack, IPunObservable, IOnEventCallback
    {
        private float _attackCooldown;

        private int _attackComboCount;
        [SerializeField] private float _comboMissTimeout;
        [SerializeField] private ParticleSystem[] _attackEffects = new ParticleSystem[3];
        [SerializeField] private ParticleSystem[] _hitEffects = new ParticleSystem[3];
        [SerializeField] private float[] _attackEffectsSpeed = new float[3];

        [Space]

        [SerializeField] private float[] _Range = new float[3];
        [SerializeField] private float[] _Angle = new float[3];
        [SerializeField] private float[] _Damage = new float[3];
        [SerializeField] private float[] _Knockback = new float[3];

        private int _currentAttackNum;

        [SerializeField] private int _MaxInCombo;

        [Space]

        [SerializeField] private Transform _attackTransform;

        [Space]

        [SerializeField] private ParticleSystem _rollEffect;
        [SerializeField] private float _rollTimeToStart;
        [SerializeField] private int _rollMaxTime;

        private int _rollingTime;
        public int RollingTime
        {
            get => _rollingTime;
            private set
            {
                _rollingTime = value;
                ChargeChanged?.Invoke(value);
            }
        }
        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;


        [SerializeField] private float _rollingTimeRate;
        [SerializeField] private float _rollingTimeRecoverTime;
        private float _rollingTimeRecoverTimer;
        private float _rollTimer;
        private float _currentRollRateTimer;
        public bool InRoll { get; private set; }
        [SerializeField] private float _rollSpeed;

        [Space]

        [SerializeField] private float _rollTimeBetweenAttack;
        private float _rollAttackTimesTimer;
        [SerializeField] private float _rollDamage;
        [SerializeField] private float _rollRadius;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _hit;

        [SerializeField] private AudioSource _rollAudioSource;

        [SerializeField] private AudioSource _jagAudioSource;
        [SerializeField] private AudioClip _jaggernaut;

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case (byte)Event.Damage:
                    int attackNum = (int)photonEvent.CustomData;
                    DoDamage(attackNum);
                    break;

                case (byte)Event.RollDamage:
                    var data = (float[])photonEvent.CustomData;
                    var pos = new Vector3(data[0], data[1], data[2]);
                    DoRollDamage(pos);
                    break;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentAttackNum);
            }
            else
            {
                _currentAttackNum = (int)stream.ReceiveNext();
            }
        }

        private void Start()
        {
            InAttack = false;
            _attackCooldown = 0;
            _attackComboCount = 0;
            _rollingTimeRecoverTimer = Time.time + _rollingTimeRecoverTime;
            RollingTime = _rollMaxTime;
            _rollAttackTimesTimer = 0;
        }
        private void Update()
        {
            if (!Unit.PhotonView.IsMine) return;

            if (!Unit.enabled) return;

            if (Input.GetMouseButton(0) && !InAttack && !Unit.UnitMovement.Blocking)
            {
                if (_attackCooldown + _Speed <= Time.time)
                {
                    StartCoroutine(AttackTrigger());
                }
            }

            if (Input.GetMouseButton(0) && !InAttack && RollingTime > 0 && !InRoll)
            {
                InRoll = true;
                _rollTimer = Time.time + _rollTimeToStart;
                _currentRollRateTimer = Time.time + _rollingTimeRate;
            }

            if (Input.GetMouseButton(0) && InRoll)
            {
                if(_rollTimer <= Time.time)
                {
                    InAttack = true;
                    if (RollingTime > 0)
                    {
                        if (!_rollEffect.isPlaying)
                        { 
                            Unit.PhotonView.RPC(nameof(RollEffect), RpcTarget.AllViaServer, true);
                        }
                        if (_currentRollRateTimer <= Time.time)
                        {
                            RollingTime -= 1;
                            _currentRollRateTimer = Time.time + _rollingTimeRate;
                        }
                    }
                    else
                    {
                        InAttack = false;

                        Unit.PhotonView.RPC(nameof(RollEffect), RpcTarget.AllViaServer, false);

                        _rollAudioSource.Stop();
                        _rollTimer = Mathf.Infinity;
                    }
                }
            }

            if (!Input.GetMouseButton(0))
            {
                if (InRoll)
                {
                    InAttack = false;

                    Unit.PhotonView.RPC(nameof(RollEffect), RpcTarget.AllViaServer, false);

                    _rollTimer = Mathf.Infinity;
                }
                InRoll = false;
            }

            if(_rollingTimeRecoverTimer <= Time.time)
            {
                _rollingTimeRecoverTimer = Time.time + _rollingTimeRecoverTime;
                if(RollingTime < _rollMaxTime)
                    RollingTime++;
            }

            // Roll Damage
            if (InRoll && InAttack)
            {
                if(_rollAttackTimesTimer <= Time.time)
                {
                    _rollAttackTimesTimer = Time.time + _rollTimeBetweenAttack;

                    RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    SendOptions sendOptions = new SendOptions { Reliability = true };

                    var pos = transform.position;
                    PhotonNetwork.RaiseEvent((byte)Event.RollDamage, new float[] { pos.x, pos.y, pos.z }, options, sendOptions);
                }

                if (_rollAudioSource.time >= 1f)
                {
                    _rollAudioSource.time = .2f;
                }
            }
        }

        private IEnumerator AttackTrigger()
        {
            yield return 0;

            if (!Input.GetMouseButton(0))
            {
                Attack();
                StartCoroutine(AttackCheck());
            }
        }
        
        [PunRPC]
        private void RollEffect(bool start)
        {
            if (start)
            {
                _rollEffect.Play();

                _rollAudioSource.Play();
                if (Random.Range(0, 4) == 0) _jagAudioSource.PlayOneShot(_jaggernaut);
            }
            else
            {
                _rollEffect.Stop();
            }
        }

        private float _rotation = 0;
        private void FixedUpdate()
        {
            if (InRoll && InAttack)
            {
                _rotation += Time.deltaTime * _rollSpeed;
                transform.rotation = Quaternion.Euler(0, _rotation, 0);
            }
        }
        
        [PunRPC]
        private void AttackEffect(int attackNum)
        {
            StartCoroutine(StartAttackEffect(attackNum));
        }

        private IEnumerator AttackCheck()
        {
            Unit.PhotonView.RPC(nameof(AttackEffect), RpcTarget.AllViaServer,_currentAttackNum - 1);
            
            yield return new WaitForSeconds(.4f);
            InAttack = false;

            _currentAttackNum = -1;

            _attackCooldown = Time.time;
        }

        private IEnumerator StartAttackEffect(int i)
        {
            var cooldowns = new float[3] { 0.12f, 0.09f, 0.3f };

            yield return new WaitForSeconds(cooldowns[i]);

            var eff = Instantiate(_attackEffects[i]);
            eff.transform.SetPositionAndRotation(_attackEffects[i].transform.position, _attackEffects[i].transform.rotation);
            var m = eff.main; m.simulationSpeed = _attackEffectsSpeed[i];
            StartCoroutine(DestroyEffectTimed(eff, .6f));
            eff.Play();
        }

        private int GetAttackNumFromCombo(int combo)
        {
            int attackNum = 1;
            if (combo >= 1 && combo < _MaxInCombo) attackNum = Random.Range(1, 3);
            if (combo >= _MaxInCombo) attackNum = 3;
            
            return attackNum;
        }

        private void Attack()
        {
            InAttack = true;

            int attackNum = GetAttackNumFromCombo(_attackComboCount);
            _currentAttackNum = attackNum;

            var main = _attackEffects[attackNum - 1].main;
            main.simulationSpeed = _attackEffectsSpeed[attackNum - 1];

            Unit.Animator.SetInteger("AttackNum", attackNum);
            Unit.Animator.SetTrigger("Attack");

            StartCoroutine(DoAttack(attackNum));
        }

        private void AddCombo()
        {
            if (_attackComboCount >= _MaxInCombo) _attackComboCount = 0;

            if (_attackCooldown + _comboMissTimeout >= Time.time)
                _attackComboCount += 1;
            else
                _attackComboCount = 0;
        }

        private IEnumerator DoAttack(int attackNum)
        {
            yield return new WaitForSeconds(0.1f);

            if (InAttack)
            {
                RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)Event.Damage, attackNum, options, sendOptions);

                AddCombo();
            }
        }

        private void DoRollDamage(Vector3 position)
        {
            var objects = Physics.OverlapSphere(position, _rollRadius);
            foreach (var obj in objects)
            {
                var unit = obj.GetComponent<Unit>();
                if (unit != null && !unit.CompareTag("Player") && unit.UnitHealth != null && unit != Unit && obj.GetComponent<Unit>().enabled)
                {
                    var position1 = unit.transform.position;
                    var posTo = (position1 - position).normalized;

                    unit.UnitHealth.TakeDamage(_rollDamage);
                    _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);

                    var p = Instantiate(_hitEffects[0]);
                    p.transform.position = position1 + new Vector3(0, .5f, 0);
                    StartCoroutine(DestroyEffectTimed(p, .5f));

                    posTo.y = 0;
                    unit.UnitMovement.AddImpulse((posTo) * _Knockback[0]);
                }
            }
        }

        private void DoDamage(int attackNum)
        {
            var range = _Range[attackNum - 1];
            var angle = _Angle[attackNum - 1] / 2 * Mathf.Deg2Rad;
            var damage = _Damage[attackNum - 1];

            var _transform = _attackTransform;

            var objects = Physics.OverlapSphere(_transform.position, range);
            foreach (var obj in objects)
            {
                var unit = obj.GetComponent<Unit>();
                if (unit != null && !unit.CompareTag("Player") && unit.UnitHealth != null && unit != Unit && obj.GetComponent<Unit>().enabled)
                {
                    var posTo = (unit.transform.position - _transform.position).normalized;
                    var dot = Vector3.Dot(posTo, _transform.forward);
                    if (dot >= Mathf.Cos(angle))
                    {
                        unit.UnitHealth.TakeDamage(damage);

                        if(!_audioSource.isPlaying) _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);

                        var p = Instantiate(_hitEffects[attackNum - 1]);
                        p.transform.position = unit.transform.position + new Vector3(0, .5f, 0);
                        StartCoroutine(DestroyEffectTimed(p, .5f));

                        posTo.y = 0;
                        unit.UnitMovement.AddImpulse((posTo) * _Knockback[attackNum - 1]);
                    }
                }
            }
        }

        private IEnumerator DestroyEffectTimed(ParticleSystem particleSystem, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(particleSystem.gameObject);
            _currentAttackNum = -1;
        }

        private void OnDrawGizmos()
        {
            var colors = new UnityEngine.Color[3] { UnityEngine.Color.green, UnityEngine.Color.cyan, UnityEngine.Color.blue };
            var _transform = _attackTransform;

            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(_transform.position, _rollRadius);

            for (int i = 0; i < _Range.Length; i++)
            {
                Gizmos.color = colors[i];

                var vectorTo1 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((_Angle[i] + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((_Angle[i] + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(_transform.position, vectorTo1);

                var vectorTo2 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((-_Angle[i] + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((-_Angle[i] + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(_transform.position, vectorTo2);

                for (float j = 1; j <= 10; j += .2f)
                {
                    vectorTo1 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((_Angle[i] / j + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((_Angle[i] / j + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    vectorTo2 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    Gizmos.DrawLine(vectorTo1, vectorTo2);

                    vectorTo1 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((-_Angle[i] / j + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((-_Angle[i] / j + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    vectorTo2 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((-_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((-_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    Gizmos.DrawLine(vectorTo1, vectorTo2);

                    if (j + .2f > 10)
                    {
                        vectorTo1 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                        vectorTo2 = new Vector3(_transform.position.x + _Range[i] * Mathf.Sin((-_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _Range[i] * Mathf.Cos((-_Angle[i] / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                        Gizmos.DrawLine(vectorTo1, vectorTo2);
                    }
                }
            }
        }
    }
}
