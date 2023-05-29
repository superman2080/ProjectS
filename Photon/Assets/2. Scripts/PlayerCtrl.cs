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
    public GameObject selectItemPanel;
    public Image hpBar;
    public float damage;
    [Range(0, 100)]
    public float maxHp;
    public float hp;
    public Transform itemTr;
    public List<ItemCtrl> itemList = new List<ItemCtrl>();
    [HideInInspector]
    public int actorNum;

    private bool isGround;
    private Vector3 curPos;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(hp);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            hp = (float)stream.ReceiveNext();
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
        actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        //꺼져있는 오브젝트이기 때문에
        selectItemPanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        selectItemPanel.SetActive(false);
    }

    public void Start()
    {
        pv.RPC(nameof(InitialPlayerProps), RpcTarget.All);
    }

    [PunRPC]
    private void InitialPlayerProps()
    {
        hp = maxHp;
        hpBar.fillAmount = 1;
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
                BulletCtrl b = PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(spriteRenderer.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity).GetComponent<BulletCtrl>();
                b.pv.RPC(nameof(BulletCtrl.SetDamage), RpcTarget.All, damage);
                b.GetComponent<PhotonView>().RPC(nameof(BulletCtrl.DirRPC), RpcTarget.All, spriteRenderer.flipX ? -1 : 1);
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

    [PunRPC]
    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        hpBar.fillAmount = hp / maxHp;
        if(hp <= 0)
        {
            pv.RPC(nameof(PlayerDeath), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    public void PlayerDeath()
    {
        spriteRenderer.enabled = false;
        transform.Find("Canvas").gameObject.SetActive(false);
        if (pv.IsMine)
            selectItemPanel.SetActive(true);
    }

    [PunRPC]
    public void Respawn()
    {
        spriteRenderer.enabled = true;
        transform.Find("Canvas").gameObject.SetActive(true);
        pv.RPC(nameof(InitialPlayerProps), RpcTarget.All);
        if (pv.IsMine)
        {
            selectItemPanel.SetActive(false);
        }
    }
}
