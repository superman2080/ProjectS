using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Vampirism : ItemCtrl
{
    protected PlayerCtrl other;

    [Range(0.1f, 1f)]
    public float vampCoefficient;


    public override void ItemEffect()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (pv.IsMine){
            foreach (var player in players){
                if (!player.GetComponent<PhotonView>().IsMine)
                {
                    other = player.GetComponent<PlayerCtrl>();
                    break;
                }
            }
        }
        other.OnTakenDamage += new EventHandler(OnOtherIsTakenDamage);
    }


    void OnOtherIsTakenDamage(object sender, EventArgs e)
    {
        pv.RPC(nameof(OnVampirism), RpcTarget.AllBuffered);
    }


    [PunRPC]
    void OnVampirism()
    {
        //왠진 모르겠는데 1회 공격 당 흡혈이 3번 발동함....
        float vampAmount = owner.damage * vampCoefficient / 3;

        if (owner.hp != owner.maxHp)
        {
            if (owner.hp + vampAmount >= owner.maxHp) owner.hp = owner.maxHp;
            else  owner.hp += vampAmount;
            owner.hpBar.fillAmount = owner.hp / owner.maxHp;
        }
    }


    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);
        owner.OnSpawnPlayer += ItemEvent;
    }
}