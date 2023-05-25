using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class BulletCtrl : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    int dir;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground")
            pv.RPC(nameof(DestroyRPC), RpcTarget.AllBuffered);
        if(!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<PlayerCtrl>().TakeDamage();
            pv.RPC(nameof(DestroyRPC), RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
