using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class ItemCtrl : MonoBehaviourPunCallbacks
{
    [SerializeField]                //µð¹ö±ë¿ë
    protected PlayerCtrl owner;
    public PhotonView pv;
    public ItemManager itemManager;

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
        item.transform.parent = owner.itemTr;
        owner.itemList.Add(item);

        foreach (var tempItem in itemManager.itemList)
        {
            PhotonNetwork.Destroy(tempItem.gameObject);
        }
        itemManager.itemList.Clear();
        owner.pv.RPC(nameof(PlayerCtrl.Respawn), RpcTarget.AllBuffered);
    }
}
