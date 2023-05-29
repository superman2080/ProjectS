using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMath : MonoBehaviour
{
    public static float GetAngle(Vector2 origin, Vector2 moveTo)
    {
        Vector2 rot = (origin - moveTo).normalized;
        return Mathf.Atan2(rot.y, rot.x) * Mathf.Rad2Deg;
    }
}
