using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public abstract class ItemCtrl : MonoBehaviourPunCallbacks
{
    [SerializeField]                //디버깅용
    protected PlayerCtrl owner;
    public PhotonView pv;
    public EventHandler ItemEvent;
    private void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
    }

    [PunRPC]
    public virtual void OnGetItem(int actorNum, string itemName)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if(player.GetComponent<PlayerCtrl>().actorNum == actorNum)
            {
                owner = player.GetComponent<PlayerCtrl>();
                break;
            }
        }
        ItemCtrl item = PhotonNetwork.Instantiate("Items/" + itemName, owner.transform.position, Quaternion.identity).GetComponent<ItemCtrl>();
        item.owner = owner;
        item.transform.parent = owner.itemTr;

        ItemEvent += (sender, e) => { ItemEffect(); };
        owner.itemList.Add(item);
        owner.pv.RPC(nameof(PlayerCtrl.Respawn), RpcTarget.AllBuffered);
    }

    public abstract void ItemEffect();

    protected bool IsContainingItem(string itemName)
    {
        //아이템 존재 체크 코드
        foreach (var it in owner.itemList)
        {
            if (it.name.Contains(itemName))
            {
                return true;
            }
        }
        return false;
    }
}
