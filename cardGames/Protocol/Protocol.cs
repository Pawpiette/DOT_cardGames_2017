﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public enum Cmd
    {
        Ready=0xff00,
        DrawCard,
    }

    public enum Card { None=-1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace };

    [Serializable]
    public class Protocol
    {
        public Cmd Command { get; set; }
        public Card CardSend { get; set; }

        public Protocol(Cmd cmd, Card card)
        {
            Command = cmd;
            CardSend = card;
        }
    }
}
