using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

[RequireComponent(typeof(PhotonView))]
public abstract class ItemCtrl : MonoBehaviourPunCallbacks
{
    protected PlayerCtrl owner;
    public PhotonView pv;
    public EventHandler ItemEvent;
    private void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
    }

    [PunRPC]
    public virtual void OnGetItem(int actorNum)
    {
        transform.parent = owner.itemTr;
        owner.itemList.Add(this);
        ItemEvent += (sender, e) => { ItemEffect(); };
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

    [PunRPC]
    public void SetOwner(int actorNum)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //죽은 플레이어를 검색해서 그 플레이어가 아이템 선택
        foreach (var player in players)
        {
            if (actorNum == player.GetComponent<PlayerCtrl>().actorNum)
            {
                owner = player.GetComponent<PlayerCtrl>();
                break;
            }
        }
    }
}
