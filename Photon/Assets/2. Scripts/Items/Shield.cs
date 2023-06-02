using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shield : ItemCtrl
{
    public SpriteRenderer sR;
    public Sprite shieldImg;
    [Range(1, 10)]
    public int shieldCnt;
    private bool isProtected;


    public override void ItemEffect()
    {
        if(shieldCnt > 0)
        {
            owner.pv.RPC(nameof(PlayerCtrl.SetIsInvincible), RpcTarget.AllBuffered, true);

            pv.RPC(nameof(InitShield), RpcTarget.AllBuffered, --shieldCnt);
        }
        else
        {
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
    }

    [PunRPC]
    public void InitShield(int n)
    {
        shieldCnt = n;
        sR.enabled = n > 0;
        isProtected = n > 0;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);
        pv.RPC(nameof(InitShield), RpcTarget.AllBuffered, shieldCnt);
        owner.OnTakenDamage += ItemEvent;
    }
}
