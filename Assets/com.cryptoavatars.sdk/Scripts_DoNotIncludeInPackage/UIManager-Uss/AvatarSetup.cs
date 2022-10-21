using BloodUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSetup : MonoBehaviour
{
    public AvatarWindow avatarWindow;
    // Start is called before the first frame update
    void Start()
    {
        avatarWindow = new AvatarWindow();
    }
}
