using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

namespace _Scripts.Level.Doodads
{
    [Serializable]
    internal class DoodadOptions
    {
        public GameObject Object;
        public RandomFloat Scale;
        public RandomFloat Rotation;
    }
    
    public class DoodadFactory : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private bool _showGizmos;
        private void OnDrawGizmos()
        {
            if (!_showGizmos) return;
            
            var _transform = transform;
            Gizmos.color = UnityEngine.Color.white;
            for (int i = 0; i < _transform.childCount; i++)
            {
                Gizmos.DrawWireSphere(_transform.GetChild(i).position, .1f);
            }
        }

        [Space] [SerializeField] private DoodadOptions[] _doodadOptions;
        
        private void Start()
        {
            if(!PhotonNetwork.IsMasterClient) return;

            var _transform = transform;
            var random = new Random();
            
            for (int i = 0; i < _transform.childCount; i++)
            {
                var index = random.Next(0, _doodadOptions.Length);
                
                PhotonNetwork.Instantiate(_doodadOptions[index].Object.name, _transform.GetChild(i).position,
                    Quaternion.Euler(0, _doodadOptions[index].Rotation.GetValue(), 0), 0, new object[]{_doodadOptions[index].Scale.GetValue()});
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            float scale = (float)info.photonView.InstantiationData[0];
            info.photonView.gameObject.transform.localScale *= scale;
        }
    }
}