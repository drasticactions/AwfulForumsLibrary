using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace AwfulForumsLibrary.Entity
{
    public class DraftEntity
    {
        [PrimaryKey]
        public long Id { get; set; }

        public long ThreadId { get; set; }

        public string Draft { get; set; }

        public int ForumId { get; set; }

        [ManyToOne]
        public ForumEntity Forum { get; set; }

        public bool NewThread { get; set; }
    }
}
