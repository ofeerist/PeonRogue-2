using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace _Scripts.Unit.AI
{
    class AIMovement : UnitMovement
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _detectionRange;

        private Unit _target;
        private NavMeshAgent _navMeshAgent;

        private PhotonView _photonView;
        
        public float BounceDamage;
        private TextTag.TextTag _textTag;
        private static readonly int Speed1 = Animator.StringToHash("Speed");

        public void SetData(float detectionRange, float rotateSpeed, float speed)
        {
            _detectionRange = detectionRange;
            _rotateSpeed = rotateSpeed;
            Speed = speed;
        }

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _photonView = GetComponent<PhotonView>();

            _navMeshAgent.speed = Speed;
            _navMeshAgent.angularSpeed = _rotateSpeed;

            InvokeRepeating(nameof(UpdateAnim), 0, .1f);
            InvokeRepeating(nameof(FindTarget), 0, .5f);
        }
        
        private void UpdateAnim()
        {
            if (!Unit.enabled) return;

            if (!Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                Unit.Animator.SetFloat(Speed1, _navMeshAgent.velocity.magnitude);
            else
                Unit.Animator.SetFloat(Speed1, 0);
        }

        private void FindTarget()
        {
            if (!PhotonNetwork.IsMasterClient) return;
        
            if (!Unit.enabled) return;

            var _transform = transform;
            DetectEnemy(_transform);

            if (Unit.enabled && _navMeshAgent.enabled)
            {
                var position = BlockMovement ? _transform.position : (_target != null ? _target.transform.position : Vector3.zero);

                if (_target == null) return;

                _photonView.RPC(nameof(SetDestination), RpcTarget.AllViaServer, position.x, position.y, position.z);
            }

            if (Unit.Rigidbody.velocity.magnitude <= 0.1f)
            {
                _navMeshAgent.enabled = true;
                Unit.Rigidbody.isKinematic = true;
            }
            else
            {
                _navMeshAgent.enabled = false;
            }
        }

        [PunRPC]
        private void SetDestination(float x, float y, float z)
        {
            OnTick();

            if (BlockMovement) return;
            
            var position = new Vector3(x, y, z);
            if(_navMeshAgent.enabled) _navMeshAgent.SetDestination(position);
        }
        
        protected override void OnTick()
        {
            if (Unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") || !Unit.enabled || Unit.UnitHealth.InStan) BlockMovement = true;
            else BlockMovement = false;
        }

        public override void AddImpulse(Vector3 direction, bool stan = true, float stanTime = 1f)
        {
            _navMeshAgent.enabled = false;
            Unit.Rigidbody.isKinematic = false;
            base.AddImpulse(direction, stan, stanTime);
        }

        private void DetectEnemy(Transform _transform)
        {
            if (_target == null || Vector3.Distance(_transform.position, _target.transform.position) > _detectionRange || !_target.enabled)
            {
                _target = null;

                var results = new Collider[10];
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _detectionRange, results, _layerMask);

                Collider closest = null;
                var minDistance = Mathf.Infinity;
                for (int i = 0; i < size; i++)
                {
                    if (Vector3.Distance(_transform.position, results[i].transform.position) < minDistance)
                    {
                        closest = results[i];
                    }
                }

                if(closest != null) _target = closest.GetComponent<Unit>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.red;

            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 9 && !_navMeshAgent.enabled && enabled)
            {
                Unit.UnitHealth.TakeDamage(BounceDamage);

                var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));

                if (_textTag == null)
                    _textTag = TextTag.TextTag.Create(transform.position + randomOffset, "Столкновение!", UnityEngine.Color.gray, 1, new Vector3(0, .005f), false, 0.2f);
                else
                {
                    _textTag.transform.position = transform.position + randomOffset;
                    _textTag.Color = UnityEngine.Color.gray;
                }
            }
        }
    }
}
