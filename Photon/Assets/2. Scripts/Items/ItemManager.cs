using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ItemManager : MonoBehaviour
{
    public Button[] selectBtn = new Button[3];
    public PhotonView pv;
    private PlayerCtrl owner;

    private void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        GetRandomItems();
    }

    public void GetRandomItems()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int actorNum = 0;
        //죽은 플레이어를 검색해서 그 플레이어가 아이템 선택
        foreach (var player in players)
        {
            if (player.GetComponent<PlayerCtrl>().isDead == true)
            {
                actorNum = player.GetComponent<PlayerCtrl>().actorNum;
                owner = player.GetComponent<PlayerCtrl>();
                break;
            }
        }
        foreach (var btn in selectBtn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                pv.RPC(nameof(GetItem), RpcTarget.AllBuffered, actorNum,"Shield");   
            });
        }
    }

    [PunRPC]
    public void GetItem(int actorNum, string itemName)
    {
        Debug.LogError(owner.actorNum);
        ItemCtrl item = PhotonNetwork.Instantiate("Items/" + itemName, owner.transform.position, Quaternion.identity).GetComponent<ItemCtrl>();
        item.pv.RPC(nameof(ItemCtrl.SetOwner), RpcTarget.AllBuffered, actorNum);

        item.pv.RPC(nameof(ItemCtrl.OnGetItem), RpcTarget.AllBuffered, actorNum);
    }
}
