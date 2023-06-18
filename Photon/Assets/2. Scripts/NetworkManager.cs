using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField nicknameInput;
    public GameObject disconnectedPanel;
    public GameObject respawnPanel;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom()
    {
        disconnectedPanel.SetActive(false);
        StartCoroutine(DestroyBullet());
        Spawn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        disconnectedPanel.SetActive(true);
        respawnPanel.SetActive(false);
    }

    public void Spawn()
    {
        //랜덤 위치 생성
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-19.2f, 19.2f), -2, 0), Quaternion.identity);
        respawnPanel.SetActive(false);
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject bullets in GameObject.FindGameObjectsWithTag("Bullet"))
            bullets.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

}
