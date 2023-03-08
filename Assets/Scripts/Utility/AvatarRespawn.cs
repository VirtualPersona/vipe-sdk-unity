using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarRespawn : MonoBehaviour
{
    [SerializeField]
    private GameObject VRM_TARGET;
    
    private void OnCollisionEnter(Collision collision)
    {
        collision.transform.position = VRM_TARGET.transform.position;
    }

}
