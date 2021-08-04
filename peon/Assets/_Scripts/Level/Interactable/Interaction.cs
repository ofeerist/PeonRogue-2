using System;
using System.Collections;
using _Scripts.Level.Interactable.Talents;
using _Scripts.UI.InGameUI;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Level.Interactable
{
    internal class Interaction : MonoCached.MonoCached
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _radius;

        [SerializeField] private TalentWindow _talantWindow;

        [SerializeField] private Image _image;

        [SerializeField] private UnitObserver _observer;

        [SerializeField] private Animator _arrow;
        private static readonly int Birth = Animator.StringToHash("Birth");
        private static readonly int Death = Animator.StringToHash("Death");

        private void Start()
        {
            _arrow = Instantiate(_arrow, transform);
            _arrow.gameObject.SetActive(false);
        }

        protected override void OnTick()
        {
            if (_observer.Unit == null) return;

            var position = _observer.Unit.transform.position;

            var results = new Collider[10];
            var count = Physics.OverlapSphereNonAlloc(position, _radius, results, _layerMask);

            _image.enabled = count > 0;

            Collider closest = null;
            var min = Mathf.Infinity;
            for (int i = 0; i < count; i++)
                if (Vector3.Distance(position, results[i].transform.position) < min)
                    closest = results[i];
            
            var arrowActive = _arrow.gameObject.activeSelf;
            if (closest != null)
            {
                Interactable interactable;
                if (!arrowActive)
                {
                    interactable = closest.GetComponent<Interactable>();
                    _arrow.gameObject.SetActive(true);
                    _arrow.SetTrigger(Birth);
                    _arrow.transform.position = interactable.ArrowPosition.GetPosition();
                }

                if (!Input.GetKeyDown(KeyCode.E)) return;
                
                interactable = closest.GetComponent<Interactable>();
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
            else
            {
                if(arrowActive)
                    StartCoroutine(DisableArrow());
            }
        }

        private IEnumerator DisableArrow()
        {
            _arrow.SetTrigger(Death);
            yield return new WaitForSeconds(.330f);
            _arrow.gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
