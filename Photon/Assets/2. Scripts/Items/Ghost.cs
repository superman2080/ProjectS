using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Ghost : ItemCtrl
{
    /* 마우스 휠 클릭 시
     상대방에게 자신과 자신의 닉네임,체력바, 보호막은 투명하게
     자신에게 자신은 반투명하게, UI는 그대로 */

    private SpriteRenderer[] srList = new SpriteRenderer[2];
    private SpriteRenderer shieldSr = new SpriteRenderer();

    [Range(0f, 0.5f)]
    public float transparency;
    private Color color;

    [Range(3f, 10f)]
    public float ghostDuration;
    private float ghostStartTime;

    private bool isUsed;
    private bool isDead;
    private bool isEnded;
    private bool hasShield;


    public override void ItemEffect()
    {
        GameObject gunSr = (owner.transform.Find("GunTr")).transform.Find("GunImage").gameObject;

        srList[0] = owner.GetComponent<SpriteRenderer>();
        srList[1] = gunSr.GetComponent<SpriteRenderer>();
        isUsed = false;
        isEnded = false;
        hasShield = (owner.transform.Find("ItemTr")).transform.Find("Shield(Clone)");
    }

    void Update()
    {
        isDead = owner.isDead;
        StartCoroutine(OnBecameGhostCor());
    }

    [PunRPC]
    public void OnBecameGhost()
    {
        transparency = pv.IsMine ? 0.5f : transparency;
        color = new Color(1f, 1f, 1f, transparency);
        foreach (SpriteRenderer sr in srList) sr.color = color;

        if (!pv.IsMine) owner.transform.Find("Canvas").gameObject.SetActive(false);

        if (hasShield)
        {
            GameObject shield = (owner.transform.Find("ItemTr")).transform.Find("Shield(Clone)").gameObject;
            shieldSr = shield.GetComponent<SpriteRenderer>();
            if (!pv.IsMine) shieldSr.color = new Color(1f, 1f, 1f, 0f);
        }
            
    }

    [PunRPC]
    public void OnRespawn()
    {
        color.a = 1f;
        foreach (SpriteRenderer sr in srList) sr.color = color;
        if (hasShield)
            shieldSr.color = new Color(1f, 1f, 1f, 1f);
    }

    [PunRPC]
    public void OnTimeOut()
    {
        color.a = 1f;
        foreach (SpriteRenderer sr in srList) sr.color = color;
        owner.transform.Find("Canvas").gameObject.SetActive(true);
        if (hasShield)
            shieldSr.color = new Color(1f, 1f, 1f, 1f);
    }

    private IEnumerator OnBecameGhostCor()
    {
        yield return null;

        if (!isUsed){
            if (pv.IsMine && Input.GetMouseButtonDown(2)) //마우스휠
            {
                ghostStartTime = Time.time;
                pv.RPC(nameof(OnBecameGhost), RpcTarget.AllBuffered);
                isUsed = true;
            }
        }
        else{
            if (!isEnded){
                if (Time.time - ghostStartTime >= ghostDuration){
                    pv.RPC(nameof(OnTimeOut), RpcTarget.AllBuffered);
                    isEnded = true;   }
                else if (isDead) {
                    pv.RPC(nameof(OnRespawn), RpcTarget.AllBuffered);
                    isEnded = true;   }
            }
        }
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.OnSpawnPlayer += ItemEvent;
    }
}