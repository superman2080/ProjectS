using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpeedUp : ItemCtrl
{
    [Range(4f, 10f)]
    public float speed;

    public override void ItemEffect()
    {
        owner.speed = speed;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.OnSpawnPlayer += ItemEvent;
    }
}
