using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Elevator : PlatformCtrl
{
    public Vector3[] destination = new Vector3[2];
    public float moveTime;
    private bool flag;
    private Coroutine nowCor;


    protected override void OnLeavePlatform(Collision2D collision)
    {
    }

    protected override void OnTouchPlatform(Collision2D collision)
    {
    }

    protected override void PlatformBehavior()
    {
        if (nowCor == null)
            nowCor = StartCoroutine(MoveElevator(moveTime));

    }

    private IEnumerator MoveElevator(float mT)
    {
        float dT = 0;
        while (true)
        {
            if (dT > mT)
                break;

            if (flag)
                transform.position = Vector3.Lerp(destination[0], destination[1], dT / mT);
            else
                transform.position = Vector3.Lerp(destination[1], destination[0], dT / mT);

            dT += Time.deltaTime;
            yield return null;
        }
        flag = !flag;
        nowCor = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            PlatformBehavior();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(destination[0], destination[1]);
        Gizmos.DrawWireSphere(destination[0], 0.1f);
        Gizmos.DrawWireSphere(destination[1], 0.1f);
    }
}
