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
    public GameObject winPanel;
    public GameObject losePanel;

    public AudioSource audioSource;
    public AudioClip lobbyBGM;
    public AudioClip ingameBGM;


    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
    }

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        PlayBGM(lobbyBGM);
    }


    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom("Room" + GetRandomRoomCode(), roomOptions);
    }

    public override void OnJoinedRoom()
    {
        disconnectedPanel.SetActive(false);
        StartCoroutine(DestroyBullet());
        Spawn();
        PlayBGM(ingameBGM);
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
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        PlayBGM(lobbyBGM);
    }

    public void Spawn()
    {
        //랜덤 위치 생성
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        respawnPanel.SetActive(false);


    }

    private void PlayBGM(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private string GetRandomRoomCode()
    {
        string code = "";
        for (int i = 0; i < 5; i++)
        {
            code += Random.Range(0, 10).ToString();
        }
        return code;
    }


    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject bullets in GameObject.FindGameObjectsWithTag("Bullet"))
            bullets.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

}
