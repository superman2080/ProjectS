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
    public AudioClip buttonSound;
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

        List<int> tempItemIDList = GetRandomItemID();
        for (int i = 0; i < selectBtn.Length; i++)
        {
            selectBtn[i].onClick.RemoveAllListeners();
            int itemID = tempItemIDList[i];
            if (string.IsNullOrEmpty(debugItemList[i]) == false)
                itemID = int.Parse(debugItemList[i]);
            //selectBtn[i].GetComponentInChildren<Text>().text = itemName;
            selectBtn[i].image.sprite = Resources.Load<Sprite>("Sprites/Card/" + itemChart[itemID]["ItemName"].ToString());
            selectBtn[i].GetComponentInChildren<Text>().text = itemChart[itemID]["ItemDescription"].ToString();
            selectBtn[i].onClick.AddListener(() =>
            {
                ItemCtrl item = PhotonNetwork.Instantiate("Items/" + itemChart[itemID]["ItemName"].ToString(), owner.transform.position, Quaternion.identity).GetComponent<ItemCtrl>();
                item.pv.RPC(nameof(ItemCtrl.SetOwner), RpcTarget.AllBuffered, actorNum);
                item.pv.RPC(nameof(ItemCtrl.OnGetItem), RpcTarget.AllBuffered, actorNum);
                owner.audioSource.PlayOneShot(buttonSound);
                //pv.RPC(nameof(GetItem), RpcTarget.AllBuffered, actorNum, itemName);
            });
        }
    }

    private List<int> GetRandomItemID()
    {
        int rand = 0;
        int cnt = 0;
        List<int> tempItemList = new List<int>();
        while (tempItemList.Count < 3)
        {
            cnt = 0;
            while (true)
            {
                rand = Random.Range(0, 10);
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
                    if (tempItemList[i] == int.Parse(itemChart[rand]["ItemID"].ToString()) - 1)
                    {
                        notEqual = false;
                        break;
                    }

                }
                if (notEqual == true)
                    break;
                //Error prevantion code
                if (cnt > 100)       
                {
                    tempItemList.Add(0);
                    break;
                }
                cnt++;
                //
            }
            tempItemList.Add(rand);
        }
        return tempItemList;
    }
}
