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
        if (IsContainingItem(nameof(Busuker)))
        {
            Debug.LogError("Exist busuker");
            ItemCtrl busurker = owner.itemList.Find((n) => n is Busuker) as Busuker;
            busurker.pv.RPC(nameof(Busuker.SetOriginValues), RpcTarget.All, owner.speed, owner.damage);
        }
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.OnSpawnPlayer += ItemEvent;
    }
}
