using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using System;

public class DamageEventArgs : EventArgs
{
    public float damage;
}

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D rb;
    [Header("Related to appearance")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public PhotonView pv;
    public Text nicknameText;
    public GameObject selectItemPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public SpriteRenderer gunSprite;
    public Transform gunTr;
    public Image hpBar;
    public Image attGauge;
    public Image gunUIImage;
    public Image deathCntImg;

    [Header("Related to attack")]
    public float damage;
    [Range(0, 100)]
    public float maxHp;
    [HideInInspector]
    public float hp;
    [Range(1, 10)]
    public float speed = 4;
    [HideInInspector]
    public bool isInvincible = false;
    [HideInInspector]
    public float gunAngle;
    public float attSpeed;
    [HideInInspector]
    public float attMag;
    private float attDelta;
    private Vector2 bulletMovePos;
    private float gunTrX;
    private float flip = 1;

    [Header("Related to items")]
    public Transform itemTr;
    public List<ItemCtrl> itemList = new List<ItemCtrl>();
    public event EventHandler OnSpawnPlayer;
    public event EventHandler OnPlayerAttack;
    public event EventHandler OnUseSkill;
    public event EventHandler OnTakenDamage;
    public EventHandler DefaultAttack;

    [Header("Related to audio")]
    public AudioSource audioSource;
    public AudioClip gunSFX;

    [HideInInspector]
    public int actorNum;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public int deathCnt;
    private int groundCnt;
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
        winPanel = GameObject.Find("Canvas").transform.Find("WinPanel").gameObject;
        losePanel = GameObject.Find("Canvas").transform.Find("LosePanel").gameObject;
        selectItemPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        attGauge = GameObject.Find("Canvas").transform.Find("IngameUI").Find("AttackGauge").GetComponent<Image>();
        gunUIImage = GameObject.Find("Canvas").transform.Find("IngameUI").Find("GunUIImage").GetComponent<Image>();
        deathCntImg = GameObject.Find("Canvas").transform.Find("IngameUI").Find("DeathCountImg").GetComponent<Image>();
        //총 포지션 x 부분
        gunTrX = gunTr.transform.localPosition.x;
        attMag = 1;
        //이벤트 처리 부분
        if (pv.IsMine)
        {
            pv.RPC(nameof(InitialEvents), RpcTarget.AllBuffered);
        }
    }

    public void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        pv.RPC(nameof(SetActorNum), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        pv.RPC(nameof(InitialPlayerProps), RpcTarget.All);
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            StartCoroutine(InitRoundCor());
    }

    [PunRPC]
    private void InitialPlayerProps()
    {
        hp = maxHp;
        hpBar.fillAmount = 1;
        OnSpawnPlayer?.Invoke(this, EventArgs.Empty);
    }

    [PunRPC]
    private void InitialEvents()
    {
        DefaultAttack += (sender, e) => { 
            audioSource.PlayOneShot(gunSFX);
            CreateBullet(damage, gunAngle);
        };
        OnPlayerAttack += DefaultAttack;
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
            FlipX(flip);
            SetGunAngle(gunAngle);
        }
    }

    void PlayerMove()
    {
        float velocity = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(speed * velocity, rb.velocity.y);

        //Processing animation
        if (velocity != 0)
            animator.SetBool("Walk", true);
        else
            animator.SetBool("Walk", false);

        if (groundCnt != 2)
        {
            animator.SetBool("Jump", true);
            if (rb.velocity.y < 0)
                animator.SetBool("Landing", true);
        }
        else
        {
            animator.SetBool("Jump", false);
            animator.SetBool("Landing", false);
        }
        //



        //바닥 체크, 점프
        if (Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.1f, 1 << LayerMask.NameToLayer("Ground")) != null && rb.velocity.y < 0)
        {
            groundCnt = 2;
        }

        if (Input.GetKeyDown(KeyCode.Space) && groundCnt > 0)
        {
            groundCnt--;
            pv.RPC(nameof(JumpRPC), RpcTarget.All);
        }
        //

        

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnUseSkill?.Invoke(this, EventArgs.Empty);
        }
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
        FlipX(flip);
        SetGunAngle(gunAngle);

        if (attDelta <= 1 / (attSpeed * attMag))
        {
            attDelta += Time.deltaTime;

        }

        if (Input.GetMouseButtonDown(0) && attDelta >= 1 / (attSpeed * attMag))
        {
            OnPlayerAttack?.Invoke(this, EventArgs.Empty);

            attDelta = 0;
        }
        attGauge.fillAmount = attDelta / (1 / (attSpeed * attMag));
    }

    public void CreateBullet(float dmg, float ang)
    {
        BulletCtrl b = PhotonNetwork.Instantiate("Bullet", gunTr.position, Quaternion.identity).GetComponent<BulletCtrl>();
        b.pv.RPC(nameof(BulletCtrl.SetDamage), RpcTarget.All, damage);
        b.pv.RPC(nameof(BulletCtrl.SetAngle), RpcTarget.All, ang);
    }

    void FlipX(float axis)
    {
        spriteRenderer.flipX = axis == 1; // -1;
        gunTr.transform.localScale = new Vector3(-axis, -1, 1);
        gunTr.transform.localPosition = new Vector3(-Mathf.Cos(gunAngle * Mathf.Deg2Rad) * 0.5f, -Mathf.Sin(gunAngle * Mathf.Deg2Rad) * 0.25f, 0);
    }

    void SetGunAngle(float ang)
    {
        gunTr.eulerAngles = new Vector3(0, 0, !spriteRenderer.flipX ? ang + 180f : ang);
    }

    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 500);

    }

    [PunRPC]
    public void TakeDamage(float dmg)
    {
        if (OnTakenDamage != null)
        {
            DamageEventArgs args = new DamageEventArgs();
            args.damage = dmg;
            OnTakenDamage(this, args);
        }
        if (!isInvincible)
            hp -= dmg;
        hpBar.fillAmount = hp / maxHp;
        if(hp <= 0)
        {
            deathCnt++;
            pv.RPC(nameof(PlayerDeath), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
    [PunRPC]
    public void SetActorNum(int n) => actorNum = n;

    [PunRPC]
    public void SetIsInvincible(bool b) => isInvincible = b;

    [PunRPC]
    public void SetGunSprite(string name)
    {
        Sprite gunResource = Resources.Load<Sprite>("Sprites/Guns/" + name);
        gunSprite.sprite = gunResource;
        if (pv.IsMine)
            gunUIImage.sprite = gunResource;
    }

    [PunRPC]
    public void PlayerDeath()
    {
        spriteRenderer.enabled = false;
        gunSprite.enabled = false;
        transform.Find("Canvas").gameObject.SetActive(false);
        gameObject.GetComponent<Collider2D>().enabled = false;
        rb.isKinematic = true;
        isDead = true;

        if (deathCnt >= 3f)
        {
            if (pv.IsMine)
            {
                StartCoroutine(PanelFadeIn(losePanel, 1.5f));
            }
            else
            {
                StartCoroutine(PanelFadeIn(winPanel, 1.5f));
            }
        }
        else if (pv.IsMine)
        {
            selectItemPanel.SetActive(true);
        }
    }

    private IEnumerator PanelFadeIn(GameObject resultPanel, float fadeTime)
    {
        resultPanel.SetActive(true);
        Color color = new Color (1f, 1f, 1f, 0f);
        float dT = 0;
        while (true)
        {
            if (dT > fadeTime)
                break;

            dT += Time.deltaTime;
            float a = dT / fadeTime;
            color.a = a;
            resultPanel.transform.GetChild(0).GetComponent<Image>().color = color;
            yield return null;
        }
        resultPanel.transform.GetChild(0).GetComponent<Image>().color = Color.white;
    }

    public void ClearAttackEvent() => OnPlayerAttack = null;

    public void ClearSkillEvent() => OnUseSkill = null;

    [PunRPC]
    public void Respawn()
    {
        spriteRenderer.enabled = true;
        gunSprite.enabled = true;
        transform.Find("Canvas").gameObject.SetActive(true);
        gameObject.GetComponent<Collider2D>().enabled = true;
        rb.isKinematic = false;
        isDead = false;
        if (pv.IsMine)
        {
            StartCoroutine(InitRoundCor());
            selectItemPanel.SetActive(false);
            deathCntImg.fillAmount = (3 - deathCnt) / 3f;
        }
    }

    [PunRPC]
    public void FindSpawnPos()
    {
        Vector3 spawnPos = GameObject.Find("Spawn_" + actorNum).transform.position;
        transform.position = spawnPos;
    }

    private IEnumerator InitRoundCor()
    {
        yield return null;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            player.GetComponent<PlayerCtrl>().pv.RPC(nameof(InitialPlayerProps), RpcTarget.All);
            player.GetComponent<PlayerCtrl>().pv.RPC(nameof(PlayerCtrl.FindSpawnPos), RpcTarget.AllBuffered);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(0, -0.5f), 0.1f);
    }

}
