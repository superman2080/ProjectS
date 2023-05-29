using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MaxHPUp : ItemCtrl
{
    [PunRPC]
    public override void OnGetItem(int actorNum, string itemName)
    {
        if (!pv.IsMine)
            return;
        base.OnGetItem(actorNum, itemName);
        Debug.LogError(owner.name + "MaxHPUp!!");
    }
}
