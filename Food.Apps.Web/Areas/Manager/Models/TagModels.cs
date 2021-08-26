using System.Collections.Generic;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class TagsModel
    {
        public long Id { get; set; }
        public long CafeId { get; set; }
        public string ObjectName { get; set; }
        public List<TagModel> AvailableTags { get; set; }
        public List<TagModel> ObjectTags { get; set; }
        public ObjectTypesEnum ObjectType { get; set; }
        public TagsModel()
        {
            AvailableTags = new List<TagModel>();
            ObjectType = ObjectTypesEnum.ANOTHER_TYPE;
            Id = -1;
            ObjectTags = new List<TagModel>();
        }
    }

    public class FilterObjects
    {
        public long CafeId { get; set; }
        public string SearchString { get; set; }
        public Dictionary<FoodCategoryModel, List<Core.DataContracts.Common.FoodDishModel>> Objects { get; set; }
    }
}