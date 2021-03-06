﻿using System;
using LiteDB;

namespace Bot.DataAccessLayer.Entities
{
    public class MoneyTransaction
    {
        public ObjectId Id { get; set; }
        public float Sum { get; set; }
        public string Sign { get; set; }
        public string CategoryName { get; set; }
        public string CategoryHumanName { get; set; }
        public string CategoryIcon { get; set; }
        public DateTime RegisterDate { get; set; }
        public string FromUserName { get; set; }
    }
}