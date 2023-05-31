using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpeedUp : ItemCtrl
{
    [Range(100f, 200f)]
    public float speed;

    public override void ItemEffect()
    {
        owner.speed = speed;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum, string itemName)
    {
        base.OnGetItem(actorNum, itemName);
        owner.OnSpawnPlayer += ItemEvent;
    }
}
