using System.Collections;
using UnityEngine;

namespace _Scripts.Unit
{
    public class UnitHealth : MonoCached.MonoCached
    {
        [HideInInspector] public Unit Unit;

        public bool InStan { get; protected set; }

        [SerializeField] private float _maxHealth;
        protected float _currentHealth;

        [SerializeField] private float _regenPerSecond;
        public float Regen {
            get => _regenPerSecond;
            protected set
            {
                _regenPerSecond = value;
                RegenChanged?.Invoke(_regenPerSecond);
            } 
        }
        
        public delegate void ValueChanged(float value);
        public event ValueChanged MaxHealthChanged;
        public event ValueChanged HealthChanged;
        public event ValueChanged RegenChanged;
        
        public float MaxHealth {
            get => _maxHealth;
            protected set
            {
                _maxHealth = value;
                MaxHealthChanged?.Invoke(value);
            } 
        }
        
        public float CurrentHealth {
            get => _currentHealth;
            protected set
            {
                _currentHealth = value < 0 ? 0 : value;
                HealthChanged?.Invoke(_currentHealth);
            } 
        }
        
        public delegate void Dead(Unit u);
        public virtual event Dead OnDeath;

        private void Start()
        {
            CurrentHealth = _maxHealth;
        }

        private void Update()
        {
            if (CurrentHealth + _regenPerSecond * Time.deltaTime <= MaxHealth)
                CurrentHealth += _regenPerSecond * Time.deltaTime;
            else
                CurrentHealth = MaxHealth;
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