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

        owner.transform.localScale = new Vector3(size, size, 1f);

        canvas = owner.transform.Find("Canvas");
        canvas.transform.localScale = new Vector3(1f, 1f, 1f) * 1 / size;
        canvas.transform.position += new Vector3(0f, 0.134f, 0f);

    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.OnSpawnPlayer += ItemEvent;
    }
}
