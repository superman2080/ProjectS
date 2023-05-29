using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ItemManager : MonoBehaviour
{
    public Button[] selectBtn = new Button[3];
    public List<ItemCtrl> itemList = new List<ItemCtrl>();

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        GetRandomItems();
    }

    public void GetRandomItems()
    {
        foreach (var btn in selectBtn)
        {
            btn.onClick.RemoveAllListeners();
            ItemCtrl item = PhotonNetwork.Instantiate("Items/MaxHPUp", Vector3.zero, Quaternion.identity).GetComponent<ItemCtrl>();
            item.transform.parent = transform;
            item.itemManager = this;
            itemList.Add(item);
            btn.onClick.AddListener(() => item.pv.RPC(nameof(ItemCtrl.OnGetItem), RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, "MaxHPUp"));
        }
    }
}
