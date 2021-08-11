using System;
using System.Collections.Generic;
using _Scripts.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.InGameUI
{
    public class ReviveCounter : MonoCached.MonoCached
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Transform _parent;

        [SerializeField] private UnitObserver _unitObserver;

        [SerializeField] private float _disposeSpeed;
        
        private List<Image> _toDispose = new List<Image>();

        private void Start()
        {
            _unitObserver.UnitChanged += OnUnitChanged;
        }

        protected override void OnTick()
        {
            foreach (var image in _toDispose)
            {
                if (image == null) continue;
                
                image.color = UnityEngine.Color.Lerp(image.color, UnityEngine.Color.clear, _disposeSpeed * Time.deltaTime);

                if (image.color.a <= .02f)
                    Destroy(image.transform.parent.gameObject);
                
            }
        }

        private void OnUnitChanged(Unit.Unit u)
        {
            var h = u.GetComponent<UniversalHealth>();
            if (h == null) throw new Exception("An unhandled exception occured: che delaesh shakal ebaniy");
            
            h.RevivesChanged += OnRevivesChanged;
        }

        private void OnRevivesChanged(int i)
        {
            while (_parent.childCount < i)
            {
                Instantiate(_prefab, _parent);
            }

            while (_parent.childCount > i)
            {
                var o = _parent.GetChild(0);
                o.SetParent(_parent.parent);
                o.SetSiblingIndex(0);
                //if(_parent.childCount != 0) o.GetChild(1).gameObject.SetActive(false);
                _toDispose.Add(o.GetComponentInChildren<Image>());
            }
            
            _toDispose.RemoveAll(item => item == null);
        }
    }
}