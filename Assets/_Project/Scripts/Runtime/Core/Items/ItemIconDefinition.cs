using System;
using UnityEngine;

namespace PH.Core.Items
{
    [Serializable]
    public sealed class ItemIconDefinition
    {
        [SerializeField]
        private string iconKey;

        [SerializeField]
        private string localAddress;

        [SerializeField]
        private string fallbackIconKey;

        [SerializeField]
        private string remotePath;

        [SerializeField]
        private string hash;

        [SerializeField]
        private int version;

        public string IconKey => iconKey;
        public string LocalAddress => localAddress;
        public string FallbackIconKey => fallbackIconKey;
        public string RemotePath => remotePath;
        public string Hash => hash;
        public int Version => version;

        public static ItemIconDefinition Create(
            string iconKey,
            string localAddress,
            string fallbackIconKey,
            string remotePath,
            string hash,
            int version)
        {
            return new ItemIconDefinition
            {
                iconKey = iconKey,
                localAddress = localAddress,
                fallbackIconKey = fallbackIconKey,
                remotePath = remotePath,
                hash = hash,
                version = Mathf.Max(1, version)
            };
        }
    }
}
