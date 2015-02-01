using System.Collections.Generic;
using AwfulForumsLibrary.Entity;

namespace AwfulForumsLibrary.Manager
{
    public static class BbCodeManager
    {
        private static List<BbCodeCategoryEntity> bbCodes;

        public static List<BbCodeCategoryEntity> BBCodes
        {
            get { return bbCodes ?? (bbCodes = GetBbCodes()); }
        }

        private static List<BbCodeCategoryEntity> GetBbCodes()
        {
            var bbCodeCategoryList = new List<BbCodeCategoryEntity>();
            var bbCodeList = new List<BbCodeEntity>
            {
                new BbCodeEntity()
                {
                    Code = "url",
                    Title = "url"
                },
                new BbCodeEntity()
                {
                    Code = "email",
                    Title = "email"
                },
                new BbCodeEntity()
                {
                    Code = "img",
                    Title = "img"
                },
                new BbCodeEntity()
                {
                    Code = "timg",
                    Title = "timg"
                },
                new BbCodeEntity()
                {
                    Code = "video",
                    Title = "video"
                },
                new BbCodeEntity()
                {
                    Code = "b",
                    Title = "b"
                },
                new BbCodeEntity()
                {
                    Code = "s",
                    Title = "s"
                },
                new BbCodeEntity()
                {
                    Code = "u",
                    Title = "u"
                },
                new BbCodeEntity()
                {
                    Code = "i",
                    Title = "i"
                },
                new BbCodeEntity()
                {
                    Code = "spoiler",
                    Title = "spoiler"
                },
                new BbCodeEntity()
                {
                    Code = "fixed",
                    Title = "fixed"
                },
                new BbCodeEntity()
                {
                    Code = "super",
                    Title = "super"
                },
                new BbCodeEntity()
                {
                    Code = "sub",
                    Title = "sub"
                },
                new BbCodeEntity()
                {
                    Code = "size",
                    Title = "size"
                },
                new BbCodeEntity()
                {
                    Code = "color",
                    Title = "color"
                },
                new BbCodeEntity()
                {
                    Code = "quote",
                    Title = "quote"
                },
                new BbCodeEntity()
                {
                    Code = "url",
                    Title = "url"
                },
                new BbCodeEntity()
                {
                    Code = "pre",
                    Title = "pre"
                },
                new BbCodeEntity()
                {
                    Code = "code",
                    Title = "code"
                },
                new BbCodeEntity()
                {
                    Code = "php",
                    Title = "php"
                },
                new BbCodeEntity()
                {
                    Code = "list",
                    Title = "list"
                }
            };
            bbCodeCategoryList.Add(new BbCodeCategoryEntity()
            {
                Name = "BBCode",
                BbCodes = bbCodeList
            });
            return bbCodeCategoryList;
        }
    }
}
