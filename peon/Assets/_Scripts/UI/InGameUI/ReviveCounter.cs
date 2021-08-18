using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Unit;
using _Scripts.Unit.Player;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class ReviveCounter : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Transform _parent;

        [SerializeField] private UnitObserver _unitObserver;

        [SerializeField] private float _disposeSpeed;
        
        private readonly List<Image> _toDispose = new List<Image>();

        private void Start()
        {
            _unitObserver.UnitChanged += OnUnitChanged;

            Observable.EveryUpdate().Subscribe(x =>
            {
                foreach (var image in _toDispose.Where(image => image != null))
                {
                    image.color = UnityEngine.Color.Lerp(image.color, UnityEngine.Color.clear, _disposeSpeed * Time.deltaTime);

                    if (image.color.a <= .02f)
                        Destroy(image.transform.parent.gameObject);
                }
            }).AddTo(this);
        }

        private void OnUnitChanged(Unit.Unit u)
        {
            var h = u.GetComponent<PlayerHealth>();
            if (h == null) throw new Exception("An unhandled exception occured: che delaesh shakal ebaniy");
            
            h.RevivesChanged += OnRevivesChanged;
            OnRevivesChanged(h.CurrentRiveves);
        }

        private void OnRevivesChanged(int i)
        {
            while (_parent.childCount < i)
                Instantiate(_prefab, _parent);
            
            while (_parent.childCount > i)
            {
                var o = _parent.GetChild(0);
                o.SetParent(_parent.parent);
                o.SetSiblingIndex(0);
                
                _toDispose.Add(o.GetComponentInChildren<Image>());
            }
            
            _toDispose.RemoveAll(item => item == null);
        }
    }
}