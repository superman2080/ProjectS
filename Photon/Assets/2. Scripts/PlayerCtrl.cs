using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public PhotonView pv;
    public Text nicknameText;
    public Image hp;

    private bool isGround;
    private Vector3 curPos;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(hp.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            hp.fillAmount = (float)stream.ReceiveNext();
        }
    }

    public void Awake()
    {
        nicknameText.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nicknameText.color = pv.IsMine ? Color.green : Color.red;

        if (pv.IsMine)
        {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.Follow = transform;
            cm.LookAt = transform;

        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            //이동
            float velocity = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * velocity, rb.velocity.y);
            if (velocity != 0)
            {
                animator.SetBool("Walk", true);
                pv.RPC(nameof(FlipXRPC), RpcTarget.AllBuffered, velocity);
            }
            else
                animator.SetBool("Walk", false);
            //

            //바닥 체크, 점프
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            animator.SetBool("Jump", !isGround);
            if (Input.GetKeyDown(KeyCode.Space) && isGround) pv.RPC(nameof(JumpRPC), RpcTarget.All);
            //

            //총알 발사
            if (Input.GetKeyDown(KeyCode.C))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(spriteRenderer.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity)
                    .GetComponent<PhotonView>().RPC(nameof(BulletCtrl.DirRPC), RpcTarget.All, spriteRenderer.flipX ? -1 : 1);
                animator.SetTrigger("Shot");
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100)
            transform.position = curPos;
        else
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
        }
    }

    [PunRPC]
    void FlipXRPC(float axis) => spriteRenderer.flipX = axis == -1;

    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 700);
    }

    public void TakeDamage()
    {
        hp.fillAmount -= 0.1f;
        if(hp.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            pv.RPC(nameof(DestroyRPC), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    public void SelectItems()
    {
        
    }

}
