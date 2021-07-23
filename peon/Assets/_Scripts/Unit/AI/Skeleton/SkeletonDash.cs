using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Unit
{
    class SkeletonDash : MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _dashMaxDetectRange;
        [SerializeField] private float _dashMinDetectRange;

        [SerializeField] private float _dashDamageRange;
        [SerializeField] private float _dashDamage;
        [SerializeField] private ParticleSystem _dashDamageEffect;

        [SerializeField] private float _dashSpeed;

        [SerializeField] private float _timeToPrepare;

        [SerializeField] private float _dashCooldown;
        private float _dashCooldownTimer;

        [SerializeField] private float _stopAnimationTime;

        [SerializeField] private ParticleSystem _dashDamageRangeEffect;

        private Unit _unit;

        private Vector3 _aimPos;
        private ParticleSystem _aimEffect;
        private NavMeshAgent _navMeshAgent;

        private void Start()
        {
            _dashCooldownTimer = 0;

            _unit = GetComponent<Unit>();
            _navMeshAgent = _unit.GetComponent<NavMeshAgent>();

            _aimPos = Vector3.zero;

            InvokeRepeating(nameof(FindEnemy), .5f, .3f);
        }

        private void FindEnemy()
        {
            if (_dashCooldownTimer > Time.time || _aimPos != Vector3.zero || !_unit.enabled) return;

            var objects = Physics.OverlapSphere(transform.position, _dashMaxDetectRange, _layerMask);
            foreach (var obj in objects)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) < _dashMinDetectRange) continue;

                if (obj.GetComponent<Unit>().enabled && !_unit.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                {
                    _aimPos = obj.transform.position;
                    _dashCooldownTimer = Time.time + _dashCooldown;

                    _aimEffect = Instantiate(_dashDamageRangeEffect);
                    _aimEffect.transform.position = _aimPos;

                    StartCoroutine(Dash());

                    break;
                }
            }
        }

        private IEnumerator Dash()
        {
            _unit.Animator.SetTrigger("Attack");

            yield return new WaitForSeconds(_stopAnimationTime);

            _unit.Animator.speed = 0;
            _unit.UnitMovement.enabled = false;
            _navMeshAgent.enabled = false;
            _unit.Rigidbody.isKinematic = false;

            yield return new WaitForSeconds(_timeToPrepare);

            var startTime = Time.time;
            while (!(Vector3.Distance(transform.position, _aimPos) < .01f || Time.time - startTime > 2f || _aimPos == Vector3.zero))
            {
                yield return null;
                transform.position = Vector3.MoveTowards(transform.position, _aimPos, Time.deltaTime * _dashSpeed);
            }

            var ps = _aimEffect;
            ps.Stop();
            _aimEffect = null;
            StartCoroutine(TimedDestroy(1f, ps));

            var eff = Instantiate(_dashDamageEffect);
            eff.transform.position = transform.position;
            eff.Play();
            StartCoroutine(TimedDestroy(1f, eff));

            var objects = Physics.OverlapSphere(transform.position, _dashDamageRange, _layerMask);
            foreach (var obj in objects)
            {
                var unit = obj.GetComponent<Unit>();
                if (unit.enabled)
                {
                    unit.UnitHealth.TakeDamage(_dashDamage);
                }
            }

            _unit.Animator.speed = 1;
            _unit.UnitMovement.enabled = true;
            _navMeshAgent.enabled = true;
            _unit.Rigidbody.isKinematic = true;
            
            _aimPos = Vector3.zero;
        }

        private IEnumerator TimedDestroy(float time, ParticleSystem ps) 
        {
            yield return new WaitForSeconds(time);
            Destroy(ps.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.collider.gameObject != gameObject && collision.collider.name != "TerrainPlane")
                _aimPos = Vector3.zero;  
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _dashDamageRange);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _dashMinDetectRange);
            Gizmos.DrawWireSphere(transform.position, _dashMaxDetectRange);
        }
    }
}
