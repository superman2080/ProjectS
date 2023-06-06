using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SizeDown : ItemCtrl
{
    private Transform canvas;
    //private CapsuleCollider2D collider;
    //static bool isMediateY = false;

    [Range(0.6f, 0.8f)]
    public float size;

    public override void ItemEffect()
    {
        //collider = owner.GetComponent<CapsuleCollider2D>();

        //collider.size = new Vector3(size, size, 1f);
        //owner.spriteRenderer.transform.localScale = new Vector3(size, size, 1f);
        //owner.transform.Find("GunTr").transform.localScale = new Vector3(size, size, 1);
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);
        owner.transform.localScale = Vector3.one * size;

        canvas = owner.transform.Find("Canvas");
        canvas.transform.localScale = Vector3.one / size;
        canvas.transform.position += new Vector3(0f, 0.134f, 0f);
    }
}
