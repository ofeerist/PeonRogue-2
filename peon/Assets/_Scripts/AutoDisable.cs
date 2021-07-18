using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AutoDisable : MonoBehaviour
{
    private PhotonView _photonView;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
    }
    void Update()
    {
        if (!_photonView.IsMine) gameObject.SetActive(false);
    }
}
