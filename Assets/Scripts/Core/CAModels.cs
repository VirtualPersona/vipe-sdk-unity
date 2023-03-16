namespace CA
{

    public class CAModels
    {
        [System.Serializable]
        public struct LoginRequestDto
        {
            public string email;
            public string password;
        }

        [System.Serializable]
        public struct LoginWeb3RequestDto
        {
            public string wallet;
            public string signature;
        }

        [System.Serializable]
        public struct LoginResponseDto
        {
            public string userId;
            public string wallet;
            public string accessToken;
        }

        [System.Serializable]
        public struct SearchAvatarsDto
        {
            public string license;
            public string name;
            public string collectionName;
            public string owner;
        }

        [System.Serializable]
        public struct NftsArray
        {
            public Nft[] nfts;
            public int totalNfts;
            public int currentPage;
            public int totalPages;
            public string next;
            public string prev;
        }

        [System.Serializable]
        public struct Nft
        {
            public string owner;
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
                public Asset[] assets;
                public string[] tags;
            }
        }

        [System.Serializable]
        public struct Asset
        {
            public string media_type;
            public string asset_type;
            public int lod_type;
            public AssetStats stats;
            public AssetFile[] files;
        }

        [System.Serializable]
        public struct AssetStats
        {
            public int triangles;
            public int meshes;
            public int bounds;
            public int numberOfTextures;
            public int texturesResolution;
            public int fileSize;
        }

        [System.Serializable]
        public struct AssetFile
        {
            public string url;
            public string file_type;
        }

        [System.Serializable]
        public struct NftsCollection
        {
            public bool isEnabled;
            public string logoImage;
            public string bannerImage;
            public string id;
            public string name;
            public CollectionContracts collectionContracts;
            public int owners;
            public string description;
            public SocialLinks socialLinks;
            public int totalSupply;
            public int floorPrice;
            public int volume;
            public int bestOffer;
            public string mobileBannerImage;
            [System.Serializable]
            public struct SocialLinks
            {
                public string twitter;
                public string website;
                public string discord;
                public string instagram;
                public string twitch;
            }
            [System.Serializable]
            public struct CollectionContracts
            {
                public string contractAddress;
                public string chainId;
            }
        }

        [System.Serializable]
        public struct NftsCollectionsArray
        {
            public NftsCollection[] nftsCollections;
        }
    }
}
