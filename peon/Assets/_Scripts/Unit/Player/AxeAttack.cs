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

        [HideInInspector] public Vector3 LookPosition;
        
        private void Start()
        {
            _unit = GetComponent<Unit>();
            _photonView = GetComponent<PhotonView>();
            
            Observable.EveryUpdate() 
                .Where(_ => Input.GetKeyDown(KeyCode.Mouse0))
                .Subscribe (x =>
                {
                    if (_attackCooldownTimer + _attackCooldown > Time.time) return;
                    if (_unit.CurrentState != UnitState.Default) return;

                    Observable.NextFrame().Subscribe(z =>
                    {
                        if (_unit.CurrentState != UnitState.Default) return;
                        Attack();

                    }).AddTo(this);
                }).AddTo (this); 
        }

        private void Attack()
        {
            _unit.CurrentState = UnitState.Attack;
            
            var ray = _unit.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                LookPosition = hit.point;
                LookPosition.y = 0;
            }


            _currentAttackNum = GetAttackNumFromCombo(_attackComboCount);
            var i = _currentAttackNum - 1;
            
            // Attack effect
            var cooldowns = new[] { 0.12f, 0.09f, 0.3f };
            Observable.Timer(TimeSpan.FromSeconds(cooldowns[i])).Subscribe(x =>
            {
                _photonView.RPC(nameof(AttackEffect), RpcTarget.AllViaServer, i);
            }).AddTo(this);
            
            // Animator
            _unit.Animator.SetInteger(AttackNum, _currentAttackNum);
            _unit.Animator.SetTrigger(Attack1);
            
            // Damage
            Observable.Timer(TimeSpan.FromSeconds(_attacks[i].DamageOffset)).Subscribe(x =>
            {
                DoDamage(_currentAttackNum);
                AddCombo();
            }).AddTo(this);
            
            // End Attack
            Observable.Timer(TimeSpan.FromSeconds(_attackTime)).Subscribe(x =>
            {
                _attackCooldownTimer = Time.time;
                _unit.CurrentState = UnitState.Default;
            }).AddTo(this);
        }
        
        [PunRPC]
        private void AttackEffect(int attackNum)
        {
            var eff = Instantiate(_attacks[attackNum].AttackEffect);
            eff.transform.SetPositionAndRotation(_attacks[attackNum].AttackEffect.transform.position, _attacks[attackNum].AttackEffect.transform.rotation);
            var m = eff.main; m.simulationSpeed = _attacks[attackNum].EffectSpeed;

            Observable.Timer(TimeSpan.FromSeconds(.6f)).Subscribe(z =>
            {
                Destroy(eff.gameObject);
            }).AddTo(this);
                
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

            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(_transform.position, range, results, _layerMask);

            for(int j = 0; j < size; j++)
            {
                var unit = results[j].GetComponent<Unit>();
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
                        unit.UnitMovement.AddImpulse((posTo) * _attacks[i].Knockback);
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
            
            Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(_ =>
            {
                Destroy(p.gameObject);
            }).AddTo(this);
        }
        
    }
}