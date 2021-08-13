using System;
using System.Collections;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit.Player
{
    public class PlayerHealth : UnitHealth
    {
        private static readonly int Dead1 = Animator.StringToHash("Dead");

        [SerializeField] private int _maxRevives;
        [SerializeField] private int _startRevives;
        private int _currentRevives;
            
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
            CurrentHealth = MaxHealth;
            CurrentRiveves = _startRevives;
        }
        
        [PunRPC]
        public override void TakeDamage(float damage)
        {
            if (!Unit.enabled) return;
            
            CurrentHealth -= damage;

            if (CurrentHealth <= 0) Death();
        }

        private void Death()
        {
            Unit.Animator.SetBool(Dead1, true);
            Unit.enabled = false;
            Unit.Controller.enabled = false;

            if (_currentRevives > 0)
            {
                CurrentRiveves -= 1;
                
                Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe (_ => { 
                    
                    CurrentHealth = MaxHealth;
                    Unit.Animator.SetBool(Dead1, false);
                    Unit.enabled = true;
                    Unit.Controller.enabled = true;
                    
                }).AddTo (this);
            }
        }


    }
}
