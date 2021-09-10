using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.Campaign.Ocean
{
    public class OnSelectActions : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private GameObject[] _toEnable;
        [SerializeField] private GameObject[] _toDisable;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var VARIABLE in _toEnable)
            {
                VARIABLE.SetActive(true);
            }
            foreach (var VARIABLE in _toDisable)
            {
                VARIABLE.SetActive(false);
            }
        }
    }
}