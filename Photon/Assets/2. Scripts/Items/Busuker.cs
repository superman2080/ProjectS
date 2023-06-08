using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class Busuker : ItemCtrl
{


    [Header("Busuker increase magnitude")]
    [Range(1f, 3f)]
    public float magnitude;
 
    private float originDamage;
    private float originSpeed;

    public override void ItemEffect()
    {
        StartCoroutine(BusukerEffect());
    }

    private IEnumerator BusukerEffect()
    {
        yield return null;
        owner.speed = originSpeed + ((1 - (owner.hp / owner.maxHp)) * (magnitude - 1) * originSpeed);
        owner.damage = originDamage + ((1 - (owner.hp / owner.maxHp)) * (magnitude - 1) * originDamage);

        if (owner.hp <= 0)
        {
            Debug.LogError("!");
            owner.damage = originDamage;
            owner.speed = originSpeed;
        }
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        originSpeed = owner.speed;
        originDamage = owner.damage;
        owner.OnTakenDamage += ItemEvent;

  
    }
}

