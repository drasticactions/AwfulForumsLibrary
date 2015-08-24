using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwfulForumsLibrary.Entity
{
    public class PollGroupEntity
    {
        public string Title { get; set; }

        public int Id { get; set; }

        public List<PollEntity> PollList { get; set; } 
    }

    public class PollEntity
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}
