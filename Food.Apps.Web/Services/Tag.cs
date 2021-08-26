using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site.Services.Client
{
    public partial class Tag
    {
        public List<Tag> Children { get; set; }

        public Tag Parent { get; set; }

        public bool HasChildren { get { return Children?.Count > 0; } }

        public bool IsSelected { get; set; }

        public static List<Tag> BuildTagsTree(List<Tag> allTags)
        {
            var groupedTags = allTags.GroupBy(item => item.ParentId);

            List<Tag> tree = new List<Tag>();

            foreach (var group in groupedTags)
            {
                if (group.Key.HasValue)
                {
                    Tag parent = allTags.Find(item => item.Id == group.Key);
                    if (parent != null)
                    {
                        parent.Children = group.ToList();
                        foreach (var item in group)
                            item.Parent = parent;
                    }
                }
                else
                    tree.AddRange(group.ToList());
            }


            return tree;
        }
    }
}