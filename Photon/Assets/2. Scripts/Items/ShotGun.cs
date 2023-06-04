using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class ShotGun : ItemCtrl
{
    [Range(2, 5)]
    public int bulletNum;
    [Range(3, 7)]
    public int interval;
    public EventHandler ShotGunEvent;

    public override void ItemEffect()
    {
        for (int i = 0; i < bulletNum; i++)
        {
            owner.CreateBullet(owner.damage, owner.gunAngle + (-interval * (bulletNum - 1) / 2 + i * interval));
        }
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);
        owner.ClearAttackEvent();
        owner.OnPlayerAttack += ItemEvent;
    }
}
