using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class AnimatorEventsHandler : MonoBehaviour
{
    public Transform playerFeet;
    
    Player playerRoot;

    private void Awake()
    {
        playerRoot = GetComponentInParent<Player>();
    }

    public void UnParentMesh()
    {
        transform.parent = null;
    }

    public void ParentMesh()
    {
        if (transform.parent != null) return;
        
        transform.parent = playerRoot.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0,0,0);
        playerRoot.StopClimbing();
    }

    public void TeleportRootToMesh()
    {
        playerRoot.transform.position = playerFeet.position;
    }
}