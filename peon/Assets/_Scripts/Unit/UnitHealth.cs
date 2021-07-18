using System.Collections;
using UnityEngine;

namespace Game.Unit
{
    public class UnitHealth : MonoBehaviour
    {
        [HideInInspector] public Unit Unit;

        public bool InStan { get; protected set; }

        [SerializeField] public float _maxHealth;
        protected float _currentHealth;
        private void Start()
        {
            _currentHealth = _maxHealth;
        }

        private void Update()
        {

        }

        public virtual void TakeDamage(float damage)
        {
            _currentHealth -= damage;
        }

        public virtual void Stan(float time)
        {
            InStan = true;
            StartCoroutine(UncheckStan(time));
        }

        private IEnumerator UncheckStan(float time)
        {
            yield return new WaitForSeconds(time);
            InStan = false;
        }
    }
}