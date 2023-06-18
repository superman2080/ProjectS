using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Dash : ItemCtrl
{
    public float dashDist;

    public float dashTime;
    private Coroutine nowCor;


    public override void ItemEffect()
    {
        int flip = owner.spriteRenderer.flipX ? -1 : 1;
        RaycastHit2D hit = Physics2D.Raycast(owner.transform.position, Vector2.left * flip, dashDist, 1 << 8);

        if (hit)
        {
            if (nowCor == null)
                nowCor = StartCoroutine(DashCor(transform.position, hit.point, dashTime));
        }
        else
        {
            if (nowCor == null)
                nowCor = StartCoroutine(DashCor(owner.transform.position, (Vector2)owner.transform.position + (Vector2.left * flip * dashDist), dashTime));
        }
    }

    private IEnumerator DashCor(Vector2 origin, Vector2 moveTo, float time)
    {
        float dT = 0;
        while(true)
        {
            if (dT > time)
                break;

            owner.transform.position = Vector2.Lerp(origin, moveTo, dT / time);
            yield return null;
            dT += Time.deltaTime;
        }
        nowCor = null;
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
