using Game.Level._Interactable._Talents;
using UnityEngine;

namespace Game.Level._Interactable
{
    class Interaction : MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _radius;

        [SerializeField] private TalentWindow _talantWindow;

        protected override void OnTick()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                var position = transform.position;
                
                Collider[] results = null;
                Physics.OverlapSphereNonAlloc(position, _radius, results, _layerMask);

                Collider closest = null;
                float min = Mathf.Infinity;
                for (int i = 0; i < results.Length; i++)
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
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
