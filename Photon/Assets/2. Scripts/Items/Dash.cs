using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Dash : ItemCtrl
{
    public float dashDist;

    public float dashTime;

    [Range(0.5f, 10f)]
    public float coolTime;
    public AudioClip dashSFX;
    private Coroutine nowCor;
    private Coroutine coolTimeCor;


    public override void ItemEffect()
    {
        int flip = owner.spriteRenderer.flipX ? -1 : 1;
        RaycastHit2D hit = Physics2D.Raycast(owner.transform.position, Vector2.left * flip, dashDist, 1 << 8);

        if (hit)
        {
            if (nowCor == null && coolTimeCor == null)
                nowCor = StartCoroutine(DashCor(transform.position, hit.point, dashTime));
        }
        else
        {
            if (nowCor == null && coolTimeCor == null)
                nowCor = StartCoroutine(DashCor(owner.transform.position, (Vector2)owner.transform.position + (Vector2.left * flip * dashDist), dashTime));
        }
    }

    private IEnumerator DashCor(Vector2 origin, Vector2 moveTo, float time)
    {
        float dT = 0;
        owner.audioSource.PlayOneShot(dashSFX);
        while(true)
        {
            if (dT > time)
                break;

            owner.transform.position = Vector2.Lerp(origin, moveTo, dT / time);
            yield return null;
            dT += Time.deltaTime;
        }
        owner.rb.velocity = Vector2.zero;
        nowCor = null;
        coolTimeCor = StartCoroutine(CoolTimeCor(coolTime));
    }

    private IEnumerator CoolTimeCor(float cT)
    {
        yield return new WaitForSeconds(cT);
        coolTimeCor = null;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.ClearSkillEvent();
        owner.OnUseSkill += ItemEvent;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        int flip = owner.spriteRenderer.flipX ? -1 : 1;
        Gizmos.DrawLine(owner.transform.position, (Vector2)owner.transform.position + (Vector2.left * flip * dashDist));
    }
}
