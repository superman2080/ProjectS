using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MaxHPUp : ItemCtrl
{
    [Range(100f, 200f)]
    public float maxHP;

    public override void ItemEffect()
    {
        owner.maxHp = maxHP;
        owner.hpBar.color = Color.magenta;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum, string itemName)
    {
        base.OnGetItem(actorNum, itemName);

        owner.OnSpawnPlayer += ItemEvent;
    }
}
