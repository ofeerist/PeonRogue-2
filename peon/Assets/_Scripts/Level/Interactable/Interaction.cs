using _Scripts.Level.Interactable.Talents;
using _Scripts.UI.InGameUI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Level.Interactable
{
    class Interaction : MonoCached.MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _radius;

        [SerializeField] private TalentWindow _talantWindow;

        [SerializeField] private Image _image;

        [SerializeField] private UnitObserver _observer;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnTick()
        {
            if (_observer.Unit == null) return;
            
            var position = _observer.Unit.transform.position;
            
            var results = new Collider[10];
            var count = Physics.OverlapSphereNonAlloc(position, _radius, results, _layerMask);

            _image.enabled = count > 0;
            
            if (!Input.GetKeyDown(KeyCode.E)) return;

            Collider closest = null;
            var min = Mathf.Infinity;
            for (int i = 0; i < count; i++)
                if(Vector3.Distance(position, results[i].transform.position) < min)
                    closest = results[i];
                    
            if(closest != null)
            {
                var interactable = closest.GetComponent<Interactable>();
                    
                switch (interactable)
                {
                    case Talent talent:
                        talent.Interact();
                        _talantWindow.Add(talent.TargetTalent);
                        break;
                    default:
                        interactable.Interact();
                        break;
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
