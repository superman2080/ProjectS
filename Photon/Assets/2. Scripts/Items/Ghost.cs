using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Ghost : ItemCtrl
{
    /* 마우스 휠 클릭 시
     상대방에게 자신과 자신의 닉네임,체력바, 보호막은 투명하게
     자신에게 자신은 반투명하게, UI는 그대로 */

    [Range(0f, 0.5f)]
    public float transparency;

    [Range(3f, 10f)]
    public float ghostDuration;
    private Coroutine nowCor;
    [Range(0.5f, 10f)]
    public float coolTime;
    private Coroutine coolTimeCor;
    


    //public override void ItemEffect()
    //{
    //    GameObject gunSr = (owner.transform.Find("GunTr")).transform.Find("GunImage").gameObject;



    //    srList[0] = owner.GetComponent<SpriteRenderer>();
    //    srList[1] = owner.gunSprite.GetComponent<SpriteRenderer>();
    //    isUsed = false;
    //    isEnded = false;
    //    hasShield = (owner.transform.Find("ItemTr")).transform.Find("Shield(Clone)");
    //}

    //void Update()
    //{
    //    isDead = owner.isDead;
    //    StartCoroutine(OnBecameGhostCor());
    //}

    //[PunRPC]
    //public void OnBecameGhost()
    //{
    //    transparency = pv.IsMine ? 0.5f : transparency;
    //    color = new Color(1f, 1f, 1f, transparency);
    //    foreach (SpriteRenderer sr in srList) sr.color = color;

    //    if (!pv.IsMine) owner.transform.Find("Canvas").gameObject.SetActive(false);

    //    if (hasShield)
    //    {
    //        GameObject shield = (owner.transform.Find("ItemTr")).transform.Find("Shield(Clone)").gameObject;
    //        shieldSr = shield.GetComponent<SpriteRenderer>();
    //        if (!pv.IsMine) shieldSr.color = new Color(1f, 1f, 1f, 0f);
    //    }
            
    //}

    //[PunRPC]
    //public void OnRespawn()
    //{
    //    color.a = 1f;
    //    foreach (SpriteRenderer sr in srList) sr.color = color;
    //    if (hasShield)
    //        shieldSr.color = new Color(1f, 1f, 1f, 1f);
    //}

    //[PunRPC]
    //public void OnTimeOut()
    //{
    //    color.a = 1f;
    //    foreach (SpriteRenderer sr in srList) sr.color = color;
    //    owner.transform.Find("Canvas").gameObject.SetActive(true);
    //    if (hasShield)
    //        shieldSr.color = new Color(1f, 1f, 1f, 1f);
    //}

    //private IEnumerator OnBecameGhostCor()
    //{
    //    yield return null;

    //    if (!isUsed){
    //        if (pv.IsMine && Input.GetMouseButtonDown(2)) //마우스휠
    //        {
    //            ghostStartTime = Time.time;
    //            pv.RPC(nameof(OnBecameGhost), RpcTarget.AllBuffered);
    //            isUsed = true;
    //        }
    //    }
    //    else{
    //        if (!isEnded){
    //            if (Time.time - ghostStartTime >= ghostDuration){
    //                pv.RPC(nameof(OnTimeOut), RpcTarget.AllBuffered);
    //                isEnded = true;   }
    //            else if (isDead) {
    //                pv.RPC(nameof(OnRespawn), RpcTarget.AllBuffered);
    //                isEnded = true;   }
    //        }
    //    }
    //}

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.ClearSkillEvent();

        owner.OnUseSkill += ItemEvent;
    }

    public override void ItemEffect()
    {
        pv.RPC(nameof(GhostSkill), RpcTarget.All, ghostDuration);
    }

    private IEnumerator GhostSkillCor(float duration)
    {

        //Transparenting child objects
        SpriteRenderer[] spriteRenderers = owner.transform.GetComponentsInChildren<SpriteRenderer>();
        Image hpArea = owner.transform.Find("Canvas").Find("HPArea").GetComponent<Image>();
        if (owner.pv.IsMine == false)
        {
            owner.nicknameText.enabled = false;
            owner.hpBar.enabled = false;
            hpArea.enabled = false;
        }
        foreach (var sR in spriteRenderers)
        {
            if (owner.pv.IsMine)
                sR.color = new Color(1, 1, 1, transparency);
            else
            {
                sR.enabled = false;
            }
        }
        //

        //Wait until end of duration
        yield return new WaitForSeconds(duration);
        //

        //Untransparenting child objects (not died)
        if (owner.isDead == false)
        {
            foreach (var sR in spriteRenderers)
            {
                sR.enabled = true;
                sR.color = Color.white;
            }
            owner.nicknameText.enabled = true;
            owner.hpBar.enabled = true;
            hpArea.enabled = true;
        }
        else
        {
            //If i die, wait until I'm respawn
            yield return new WaitUntil(() => owner.isDead == false);

            foreach (var sR in spriteRenderers)
            {
                sR.enabled = true;
                sR.color = Color.white;
            }
            owner.nicknameText.enabled = true;
            owner.hpBar.enabled = true;
            hpArea.enabled = true;
        }
        nowCor = null;
        coolTimeCor = StartCoroutine(CoolTimeCor(coolTime));
    }

    [PunRPC]
    public void GhostSkill(float duration)
    {
        if (nowCor == null && coolTimeCor == null)
            nowCor = StartCoroutine(GhostSkillCor(duration));
    }

    private IEnumerator CoolTimeCor(float cT)
    {
        yield return new WaitForSeconds(cT);
        coolTimeCor = null;
    }
}