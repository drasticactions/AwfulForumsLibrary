using System;
using System.Collections.Generic;
using System.Text;

namespace AwfulForumsLibrary.Tools
{
    public class EndPoints
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063";

        public const string SavedForum = "SavedForum";

        public const string SavedThread = "SavedThread";

        public const string GotoNewPost = "&goto=newpost";

        public const string PerPage = "&perpage=40";

        public const string UserProfile = "member.php?action=getinfo&userid={0}";

        public const string UserRapSheet = BaseUrl + "banlist.php?userid={0}";

        public const string RapSheet = BaseUrl + "banlist.php?pagenumber={0}";

        public const string StatsBase = BaseUrl + "stats.php?statid={0}&all=#jump";

        public const string StatsSpecificBase = BaseUrl + "stats.php?statid={0}&t_forumid=1{1}&all=#jump";

        public const string ForumListPage = "https://forums.somethingawful.com/forumdisplay.php?forumid=48";

        public const string ForumPage = BaseUrl + "forumdisplay.php?forumid={0}";

        public const string QuoteExp = "[quote=\"{0}\" post=\"{1}\"]{2}[/quote]";

        public const string ResetSeen = "action=resetseen&threadid={0}&json=1";

        public const string ResetBase = BaseUrl + "showthread.php";

        public const string Bookmark = BaseUrl + "bookmarkthreads.php";

        public const string LastRead = BaseUrl + "showthread.php?action=setseen&index={0}&threadid={1}";

        public const string RemoveBookmark = "json=1&action=remove&threadid={0}";

        public const string AddBookmark = "json=1&action=add&threadid={0}";

        public const string NewThread = BaseUrl + "newthread.php?action=newthread&forumid={0}";

        public const string NewPrivateMessage = BaseUrl + "private.php?action=newmessage";

        public const string NewPrivateMessageBase = BaseUrl + "private.php";

        public const string NewThreadBase = BaseUrl + "newthread.php";

        public const string NewReply = BaseUrl + "newreply.php";

        public const string EditPost = BaseUrl + "editpost.php";

        public const string ReplyBase = BaseUrl + "newreply.php?action=newreply&threadid={0}";

        public const string QuoteBase = BaseUrl + "newreply.php?action=newreply&postid={0}";

        public const string EditBase = BaseUrl + "editpost.php?action=editpost&postid={0}";

        public const string UserPostHistory = BaseUrl + "search.php?action=do_search_posthistory&userid={0}";

        public const string PrivateMessages = BaseUrl + "private.php";

        public const string PageNumber = "&pagenumber={0}";

        public const string ThreadPage = BaseUrl + "showthread.php?threadid={0}";

        public const string FrontPage = "https://www.somethingawful.com";

        public const string SmileUrl = BaseUrl + "misc.php?action=showsmilies";

        public const string ShowPost = BaseUrl + "showthread.php?action=showpost&postid={0}";

        public const string UserCp = BaseUrl + "usercp.php?";

        public const string HtmlFile = "{0}.html";

        public const int DefaultTimeoutInMilliseconds = 60000;

        public const string CookieDomainUrl = "https://fake.forums.somethingawful.com";

        public const string LoginUrl = "https://forums.somethingawful.com/account.php?";

        public const string SAclopediaBase = BaseUrl + "dictionary.php";

        public const string BaseUrl = "https://forums.somethingawful.com/";

        public const string SearchUrl = BaseUrl + "query.php";

        public const string BookmarksUrl = BaseUrl + "bookmarkthreads.php?perage=40&sortorder=desc&sortfield=";
    }
}
