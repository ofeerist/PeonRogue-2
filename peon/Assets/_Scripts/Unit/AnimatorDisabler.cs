using System;
using UniRx;
using UnityEngine;

namespace _Scripts.Unit
{
    class AnimatorDisabler : MonoBehaviour
    {
        private Animator _animator;
        [SerializeField] private GameObject _toDestroy;
        [SerializeField] private ParticleSystem _destroyEffect;
        
        private void Start()
        {
            _animator = GetComponentInParent<Animator>();
        }
        private void OnBecameInvisible()
        {
            _animator.enabled = false;
        }

        private void OnBecameVisible()
        {
            _animator.enabled = true;
        }

        public void Disable()
        {
            _animator.enabled = false;
            
            Observable.Timer(TimeSpan.FromSeconds(_destroyEffect.main.duration)).Subscribe(x =>
            {
                Destroy(_toDestroy);
            }).AddTo(this);
        }

        public void Effect()
        {
            var eff = Instantiate(_destroyEffect, _toDestroy.transform);
            eff.Play();
        }
    }
}
