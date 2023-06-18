using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExtinctionPlatform : PlatformCtrl
{
    public float extinctionTime;
    
    private Coroutine nowCor;
    private SpriteRenderer sR;
    private BoxCollider2D col;

    void Start()
    {
        sR = gameObject.GetComponent<SpriteRenderer>();
        col = gameObject.GetComponent<BoxCollider2D>();
    }

    protected override void OnLeavePlatform(Collision2D collision)
    {
    }

    protected override void OnTouchPlatform(Collision2D collision)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount != 2)
            return;
        else
        {
            if (nowCor == null)
                nowCor = StartCoroutine(ExtinctionPlatformBehavior(extinctionTime));
        }
    }

    protected override void PlatformBehavior()
    {
    }

    private IEnumerator ExtinctionPlatformBehavior(float eT)
    {
        for (float i = 1; i <= 10; i++)
        {
            pv.RPC(nameof(SetAlpha), RpcTarget.All, ((10f - i) * 0.1f));
            yield return new WaitForSeconds(eT / 10f);
        }
        col.enabled = false;
    }

    [PunRPC]
    public void SetAlpha(float a) => sR.color = new Color(sR.color.r, sR.color.g, sR.color.b, a);
}
