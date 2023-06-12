using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : ItemCtrl
{
    public float keepLaserTime;
    [Range(3f, 10f)]
    public float magnitude;
    public LineRenderer lineRenderer;
    public float laserWidth;
    private float laserScale;
    private Coroutine nowCor;

    private Vector2 debugSize;
    private Vector2 debugCenter;

    public override void ItemEffect()
    {
        pv.RPC(nameof(LaserShotRPC), RpcTarget.All);
    }

    [PunRPC]
    public void LaserShotRPC()
    {
        if (pv.IsMine && nowCor == null)
            nowCor = StartCoroutine(LaserShotCor());
    }

    private IEnumerator LaserShotCor()
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
        pv.RPC(nameof(SetLaserScale), RpcTarget.All, dT);
        Vector2 endPos = new Vector2((-Mathf.Cos(owner.gunAngle * Mathf.Deg2Rad) * laserScale) + owner.gunTr.position.x, (-Mathf.Sin(owner.gunAngle * Mathf.Deg2Rad) * laserScale) + owner.gunTr.position.y);
        debugCenter = new Vector2((owner.gunTr.position.x + endPos.x) / 2, (owner.gunTr.position.y + endPos.y) / 2);
        debugSize = new Vector2(Mathf.Abs(owner.gunTr.position.x - endPos.x), laserWidth);
        Collider2D[] players = Physics2D.OverlapBoxAll(debugCenter, debugSize, owner.gunAngle);
        foreach (var player in players)
        {
            if(player.gameObject != owner.gameObject && player.gameObject.CompareTag("Player"))
            {
                player.GetComponent<PlayerCtrl>().pv.RPC(nameof(PlayerCtrl.TakeDamage), RpcTarget.AllBuffered, owner.damage);
            }
        }
        pv.RPC(nameof(DrawLaser), RpcTarget.All, endPos);
        dT = 0;
        while (true)
        {
            if(dT > keepLaserTime)
            {
                break;
            }
            dT += Time.deltaTime;
            pv.RPC(nameof(SetLaserWidth), RpcTarget.All, dT);
            yield return null;
        }

        pv.RPC(nameof(RemoveLaser), RpcTarget.All);
        nowCor = null;
    }

    [PunRPC]
    public void SetLaserScale(float dT) => laserScale = (float)Math.Round(dT, 2) * magnitude;

    [PunRPC]
    public void RemoveLaser() {
        lineRenderer.SetPosition(0, Vector2.zero);
        lineRenderer.SetPosition(1, Vector2.zero);
    }

    [PunRPC]
    public void DrawLaser(Vector2 endPos)
    {
        
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, owner.gunTr.position);
        lineRenderer.SetPosition(1, endPos);
    }

    [PunRPC]
    public void SetLaserWidth(float dT) {
        float nowWidth = laserWidth - (dT / keepLaserTime * laserWidth);
        lineRenderer.startWidth = nowWidth / 2;
        lineRenderer.endWidth = nowWidth;
    }

    [PunRPC]
    public override void OnGetItem(int actorNum)
    {
        base.OnGetItem(actorNum);

        owner.ClearAttackEvent();
        owner.OnPlayerAttack += ItemEvent;
    }
}
