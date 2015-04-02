using System;
using System.Collections.ObjectModel;
using AwfulForumsLibrary.Tools;
using PropertyChanged;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace AwfulForumsLibrary.Entity
{
    [ImplementPropertyChanged]
    public class ForumThreadEntity
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public string ImageIconLocation { get; set; }
        // TODO: Add to unit tests.
        public string StoreImageIconLocation { get; set; }

        public string Author { get; set; }

        public int ReplyCount { get; set; }

        public int ViewCount { get; set; }

        public int Rating { get; set; }

        public string RatingImage { get; set; }

        public string KilledBy { get; set; }

        public bool IsSticky { get; set; }

        public bool IsLocked { get; set; }

        public bool IsAnnouncement { get; set; }

        public bool HasBeenViewed { get; set; }

        public bool CanMarkAsUnread { get; set; }

        public int RepliesSinceLastOpened { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int ScrollToPost { get; set; }

        public string ScrollToPostString { get; set; }

        [PrimaryKey]
        public long ThreadId { get; set; }

        [ForeignKey(typeof(ForumEntity))]
        public int ForumId { get; set; }

        public bool HasSeen { get; set; }

        [ManyToOne]
        public virtual ForumEntity ForumEntity { get; set; }

        public bool IsBookmark { get; set; }

        public PlatformIdentifier PlatformIdentifier { get; set; } 

        [Ignore]
        public ObservableCollection<ForumPostEntity> ForumPosts { get; set; }
        public bool IsPrivateMessage { get; set; }
    }
}
