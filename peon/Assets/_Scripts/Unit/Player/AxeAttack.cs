using System;
using _Scripts.Unit.AI;
using Photon.Pun;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Unit.Player
{
    [Serializable]
    public class Attack
    {
        public ParticleSystem AttackEffect;
        public ParticleSystem HitEffect;
        public float EffectSpeed;
        public float DamageOffset;
        
        [Space] 
        
        public float Range;
        public float Angle;
        public float Damage;
        public float Knockback;
    }
    public class AxeAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        
        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;

        [SerializeField] private float _attackTime;
        
        private int _attackComboCount;
        [SerializeField] private float _comboMissTimeout;

        [SerializeField] private Attack[] _attacks;
        
        private int _currentAttackNum = 1;

        [SerializeField] private int _maxInCombo;

        [Space]

        [SerializeField] private Transform _attackTransform;

        [Space]
        
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _hit;
        
        private Unit _unit;
        private PhotonView _photonView;
        
        private static readonly int AttackNum = Animator.StringToHash("AttackNum");
        private static readonly int Attack1 = Animator.StringToHash("Attack");

        private readonly SerialDisposable _attack = new SerialDisposable();
        private readonly SerialDisposable _effect = new SerialDisposable();
        private readonly SerialDisposable _damageEffect = new SerialDisposable();
        
        private readonly Collider[] _results = new Collider[6];

        private static readonly float[] _cooldowns = { 0.12f, 0.09f, 0.3f };
        private Movement _movement;

        private void Awake()
        {
            _attack.AddTo(this);
            _effect.AddTo(this);
            _damageEffect.AddTo(this);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();
            _movement = GetComponent<Movement>();
            
            Observable.EveryUpdate() 
                .Where(_ => Input.GetKeyUp(KeyCode.Mouse0))
                .Subscribe (x =>
                {
                    if (_attackCooldownTimer + _attackCooldown > Time.time) return;
                    if (_unit.CurrentState != UnitState.Default) return;

                    Attack();
                }).AddTo (this); 
        }

        private void Attack()
        {
            _unit.CurrentState = UnitState.Attack;
            
            var ray = _unit.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _movement.LookPosition = hit.point;
                _movement.LookPosition.y = 0;
            }
            
            _currentAttackNum = GetAttackNumFromCombo(_attackComboCount);
            var i = _currentAttackNum - 1;
            
            // Attack effect
            _attack.Disposable = Observable.Timer(TimeSpan.FromSeconds(_cooldowns[i])).Subscribe(z =>
            {
                _photonView.RPC(nameof(AttackEffect), RpcTarget.AllViaServer, i);
                
                // Damage
                _attack.Disposable = Observable.Timer(TimeSpan.FromSeconds(_attacks[i].DamageOffset)).Subscribe(x =>
                {
                    DoDamage(_currentAttackNum);
                    AddCombo();
                    
                    // End Attack
                    _attack.Disposable = Observable.Timer(TimeSpan.FromSeconds(_attackTime)).Subscribe(c =>
                    {
                        _attackCooldownTimer = Time.time;
                        _unit.CurrentState = UnitState.Default;
                    });
                });
            });
            
            // Animator
            _unit.Animator.SetInteger(AttackNum, _currentAttackNum);
            _unit.Animator.SetTrigger(Attack1);
        }
        
        [PunRPC]
        private void AttackEffect(int attackNum)
        {
            var eff = Instantiate(_attacks[attackNum].AttackEffect);
            eff.transform.SetPositionAndRotation(_attacks[attackNum].AttackEffect.transform.position, _attacks[attackNum].AttackEffect.transform.rotation);
            var m = eff.main; m.simulationSpeed = _attacks[attackNum].EffectSpeed;

            _effect.Disposable = Observable.Timer(TimeSpan.FromSeconds(.3f)).Subscribe(z =>
            {
                Destroy(eff.gameObject);
            });
                
            eff.Play();
        }

        private int GetAttackNumFromCombo(int combo)
        {
            int attackNum = 1;
            if (combo >= 1 && combo < _maxInCombo) attackNum = Random.Range(1, 3);
            if (combo >= _maxInCombo) attackNum = 3;
            
            return attackNum;
        }

        private void AddCombo()
        {
            if (_attackComboCount >= _maxInCombo) _attackComboCount = 0;

            if (_attackCooldownTimer + _comboMissTimeout >= Time.time)
                _attackComboCount += 1;
            else
                _attackComboCount = 0;
        }
        
        private void DoDamage(int attackNum)
        {
            var i = attackNum - 1;
            
            var range = _attacks[i].Range;
            var angle = _attacks[i].Angle / 2 * Mathf.Deg2Rad;
            var damage = _attacks[i].Damage;

            var _transform = _attackTransform;
            
            var size = Physics.OverlapSphereNonAlloc(_transform.position, range, _results, _layerMask);

            for(int j = 0; j < size; j++)
            {
                var unit = _results[j].GetComponent<Unit>();
                if (unit != null && unit.enabled)
                {
                    var uTransform = unit.transform;
                    var position = uTransform.position;
                    
                    var posTo = (position - _transform.position).normalized;
                    var dot = Vector3.Dot(posTo, _transform.forward);
                    
                    if (dot >= Mathf.Cos(angle))
                    {
                        unit.PhotonView.RPC(nameof(AIHealth.TakeDamage), RpcTarget.AllViaServer,
                            damage, _unit.BounceDamage, _unit.TimeToStan);
                        _photonView.RPC(nameof(DamageEffect), RpcTarget.AllViaServer, Random.Range(0, 100), i, position.x, position.y, position.z);

                        posTo.y = 0;
                        posTo *= _attacks[i].Knockback;
                        unit.PhotonView.RPC(nameof(AIHealth.AddVelocity), RpcTarget.AllViaServer,
                            posTo.x, posTo.y, posTo.z);
                    }
                }
            }
        }

        [PunRPC]
        private void DamageEffect(int seed, int i, float x, float y, float z)
        {
            var pos = new Vector3(x, y, z);
            
            _audioSource.PlayOneShot(_hit[new System.Random(seed).Next(0, _hit.Length)]);

            var p = Instantiate(_attacks[i].HitEffect);
            p.transform.position = pos + new Vector3(0, .5f, 0);
            
            _damageEffect.Disposable = Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(_ =>
            {
                Destroy(p.gameObject);
            });
        }
        
    }
}