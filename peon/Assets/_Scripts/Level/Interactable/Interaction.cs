using _Scripts.Level.Interactable.Talants;
using UnityEngine;

namespace _Scripts.Level.Interactable
{
    class Interaction : MonoCached.MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _radius;

        [SerializeField] private TalentWindow _talantWindow;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnTick()
        {
            if (!Input.GetKeyDown(KeyCode.E)) return;
            
            var position = transform.position;
                
            var results = new Collider[10];
            var lenght = Physics.OverlapSphereNonAlloc(position, _radius, results, _layerMask);

            Collider closest = null;
            var min = Mathf.Infinity;
            for (int i = 0; i < lenght; i++)
                if(Vector3.Distance(position, results[i].transform.position) < min)
                    closest = results[i];
                    
            if(closest != null)
            {
                var interactable = closest.GetComponent<Interactable>();
                    
                if(interactable is Talent talent)
                {
                    talent.Interact();
                    _talantWindow.Add(talent.TargetTalent);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
