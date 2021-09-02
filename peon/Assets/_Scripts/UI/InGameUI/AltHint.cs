using UniRx;
using UnityEngine;

namespace _Scripts.UI.InGameUI
{
    public class AltHint : MonoBehaviour
    {
        [SerializeField] private GameObject[] _toHide;

        private void Start()
        {
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.LeftAlt)).Subscribe(x =>
            {
                foreach (var VARIABLE in _toHide)
                {
                    VARIABLE.SetActive(true);
                }
            }).AddTo(this);
            
            Observable.EveryUpdate().Where(_ => !Input.GetKey(KeyCode.LeftAlt)).Subscribe(x =>
            {
                if (_toHide[0].activeSelf)
                {
                    foreach (var VARIABLE in _toHide)
                    {
                        VARIABLE.SetActive(false);
                    }
                }
            }).AddTo(this);
        }
    }
}