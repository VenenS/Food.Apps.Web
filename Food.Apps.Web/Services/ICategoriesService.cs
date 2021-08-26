using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface ICategoriesService
    {
        Task<HttpResult<FoodCategoryModel>> CreateCategory(FoodCategoryModel company);
        Task<bool> DeleteCategory(long id);
        Task<HttpResult<List<FoodCategoryModel>>> GetCategories();
        Task<HttpResult<FoodCategoryModel>> GetCategoryById(long id);
        Task<HttpResult<FoodCategoryModel>> UpdateCategory(FoodCategoryModel model);
        Task<HttpResult<bool>> SetCategoryBusinessHours(CategoryBusinessHoursModel categoryBusinessHours);
        Task<CategoryBusinessHoursModel> GetCategoryBusinessHours(long cafeId, long categoryId);
    }
}