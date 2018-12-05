﻿using System;
using System.Linq;
using ProtoBuf;
using Base58Check;
using AElf.Common;

namespace AElf.Automation.Common.Protobuf
{
    [ProtoContract]
    public class Address
    {
        public Address(byte[] val)
        {
            Value = val;
        }

        [ProtoMember(1)]
        public byte[] Value { get; set; }

        public static implicit operator Address(byte[] value)
        {
            return new Address(value);
        }

        public static Address Parse(string inputStr)
        {
            string[] split = inputStr.Split('_');

            if (split.Length != 3)
                return null;

            if (String.CompareOrdinal(split[0], "ELF") != 0)
                return null;

            if (split[1].Length != 4)
                return null;

            var chainId = Base58CheckEncoding.DecodePlain(split[1]);
            var bytes = Base58CheckEncoding.Decode(split[2]);

            if (bytes.Length != GlobalConfig.AddressHashLength)
            {
                throw new ArgumentOutOfRangeException(
                    $"Address (sha256 of pubkey) bytes has to be {GlobalConfig.AddressHashLength}. The input is {bytes.Length} bytes long.");
            }

            if (chainId.Length != GlobalConfig.ChainIdLength)
            {
                throw new ArgumentOutOfRangeException(
                    $"The chain id length has to be {GlobalConfig.ChainIdLength}. The input is {bytes.Length} bytes long.");
            }

            return new Address(ByteArrayHelpers.Combine(chainId, bytes));
        }

        public string GetFormatted()
        {
            if (Value.Length != GlobalConfig.AddressHashLength + GlobalConfig.ChainIdLength)
            {
                throw new ArgumentOutOfRangeException(
                    $"Serialized value does not represent a valid address. The input is {Value.Length} bytes long.");
            }

            string chainId = Base58CheckEncoding.EncodePlain(Value.Take(3).ToArray());
            string pubKeyHash = Base58CheckEncoding.Encode(Value.Skip(3).ToArray());

            return GlobalConfig.AElfAddressPrefix + '_' + chainId + '_' + pubKeyHash;
        }
    }

    [ProtoContract]
    public class Hash
    {
        public Hash(byte[] val)
        {
            Value = val;
        }

        [ProtoMember(1)]
        public byte[] Value { get; set; }

        public static implicit operator Hash(byte[] value)
        {
            return new Hash(value);
        }
    }
}
