namespace _Scripts.Unit
{
    class UniversalHealth : UnitHealth
    {
        public override void TakeDamage(float damage)
        {
            CurrentHealth -= damage;

            if (CurrentHealth <= 0) Death();
        }

        private void Death()
        {
            Unit.Animator.SetBool("Dead", true);
            Unit.enabled = false;
            Unit.Controller.enabled = false;
        }
    }
}
