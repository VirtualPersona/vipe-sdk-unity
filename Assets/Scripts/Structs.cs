using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structs
{
    [System.Serializable]
    public struct LoginDto
    {
        public string userId;
    }

    [System.Serializable]
    public struct AvatarsArray
    {
        public Avatar[] avatars;
    }

    [System.Serializable]
    public struct Avatar
    {
        public Metadata metadata;

        [System.Serializable]
        public struct Metadata
        {
            public string name;
            public string description;
            public string image;
            public string asset;
            public string createdBy;
            public string createdAt;
            public string[] tags;
        }
    }
}
