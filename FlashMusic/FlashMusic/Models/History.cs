﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlashMusic.Models
{
    public class History
    {
        public int UserId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime PayTime { get; set; }
        public int Num { get; set; }

    }
}
