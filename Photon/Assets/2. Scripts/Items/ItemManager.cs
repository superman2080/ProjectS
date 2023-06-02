using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ItemManager : MonoBehaviour
{
    public Button[] selectBtn = new Button[3];
    public List<ItemCtrl> itemList = new List<ItemCtrl>();
    public PhotonView pv;
    private PlayerCtrl owner;
    private int actorNum;

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
            ItemCtrl item = PhotonNetwork.Instantiate("Items/Shield", Vector3.zero, Quaternion.identity).GetComponent<ItemCtrl>();
            item.transform.parent = transform;
            itemList.Add(item);

            btn.onClick.AddListener(() =>
            {
                pv.RPC(nameof(GetItem), RpcTarget.AllBuffered, actorNum,"Shield");   
                //item.pv.RPC(nameof(ItemCtrl.OnGetItem), RpcTarget.AllBuffered, actorNum, "Shield");
                //foreach (var temp in itemList)
                //{
                //    PhotonNetwork.Destroy(temp.gameObject);
                //}
                //itemList.Clear();
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
