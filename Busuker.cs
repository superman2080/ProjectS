using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class Busuker : ItemCtrl
{
    [Range(4f, 10f)]
    private float maxSpeed = 7; // 체력이 떨어졌을 때 최대로 올라가는 속도
    private float spIf = 0.1f; // 체력이 감소할 때마다 속도가 올라감
    private float currentSpeed; // 현재 속도
    public override void ItemEffect()
    {
       currentSpeed = owner.speed;
       currentSpeed = maxSpeed + (owner.maxHp - owner.hp) * spIf; // 현재 플레이어 체력이 깎일수록 플레이어 속도가 올라감
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.OnSpawnPlayer += ItemEvent;
    }
}

