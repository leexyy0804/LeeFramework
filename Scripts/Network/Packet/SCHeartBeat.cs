﻿using ProtoBuf;
using System;

namespace FirstBattle
{
    [Serializable, ProtoContract(Name = @"SCHeartBeat")]
    public class SCHeartBeat : SCPacketBase
    {
        public SCHeartBeat()
        {
        }

        public override int Id
        {
            get
            {
                return 2;
            }
        }

        public override void Clear()
        {
        }
    }
}