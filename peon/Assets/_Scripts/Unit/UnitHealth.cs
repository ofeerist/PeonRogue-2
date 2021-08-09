using System.Collections;
using UnityEngine;

namespace _Scripts.Unit
{
    public class UnitHealth : MonoCached.MonoCached
    {
        [HideInInspector] public Unit Unit;

        public bool InStan { get; protected set; }

        [SerializeField] public float _maxHealth;
        protected float _currentHealth;
        
        public delegate void ValueChanged(float value);
        public event ValueChanged OnValueChanged;
        
        public float CurrentHealth {
            get => _currentHealth;
            protected set
            {
                _currentHealth = value;
                OnValueChanged?.Invoke(_currentHealth);
            } }
        
        public delegate void Dead(Unit u);
        public virtual event Dead OnDeath;

        private void Start()
        {
            CurrentHealth = _maxHealth;
        }

        private void Update()
        {

        }

        public virtual void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
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