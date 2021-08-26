using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ITagService
    {
        Task<bool> AddNewTag(TagModel model);
        Task<bool> AddTagToCafe(long cafeId, long tagId);
        Task<bool> AddTagToDish(long dishId, long tagId);
        Task<bool> EditTag(TagModel model);
        Task<List<TagModel>> GetAllTags();
        Task<List<TagModel>> GetChildTagsByTagId(long tagId);
        Task<List<FoodDishModel>> GetDishesByTagListAndCafeId(List<long> tagIds, long cafeId);
        Task<List<FoodCategoryModel>> GetFoodCategoriesSearchTermAndTagsAndCafeId(string searchTerm, List<long> tagIds, long cafeId);
        Task<List<FoodCategoryModel>> GetFoodCategoriesTagsAndCafeId(List<long> tagIds, long cafeId);
        Task<List<CafeModel>> GetListOfCafesBySearchTermAndTagsList(string searchTerm, List<long> tagIds);
        Task<List<CafeModel>> GetListOfCafesByTagsList(List<long> tagIds);
        Task<List<TagModel>> GetListOfTagsByString(string textToFind);
        Task<List<TagModel>> GetListOfTagsConnectedWithObjectAndHisChild(long objectId, long typeOfObject);
        Task<List<TagModel>> GetRootTags();
        Task<TagModel> GetTagByTagId(long tagId);
        Task<bool> RemoveTag(long tagId);
        Task<bool> RemoveTagObjectLink(long objectId, int objectType, long tagId);
    }
}