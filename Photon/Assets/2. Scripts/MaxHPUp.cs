using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MaxHPUp : ItemCtrl
{
    [Range(100f, 200f)]
    public float maxHP;

    [PunRPC]
    public override void OnGetItem(int actorNum, string itemName)
    {
        base.OnGetItem(actorNum, itemName);
        owner.maxHp = maxHP;
        owner.hpBar.color = Color.magenta;
        Debug.LogError(owner.maxHp);
    }
}
