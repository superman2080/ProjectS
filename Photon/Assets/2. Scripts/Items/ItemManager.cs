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
    [Header("For Debug item list(max 3)\nPut string on right place if you want item debug (or leave empty)")]
    public string[] debugItemList = new string[3];
    private PlayerCtrl owner;
    private List<Dictionary<string, object>> itemChart;

    private void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        itemChart = CSVReader.Read("ItemTable");
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

        List<string> tempItemList = GetRandomItemNames();
        for (int i = 0; i < selectBtn.Length; i++)
        {
            selectBtn[i].onClick.RemoveAllListeners();
            string itemName = tempItemList[i];
            if (string.IsNullOrEmpty(debugItemList[i]) == false)
                itemName = debugItemList[i];
            selectBtn[i].GetComponentInChildren<Text>().text = itemName;
                
            selectBtn[i].onClick.AddListener(() =>
            {
                ItemCtrl item = PhotonNetwork.Instantiate("Items/" + itemName, owner.transform.position, Quaternion.identity).GetComponent<ItemCtrl>();
                item.pv.RPC(nameof(ItemCtrl.SetOwner), RpcTarget.AllBuffered, actorNum);
                item.pv.RPC(nameof(ItemCtrl.OnGetItem), RpcTarget.AllBuffered, actorNum);
                //pv.RPC(nameof(GetItem), RpcTarget.AllBuffered, actorNum, itemName);
            });
        }
    }

    private List<string> GetRandomItemNames()
    {
        int rand = 0;
        List<string> tempItemList = new List<string>();
        while (tempItemList.Count < 3)
        {
            while (true)
            {
                rand = Random.Range(0, 9);
                bool notEqual = true;
                foreach (var it in owner.itemList)
                {
                    if (it.name.Contains(itemChart[rand]["ItemName"].ToString()))
                    {
                        notEqual = false;
                        break;
                    }
                }
                for (int i = 0; i < tempItemList.Count; i++)
                {
                    if (tempItemList[i] == itemChart[rand]["ItemName"].ToString())
                    {
                        notEqual = false;
                        break;
                    }

                }
                if (notEqual == true)
                    break;
            }
            tempItemList.Add(itemChart[rand]["ItemName"].ToString());
        }
        return tempItemList;
    }
}
