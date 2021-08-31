using UniRx;
using UnityEngine;

namespace _Scripts.UI.InGameUI
{
    public class AltHint : MonoBehaviour
    {
        [SerializeField] private GameObject[] _toHide;

        private void Start()
        {
            Observable.EveryUpdate().Where(_ => Input.GetKeyUp(KeyCode.LeftAlt)).Subscribe(x =>
            {
                foreach (var VARIABLE in _toHide)
                {
                    VARIABLE.SetActive(false);
                }
            }).AddTo(this);
            
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.LeftAlt)).Subscribe(x =>
            {
                foreach (var VARIABLE in _toHide)
                {
                    VARIABLE.SetActive(true);
                }
            }).AddTo(this);
        }
    }
}