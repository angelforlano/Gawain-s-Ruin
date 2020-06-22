using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Raycaster : MonoBehaviour
{
    public enum RaycastDirection
    {
        Forward,
        Down,
    }

    public bool debug = true;
    public RaycastDirection rayDirection;
    [Range(0.01f, 10)] public float rayLength = 1;
    public LayerMask targetMask;
    public string targetTag;

    Vector3 RayDirection
    {
        get 
        {
            switch (rayDirection)
            {
                case RaycastDirection.Forward: return transform.forward;
                case RaycastDirection.Down: return transform.up * -1;
                default: return transform.forward;
            }
        }
    }

    public bool Check()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position, RayDirection, out hitInfo, rayLength, targetMask.value))
        {
            if (targetTag != null && targetTag != "")
            {
                return hitInfo.collider.gameObject.CompareTag(targetTag);
            } else {
                return true;
            }
        }

        return false;
    }

    public Vector3 GetHitNormal()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, RayDirection, out hit, rayLength, targetMask.value))
        {
            return hit.normal;
        }

        return hit.normal;
    }

    void OnDrawGizmos()
    {
        if (!debug) return;
        
        if (Check())
        {
            Debug.DrawRay(transform.position, RayDirection * rayLength, Color.green);
        } else {
            Debug.DrawRay(transform.position, RayDirection * rayLength, Color.red);
        }
    }
}