using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class BulletCtrl : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    public float damage;
    public event EventHandler OnBulletHit;

    // Start is called before the first frame update
    void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
        Destroy(gameObject, 3.5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * 7 * Time.deltaTime);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            OnBulletHit?.Invoke(this, EventArgs.Empty);
            pv.RPC(nameof(DestroyRPC), RpcTarget.AllBuffered);
        }
        if (!pv.IsMine && collision.CompareTag("Player") && collision.GetComponent<PhotonView>().IsMine && !collision.CompareTag("Bullet"))
        {
            OnBulletHit?.Invoke(this, EventArgs.Empty);
            collision.GetComponent<PlayerCtrl>().pv.RPC(nameof(PlayerCtrl.TakeDamage), RpcTarget.AllBuffered, damage);
            pv.RPC(nameof(DestroyRPC), RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void SetAngle(float ang) => transform.eulerAngles = new Vector3(0, 0, ang);

    [PunRPC]
    public void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    public void SetDamage(float damage) => this.damage = damage;
}
