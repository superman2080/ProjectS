using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shield : ItemCtrl
{
    public SpriteRenderer sR;
    public Sprite shieldImg;
    private bool isProtected;


    public override void ItemEffect()
    {
        if(isProtected)
        {
            owner.pv.RPC(nameof(PlayerCtrl.SetIsInvincible), RpcTarget.AllBuffered, true);
            pv.RPC(nameof(ShieldOff), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void ShieldOff()
    {
        StartCoroutine(ShieldOffCor());
    }

    private IEnumerator ShieldOffCor()
    {
        yield return null;
        owner.pv.RPC(nameof(PlayerCtrl.SetIsInvincible), RpcTarget.AllBuffered, false);
        SetIsProtected(false);
    }

    [PunRPC]
    public void SetIsProtected(bool b)
    {
        isProtected = b;
        sR.enabled = b;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);
        pv.RPC(nameof(SetIsProtected), RpcTarget.AllBuffered, true);
        owner.OnTakenDamage += ItemEvent;
    }
}
