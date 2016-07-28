using System.Collections.Generic;

namespace AwfulForumsLibrary.Models.Polls
{
    public class PollGroup
    {
        public string Title { get; set; }

        public int Id { get; set; }

        public List<PollItem> PollList { get; set; }
    }
}
