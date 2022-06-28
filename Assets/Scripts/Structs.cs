using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structs
{
    [System.Serializable]
    public struct LoginRequestDto
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public struct LoginResponseDto
    {
        public string userId;
    }

    [System.Serializable]
    public struct SearchAvatarsDto
    {
        public int skip;
        public int limit;
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

    [System.Serializable]
    public struct NftsArray
    {
        public Nft[] nfts;
        public int total;
        public int page;
        public string next;
    }

    [System.Serializable]
    public struct Nft
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
