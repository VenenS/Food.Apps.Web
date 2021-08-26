using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public class TagService : BaseClient<TagService>, ITagService
    {
        private static readonly string ServiceUrl =
            FoodApps.Web.NetCore.Startup.Configuration.GetValue<string>("AppSettings:ServicePath") ?? "http://services.food.itwebnet.ru/";

        private static readonly string BaseUrl = ServiceUrl + "api/tags";

        public TagService() : base(ServiceUrl)
        {
        }

        public TagService(string baseAddress) : base(baseAddress)
        {
        }

        public TagService(bool debug) : base(debug)
        {
        }

        public TagService(string baseAddress, string token) : base(baseAddress, token)
        {
        }

        public virtual async Task<bool> AddNewTag(TagModel model)
        {
            var response = await PutAsync<bool>($"{BaseUrl}/add", model);
            return response.Content;
        }

        public virtual async Task<bool> AddTagToCafe(long cafeId, long tagId)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/addtocafe/cafe/{cafeId}/tag/{tagId}");
            return response.Content;
        }

        public virtual async Task<bool> AddTagToDish(long dishId, long tagId)
        {
            var response = await GetAsync<bool>($"{BaseUrl}/addtodish/dish/{dishId}/tag/{tagId}");
            return response.Content;
        }

        public virtual async Task<bool> EditTag(TagModel model)
        {
            var response = await PutAsync<bool>($"{BaseUrl}/edit", model);
            return response.Content;
        }

        public virtual async Task<List<TagModel>> GetAllTags()
        {
            var response = await GetAsync<List<TagModel>>($"{BaseUrl}");
            return response.Content;
        }

        public virtual async Task<List<TagModel>> GetChildTagsByTagId(long tagId)
        {
            var response = await GetAsync<List<TagModel>>($"{BaseUrl}/childrenbyparentid/{tagId}");
            return response.Content;
        }

        public virtual async Task<List<FoodDishModel>> GetDishesByTagListAndCafeId(List<long> tagIds, long cafeId)
        {
            var response =
                await PostAsync<List<FoodDishModel>>($"{BaseUrl}/getDishesByTagListAndCafeId/{cafeId}", tagIds);
            return response.Content;
        }

        public virtual async Task<List<FoodCategoryModel>> GetFoodCategoriesSearchTermAndTagsAndCafeId(string searchTerm,
            List<long> tagIds, long cafeId)
        {
            var response = await PostAsync<List<FoodCategoryModel>>(
                $"{BaseUrl}/getFoodCategoriesSearchTermAndTagsAndCafeId?searchTerm={searchTerm}&cafeId={cafeId}",
                tagIds);
            return response.Content;
        }

        public virtual async Task<List<FoodCategoryModel>> GetFoodCategoriesTagsAndCafeId(List<long> tagIds, long cafeId)
        {
            var response =
                await PostAsync<List<FoodCategoryModel>>($"{BaseUrl}/getFoodCategoriesTagsAndCafeId/{cafeId}", tagIds);
            return response.Content;
        }

        public virtual async Task<List<CafeModel>> GetListOfCafesBySearchTermAndTagsList(string searchTerm, List<long> tagIds)
        {
            var response =
                await PostAsync<List<CafeModel>>(
                    $"{BaseUrl}/getListOfCafesBySearchTermAndTagsList?searchTerm={searchTerm}", tagIds);
            return response.Content;
        }

        public virtual async Task<List<CafeModel>> GetListOfCafesByTagsList(List<long> tagIds)
        {
            var response = await PostAsync<List<CafeModel>>($"{BaseUrl}/getListOfCafesByTagsList", tagIds);
            return response.Content;
        }

        public virtual async Task<List<TagModel>> GetListOfTagsByString(string textToFind)
        {
            var response = await GetAsync<List<TagModel>>($"{BaseUrl}/tagsByString?textToFind={textToFind}");
            return response.Content;
        }

        public virtual async Task<List<TagModel>> GetListOfTagsConnectedWithObjectAndHisChild(long objectId, long typeOfObject)
        {
            var response =
                await GetAsync<List<TagModel>>(
                    $"{BaseUrl}/getListOfTagsConnectedWithObjectAndHisChild/{objectId}/{typeOfObject}");
            return response.Content;
        }

        public virtual async Task<List<TagModel>> GetRootTags()
        {
            var response = await GetAsync<List<TagModel>>($"{BaseUrl}/roottags");
            return response.Content;
        }

        public virtual async Task<TagModel> GetTagByTagId(long tagId)
        {
            var response = await GetAsync<TagModel>($"{BaseUrl}/tagbyid/{tagId}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveTag(long tagId)
        {
            var response = await DeleteAsync<bool>($"{BaseUrl}/tagbyid/{tagId}");
            return response.Content;
        }

        public virtual async Task<bool> RemoveTagObjectLink(long objectId, int objectType, long tagId)
        {
            var response = await DeleteAsync<bool>($"{BaseUrl}/removeTagObjectLink/{objectId}/{objectType}/{tagId}");
            return response.Content;
        }
    }
}