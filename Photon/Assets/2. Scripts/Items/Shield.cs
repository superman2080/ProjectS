using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : ItemCtrl
{
    private bool isProtected;

    public override void ItemEffect()
    {
    }

    public override void OnGetItem(int actorNum, string itemName)
    {
        base.OnGetItem(actorNum, itemName);

    }
}
