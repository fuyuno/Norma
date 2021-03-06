﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Norma.Delta.Models
{
    public class Channel
    {
        public string ChannelId { get; set; }

        public string Name { get; set; }

        //[Index(IsUnique = true)]
        [Column("OrderIndex")]
        public int Order { get; set; }

        public virtual ICollection<Slot> Slots { get; set; }
    }
}