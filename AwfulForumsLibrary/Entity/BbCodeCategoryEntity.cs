using System.Collections.Generic;

namespace AwfulForumsLibrary.Entity
{
    public class BbCodeCategoryEntity
    {
        public BbCodeCategoryEntity()
        {
            BbCodes = new List<BbCodeEntity>();
        }

        public virtual ICollection<BbCodeEntity> BbCodes { get; set; }

        public string Name { get; set; }
    }

    public class BbCodeEntity
    {
        public string Title { get; set; }

        public string Code { get; set; }
    }
}
