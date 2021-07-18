using UnityEngine;

namespace Game.Unit
{
    class UniversalHealth : UnitHealth
    {
        public override void TakeDamage(float damage)
        {
            _currentHealth -= damage;

            if (_currentHealth <= 0) Death();
        }

        private void Death()
        {
            Unit.Animator.SetBool("Dead", true);
            Unit.enabled = false;
            Unit.Controller.enabled = false;
        }
    }
}
