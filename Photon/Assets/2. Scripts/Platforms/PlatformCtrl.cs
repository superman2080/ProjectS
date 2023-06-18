using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformView))]
public abstract class PlatformCtrl : MonoBehaviourPunCallbacks
{
    public PhotonView pv;

    protected abstract void OnTouchPlatform(Collision2D collision);

    protected abstract void OnLeavePlatform(Collision2D collision);

    protected abstract void PlatformBehavior();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            OnTouchPlatform(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            OnLeavePlatform(collision);
    }
}
