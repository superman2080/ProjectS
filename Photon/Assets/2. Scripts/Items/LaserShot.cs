using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : ItemCtrl
{
    [Range(3f, 10f)]
    public float magnitude;
    public LineRenderer lineRenderer;
    public override void ItemEffect()
    {
        pv.RPC(nameof(LaserShotRPC), RpcTarget.All);
    }

    [PunRPC]
    public void LaserShotRPC()
    {
        StartCoroutine(ChargeGauge());
    }

    private IEnumerator ChargeGauge()
    {
        float dT = 0;
        while (true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                break;
            }
            dT += Time.deltaTime;
            yield return null;
        }
        float laserScale = (float)Math.Round(dT, 2) * magnitude;
        Vector2 endPos = new Vector2((-Mathf.Cos(owner.gunAngle * Mathf.Deg2Rad) * laserScale) + owner.gunTr.position.x , (-Mathf.Sin(owner.gunAngle * Mathf.Deg2Rad) * laserScale) + owner.gunTr.position.y);
        lineRenderer.SetPosition(0, endPos);
        lineRenderer.SetPosition(1, owner.gunTr.position);
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.ClearAttackEvent();
        owner.OnPlayerAttack += ItemEvent;
    }
}
