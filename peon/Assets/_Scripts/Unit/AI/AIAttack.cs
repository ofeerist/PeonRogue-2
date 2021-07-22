using UnityEngine;
using UnityEngine.AI;

namespace Game.Unit
{
    class AIAttack : UnitAttack
    {
        [SerializeField] private float _range;
        [SerializeField] private float _angle;
        [SerializeField] private float _damage;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _preAttack;
        [SerializeField] private AudioClip[] _hit;

        private float _attackCooldown;
        private bool _inAttack;

        protected override void OnTick()
        {
            var currentStateInfo = Unit.Animator.GetCurrentAnimatorStateInfo(0);
            if (!currentStateInfo.IsTag("Attack"))
            {
                if (_attackCooldown + _Speed <= Time.time && Unit.enabled)
                {
                    var objects = Physics.OverlapSphere(transform.position, _range);
                    foreach (var obj in objects)
                    {
                        if (obj.CompareTag("Player") && obj.GetComponent<Unit>().enabled && !_inAttack)
                        {
                            Unit.UnitMovement.BlockMovement = true;
                            _inAttack = true;
                            InAttack = true;
                            Attack();
                        }
                    }
                }
            }
            else
            {
                if (currentStateInfo.normalizedTime >= .3f && InAttack)
                {
                    InAttack = false;

                    DoDamage(_damage);
                }
                if (currentStateInfo.normalizedTime >= .7f && _inAttack)
                {
                    _inAttack = false;

                    _attackCooldown = Time.time;
                }
            }
        }

        private void Attack()
        {
            Unit.Animator.SetTrigger("Attack");
            _audioSource.PlayOneShot(_preAttack[Random.Range(0, _preAttack.Length)]);
        }

        private void DoDamage(float damage)
        {
            var objects = Physics.OverlapSphere(transform.position, _range);
            int damaged = 0;
            foreach (var obj in objects)
            {
                if (obj.CompareTag("Player"))
                {
                    var unit = obj.GetComponent<Unit>();
                    var _transform = transform;

                    var posTo = (unit.transform.position - _transform.position).normalized;
                    var dot = Vector3.Dot(posTo, _transform.forward);
                    if (dot >= Mathf.Cos(_angle))
                    {
                        unit.UnitHealth.TakeDamage(damage);
                        damaged++;
                        _audioSource.PlayOneShot(_hit[Random.Range(0, _hit.Length)]);
                    }
                }
            }

            if(damaged == 0)
            {
                var randomOffset = new Vector3(Random.Range(-.1f, .1f), Random.Range(-.1f, .1f), Random.Range(-.1f, .1f));
                TextTag.TextTag.Create(transform.position + randomOffset, "Промах!", Color.red, 1, new Vector3(0, .005f), 0.3f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var _transform = transform;

            Gizmos.color = Color.red;

            var vectorTo1 = new Vector3(_transform.position.x + _range * Mathf.Sin((_angle + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((_angle + _transform.eulerAngles.y) * Mathf.Deg2Rad));
            Gizmos.DrawLine(_transform.position, vectorTo1);

            var vectorTo2 = new Vector3(_transform.position.x + _range * Mathf.Sin((-_angle + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((-_angle + _transform.eulerAngles.y) * Mathf.Deg2Rad));
            Gizmos.DrawLine(_transform.position, vectorTo2);

            for (float j = 1; j <= 10; j += .2f)
            {
                vectorTo1 = new Vector3(_transform.position.x + _range * Mathf.Sin((_angle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((_angle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                vectorTo2 = new Vector3(_transform.position.x + _range * Mathf.Sin((_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(vectorTo1, vectorTo2);

                vectorTo1 = new Vector3(_transform.position.x + _range * Mathf.Sin((-_angle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((-_angle / j + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                vectorTo2 = new Vector3(_transform.position.x + _range * Mathf.Sin((-_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((-_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                Gizmos.DrawLine(vectorTo1, vectorTo2);

                if (j + .2f > 10)
                {
                    vectorTo1 = new Vector3(_transform.position.x + _range * Mathf.Sin((_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    vectorTo2 = new Vector3(_transform.position.x + _range * Mathf.Sin((-_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad), _transform.position.y, _transform.position.z + _range * Mathf.Cos((-_angle / (j + .2f) + _transform.eulerAngles.y) * Mathf.Deg2Rad));
                    Gizmos.DrawLine(vectorTo1, vectorTo2);
                }
            }
        }
    }
}
