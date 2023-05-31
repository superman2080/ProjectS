using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using System;

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D rb;
    [Header("Related to appearance")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public PhotonView pv;
    public Text nicknameText;
    public GameObject selectItemPanel;
    public SpriteRenderer gunSprite;
    public Transform gunTr;
    public Image hpBar;

    [Header("Related to attack")]
    public float damage;
    [Range(0, 100)]
    public float maxHp;
    public float hp;
    public float speed;
    private Vector2 bulletMovePos;
    private float gunTrX;
    private float flip = 1;
    private float gunAngle;
    [Header("Related to items")]
    public Transform itemTr;
    public List<ItemCtrl> itemList = new List<ItemCtrl>();
    public event EventHandler OnSpawnPlayer;
    public event EventHandler OnPlayerAttack;
    public event EventHandler OnPlayerJump;
    public event EventHandler OnTakenDamage;
    public EventHandler DefaultAttack;

    [HideInInspector]
    public int actorNum;
    [HideInInspector]
    public bool isDead = false;
    private bool isGround;
    private Vector3 curPos;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(hp);
            stream.SendNext(gunAngle);
            stream.SendNext(flip);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            hp = (float)stream.ReceiveNext();
            gunAngle = (float)stream.ReceiveNext();
            flip = (float)stream.ReceiveNext();
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

        //꺼져있는 오브젝트이기 때문에
        selectItemPanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        selectItemPanel.SetActive(false);

        //총 포지션 x 부분
        gunTrX = gunTr.transform.localPosition.x;
        //이벤트 처리 부분
    }

    public void Start()
    {
        pv.RPC(nameof(SetActorNum), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
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
            if (!isDead)
            {
                //이동
                PlayerMove();
                //총알 발사
                PlayerAttack();
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100)
            transform.position = curPos;
        else
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
            FlipXRPC(flip);
            SetGunAngle(gunAngle);
        }
    }

    void PlayerMove()
    {
        float velocity = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(4 * velocity, rb.velocity.y);
        if (velocity != 0)
        {
            animator.SetBool("Walk", true);
        }
        else
            animator.SetBool("Walk", false);
        //

        //바닥 체크, 점프
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        animator.SetBool("Jump", !isGround);
        if (Input.GetKeyDown(KeyCode.Space) && isGround) pv.RPC(nameof(JumpRPC), RpcTarget.All);
        //

    }

    void PlayerAttack()
    {
        bulletMovePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //총쏘는 방향 바라보기
        if (spriteRenderer.flipX == false)           //정방향 보고있을 때
        {
            if (transform.position.x > bulletMovePos.x)
            {
                flip = -1;
            }
            else
            {
                flip = 1;
            }
        }
        else                                        //역방향 보고있을 때
        {
            if (transform.position.x < bulletMovePos.x)
            {
                flip = 1;
            }
            else
            {
                flip = -1;
            }
        }
        gunAngle = GameMath.GetAngle(transform.position, bulletMovePos);
        FlipXRPC(flip);
        SetGunAngle(gunAngle);
        if (Input.GetMouseButtonDown(0))
        {
            //
            StartCoroutine(FireBulletFlipXCor(flip, 0.5f));
            BulletCtrl b = PhotonNetwork.Instantiate("Bullet", gunTr.position, Quaternion.identity).GetComponent<BulletCtrl>();
            b.pv.RPC(nameof(BulletCtrl.SetDamage), RpcTarget.All, damage);
            b.pv.RPC(nameof(BulletCtrl.SetAngle), RpcTarget.All, gunAngle);
            animator.SetTrigger("Shot");
        }
    }

    void FlipXRPC(float axis)
    {
        spriteRenderer.flipX = axis == -1;
        gunTr.transform.localScale = new Vector3(axis, 1, 1);
        gunTr.transform.localPosition = new Vector3(-Mathf.Cos(gunAngle * Mathf.Deg2Rad) * 0.5f, -Mathf.Sin(gunAngle * Mathf.Deg2Rad) * 0.25f, 0);
    }

    void SetGunAngle(float ang)
    {
        gunTr.eulerAngles = new Vector3(0, 0, !spriteRenderer.flipX ? ang + 180f : ang);
    }

    private IEnumerator FireBulletFlipXCor(float axis, float totalTime)
    {
        float dT = 0;
        while (true)
        {
            FlipXRPC(axis);
            dT += Time.deltaTime;
            yield return null;
            if (dT > totalTime)
                break;
        }
    }

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
    public void SetActorNum(int n) => actorNum = n;

    [PunRPC]
    public void PlayerDeath()
    {
        spriteRenderer.enabled = false;
        transform.Find("Canvas").gameObject.SetActive(false);
        gameObject.GetComponent<Collider2D>().enabled = false;
        rb.isKinematic = true;
        isDead = true;
        if (pv.IsMine)
            selectItemPanel.SetActive(true);
    }

    [PunRPC]
    public void Respawn()
    {
        spriteRenderer.enabled = true;
        transform.Find("Canvas").gameObject.SetActive(true);
        gameObject.GetComponent<Collider2D>().enabled = true;
        rb.isKinematic = false;
        transform.position = new Vector3(UnityEngine.Random.Range(-7f, 21f), 4, 0);
        isDead = false;
        pv.RPC(nameof(InitialPlayerProps), RpcTarget.All);
        if (pv.IsMine)
        {
            selectItemPanel.SetActive(false);
        }
    }
}
