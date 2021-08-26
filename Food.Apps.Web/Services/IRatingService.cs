using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.Food.Site.Services
{
    public interface IRatingService
    {
        Task<List<RatingModel>> GetAllRatingFromUser(bool isFilter, ObjectTypesEnum typeOfObject);
        Task<List<RatingModel>> GetAllRatingToObject(long objectId, ObjectTypesEnum objectType);
        Task<double> GetFinalRateToCafe(long cafeId);
        Task<double> GetFinalRateToDish(long dishId);
        Task<long> InsertNewRating(long objectId, ObjectTypesEnum typeOfObject, int valueOfRating);
    }
}