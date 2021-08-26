using System;
using System.Collections;
using _Scripts.Level.Interactable.Talents;
using _Scripts.UI.InGameUI;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Level.Interactable
{
    internal class Interaction : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] private float _radius;

        [SerializeField] private TalentWindow _talantWindow;

        [SerializeField] private Image _image;

        [SerializeField] private UnitObserver _observer;

        [SerializeField] private Animator _arrow;
        private static readonly int Birth = Animator.StringToHash("Birth");
        private static readonly int Death = Animator.StringToHash("Death");

        private PhotonView _photonView;

        private readonly SerialDisposable _serialDisposable = new SerialDisposable();

        private void Awake()
        {
            _serialDisposable.AddTo(this);
        }

        private void Start()
        {
            _photonView = GetComponent<PhotonView>();
            
            _arrow = Instantiate(_arrow, transform);
            _arrow.gameObject.SetActive(false);

            Observable.EveryUpdate().Subscribe(x =>
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
                    _photonView.RPC(nameof(E), RpcTarget.AllViaServer, interactable.PhotonView.ViewID);
                }
                else
                {
                    if (arrowActive)
                    {
                        _arrow.SetTrigger(Death);

                        _serialDisposable.Disposable =
                            Observable.Timer(TimeSpan.FromSeconds(.33f)).Subscribe(h =>
                                _arrow.gameObject.SetActive(false));
                    }
                }
            }).AddTo(this);
        }

        [PunRPC]
        private void E(int id)
        {
            var interactable = PhotonView.Find(id).GetComponent<Interactable>();

            if (interactable == null) return;
            
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
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.white;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
