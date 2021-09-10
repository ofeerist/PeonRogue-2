using UniRx;
using UnityEngine;

namespace _Scripts.UI
{
    public class DarknessGroupTransition : MonoBehaviour
    {
        public float Speed;
        private CanvasGroup _group;
        private bool _dark;
        private void Start()
        {
            _group = GetComponent<CanvasGroup>();
            _dark = true;
            
            Observable.EveryUpdate().Subscribe(x =>
            {
                if (_dark) { if (_group.alpha <= .999f) _group.alpha = Mathf.Lerp(_group.alpha, 1, Speed * Time.deltaTime); }
                else { if (_group.alpha >= .001f) _group.alpha = Mathf.Lerp(_group.alpha, 0, Speed * Time.deltaTime); }
            }).AddTo(this);
        }

        public void ActivateDark()
        {
            _dark = true;
        }

        public void ActivateDarkImmediatly()
        {
            _group.alpha = 1;
            _dark = true;
        }

        public void DeactivateDark()
        {
            _dark = false;
        }

        public void DeactivateDarkImmediatly()
        {
            _group.alpha = 0;
            _dark = false;
        }
    }
}