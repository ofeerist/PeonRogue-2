using UnityEngine;
using System.Collections;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Game.Unit
{
    class Slam : MonoCached, IOnEventCallback
    {
        [SerializeField] private Transform _attackPosition;

        [Space]

        [SerializeField] private float _damage;
        [SerializeField] private float _knockback;
        [SerializeField] private float _attackRadius;
        [SerializeField] private float _attackCooldown;
        private float _attackCooldownTimer;

        [Space]

        [SerializeField] private int _maxCharges;
        [SerializeField] private int _chargeRegenerateTime;
        private int _currentCharges;
        public int CurrentCharges
        {
            get
            {
                return _currentCharges;
            }
            private set
            {
                _currentCharges = value;
                ChargeChanged?.Invoke(value);
            }
        }

        public delegate void ValueChanged(int charges);
        public event ValueChanged ChargeChanged;

        [Space]

        [SerializeField] private float _inAttackDuration;

        [Space]

        [SerializeField] private ParticleSystem _slam;
        [SerializeField] private float _slamDelay;

        private Unit _unit;
        private Animator _animator;
        private PhotonView _photonView;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clap;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _attackRadius);
        }

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _animator = _unit.Animator;
            _photonView = _unit.PhotonView;

            _attackCooldownTimer = Time.time;

            CurrentCharges = _maxCharges;
            InvokeRepeating(nameof(RegenerateCharge), 0, _chargeRegenerateTime);
        }

        private void RegenerateCharge()
        {
            if(_currentCharges < _maxCharges)
                CurrentCharges += 1;
        }

        protected override void OnTick()
        {
            if (!_photonView.IsMine) return;

            if (Input.GetKeyDown(KeyCode.Q) && CurrentCharges > 0 && _attackCooldownTimer <= Time.time && !_unit.UnitAttack.InAttack)
            {
                SlamAttack();
            }
        }

        private void SlamAttack()
        {
            _unit.UnitAttack.InAttack = true;

            _attackCooldownTimer = _attackCooldown + Time.time;
            CurrentCharges -= 1;

            _animator.SetInteger("AttackNum", 2);
            _animator.SetTrigger("Attack");

            _unit.PhotonView.RPC(nameof(PlayEffect), RpcTarget.All);

            StartCoroutine(DelayedDamage(_slamDelay));
            StartCoroutine(AttackDuration(_inAttackDuration));
        }

        [PunRPC]
        private void PlayEffect()
        {
            var slam = Instantiate(_slam);
            StartCoroutine(DelayedPlay(slam, _slamDelay));
            StartCoroutine(DestroyOnEnd(slam));
        }

        private IEnumerator DelayedPlay(ParticleSystem ps, float time)
        {
            yield return new WaitForSeconds(time);
            ps.transform.position = _attackPosition.position;
            ps.Play();

            _audioSource.PlayOneShot(_clap);
        }

        private IEnumerator DelayedDamage(float time)
        {
            yield return new WaitForSeconds(time);
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte)PhotonEvent.Event.SlamDamage, null, options, sendOptions);
        }

        private void DoDamage()
        {
            var objects = Physics.OverlapSphere(_attackPosition.position, _attackRadius);
            foreach(var obj in objects)
            {
                var unit = obj.GetComponent<Unit>();
                if (unit != null && !unit.CompareTag("Player") && unit.UnitHealth != null && unit != _unit && obj.GetComponent<Unit>().enabled)
                {
                    var posTo = (unit.transform.position - _attackPosition.position).normalized;

                    unit.UnitHealth.TakeDamage(_damage);

                    posTo.y = 0;
                    unit.UnitMovement.AddImpulse((posTo) * _knockback);
                }
            }
        }

        private IEnumerator DestroyOnEnd(ParticleSystem ps)
        {
            yield return new WaitUntil(() => ps.isPlaying);
            yield return new WaitUntil(() => !ps.isPlaying);
            Destroy(ps.gameObject);
        }

        private IEnumerator AttackDuration(float time)
        {
            yield return new WaitForSeconds(time);
            _unit.UnitAttack.InAttack = false;
        }

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
                case (byte)PhotonEvent.Event.SlamDamage:
                    DoDamage();
                    break;

                default:
                    break;
            }
        }
    }
}
