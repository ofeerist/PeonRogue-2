using System;
using System.Collections;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        private static readonly int Dead1 = Animator.StringToHash("Dead");

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

        
        [SerializeField] private int _maxRevives;
        [SerializeField] private int _startRevives;
        private int _currentRevives;
        private Unit _unit;

        public int CurrentRiveves
        {
            get => _currentRevives;

            protected set
            {
                _currentRevives = value < _maxRevives ? value : _maxRevives;
                RevivesChanged?.Invoke(value);
            }
        }

        public delegate void IntChanged(int i);
        public event IntChanged RevivesChanged;
        
        private void Start()
        {
            _unit = GetComponent<Unit>();
            
            CurrentHealth = MaxHealth;
            CurrentRiveves = _startRevives;
        }
        
        [PunRPC]
        public void TakeDamage(float damage)
        {
            if (!_unit.enabled) return;
            
            CurrentHealth -= damage;

            if (CurrentHealth <= 0) Death();
        }

        private void Death()
        {
            _unit.Animator.SetBool(Dead1, true);
            _unit.CurrentState = UnitState.Dead;

            if (_currentRevives > 0)
            {
                CurrentRiveves -= 1;
                
                Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe (_ => { 
                    
                    CurrentHealth = MaxHealth;
                    _unit.Animator.SetBool(Dead1, false);
                    _unit.CurrentState = UnitState.Default;
                    
                }).AddTo (this);
            }
        }


    }
}
