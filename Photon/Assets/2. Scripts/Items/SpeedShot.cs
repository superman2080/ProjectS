using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedShot : ItemCtrl
{
    public float attSpeedMag;
    public float attDamageMag;

    public override void ItemEffect()
    {
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.attMag = attSpeedMag;
        owner.damage *= attDamageMag;

        if (IsContainingItem(nameof(Busuker)))
        {
            Debug.LogError("Exist busuker");
            ItemCtrl busurker = owner.itemList.Find((n) => n is Busuker) as Busuker;
            busurker.pv.RPC(nameof(Busuker.SetOriginValues), RpcTarget.All, owner.speed, owner.damage);
        }
    }
}
