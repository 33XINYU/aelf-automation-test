﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace AElf.Automation.Common.Protobuf
{
    [ProtoContract]
    public class Transaction
    {
        [ProtoMember(1)]
        public Hash From { get; set; }

        [ProtoMember(2)]
        public Hash To { get; set; }

        [ProtoMember(3)]
        public UInt64 IncrementId { get; set; }

        [ProtoMember(4)]
        public string MethodName { get; set; }

        [ProtoMember(5)]
        public byte[] Params { get; set; }

        [ProtoMember(6)]
        public UInt64 Fee { get; set; }

        [ProtoMember(7)]
        public byte[] R { get; set; }

        [ProtoMember(8)]
        public byte[] S { get; set; }

        [ProtoMember(9)]
        public byte[] P { get; set; }

        [ProtoMember(10)]
        public TransactionType type { get; set; }
    }

    [ProtoContract]
    public enum TransactionType
    {
        ContractTransaction = 0,
        DposTransaction = 1,

    }
}
