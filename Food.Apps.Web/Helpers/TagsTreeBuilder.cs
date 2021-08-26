using System.Collections.Generic;
using System.Linq;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Helpers
{
    public static class TagsTreeBuilder
    {
        public static List<TagModel> BuildTagsTree(List<TagModel> allTags)
        {
            var groupedTags = allTags.GroupBy(item => item.ParentId);

            var tree = new List<TagModel>();

            foreach (var group in groupedTags)
                if (group.Key.HasValue)
                {
                    var parent = allTags.Find(item => item.Id == group.Key);
                    if (parent != null)
                    {
                        parent.Children = group.ToList();
                        foreach (var item in group)
                            item.Parent = parent;
                    }
                }
                else
                {
                    tree.AddRange(group.ToList());
                }


            return tree;
        }
    }
}