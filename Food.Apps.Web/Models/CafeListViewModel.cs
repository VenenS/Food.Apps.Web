using System.Collections.Generic;
using System.Linq;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Models
{
    public class CafeListViewModel
    {
        private List<CafeModel> _allCafes;
        private List<CafeModel> _availableCafes;
        private List<CafeModel> _searchCafes;

        public CafeListViewModel(List<CafeModel> allCafes, List<CafeModel> availableCafes, List<CafeModel> searchCafes = null)
        {
            _allCafes = allCafes ?? new List<CafeModel>();
            _availableCafes = availableCafes ?? new List<CafeModel>();
            _searchCafes = searchCafes ?? new List<CafeModel>();
        }

        public List<CafeModel> GetAvailableCafes()
        {
            var list = _allCafes.Intersect(_availableCafes, new CafeComparer()).ToList();
            return GetSearchCafes(list);
        }
        
        public List<CafeModel> GetUnavailableCafes()
        {
            var list = _allCafes.Except(_availableCafes, new CafeComparer()).ToList();
            return GetSearchCafes(list);
        }

        /// <summary>
        /// Оставляем кафе из поиска
        /// </summary>
        /// <param name="cafes"></param>
        /// <returns></returns>
        private List<CafeModel> GetSearchCafes(List<CafeModel> cafes)
        {
            return _searchCafes.Count > 0 ? cafes.Intersect(_searchCafes, new CafeComparer()).ToList() : cafes;
        }
    }
    
    class CafeComparer : IEqualityComparer<CafeModel> 
    {
        public bool Equals(CafeModel x, CafeModel y)
        {
            if (x == null && y == null)
                return true;
            if (x == null | y == null)
                return false;
            if(x.Id == y.Id)
                return true;
            return false;
        }

        public int GetHashCode(CafeModel obj)
        {
            string hCode = obj.Id + obj.Name + obj.Uuid;
            return hCode.GetHashCode();
        }
    }
}