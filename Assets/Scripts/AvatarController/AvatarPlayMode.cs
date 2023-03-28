using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class AvatarPlayMode : MonoBehaviour
{

    void Play()
    {
        Debug.Log("Called");
        var VRM = GameObject.Find("AVATAR");
        VRM.AddComponent<ThirdPersonCharacter>();
        VRM.AddComponent<ThirdPersonUserControl>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
