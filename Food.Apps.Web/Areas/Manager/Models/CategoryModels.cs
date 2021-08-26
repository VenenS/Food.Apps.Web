using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Site.Areas.Manager.Models
{
    public class CategoryModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public int Index { get; set; }

        public IList<DishModel> Dishes { get; set; }
    }

    public class DishFilter
    {
        public string SearchQuery { get; set; }
    }

    public class CategoryCollectionModel : IList<CategoryModel>
    {
        public long CafeId { get; set; }

        public DishFilter Filter { get; set; }

        protected IList<CategoryModel> InnerList { get; set; }

        protected HashSet<string> SystemCategoriesSet { get; set; } = new HashSet<string>{ "блюда удаленных категорий", "удаленные блюда" };

        public int Count
        {
            get
            {
                return ((IList<CategoryModel>)InnerList).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<CategoryModel>)InnerList).IsReadOnly;
            }
        }

        public CategoryModel this[int index]
        {
            get
            {
                return ((IList<CategoryModel>)InnerList)[index];
            }

            set
            {
                ((IList<CategoryModel>)InnerList)[index] = value;
            }
        }

        public CategoryCollectionModel(long cafeId)
        {
            InnerList = new List<CategoryModel>();
            CafeId = cafeId;
            Filter = new DishFilter();
        }

        public CategoryCollectionModel(long cafeId, IEnumerable<CategoryModel> collection)
        {
            InnerList = new List<CategoryModel>(collection);
            CafeId = cafeId;
        }

        public CategoryCollectionModel Update(IEnumerable<CategoryModel> collection)
        {
            InnerList = new List<CategoryModel>(collection);
            return this;
        }

        public IList<CategoryModel> GetActive()
        {
            List<CategoryModel> result = InnerList.Where(
                item => item.IsActive
                && !SystemCategoriesSet.Contains(item.Name.Trim().ToLower())).ToList();
            return result;
        }

        public IList<CategoryModel> GetInactive()
        {
            List<CategoryModel> result = InnerList.Where(
                item => !item.IsActive
                && !SystemCategoriesSet.Contains(item.Name.Trim().ToLower())).ToList();
            return result;
        }

        public IList<CategoryModel> GetSystemCategories()
        {
            List<CategoryModel> result = InnerList.Where(item => SystemCategoriesSet.Contains(item.Name.Trim().ToLower())).ToList();
            return result;
        }

        public CategoryCollectionModel FilterDishes(DishFilter filter)
        {
            foreach (var item in InnerList)
            {
                item.Dishes =
                    item.Dishes.Where(
                        d => !string.IsNullOrEmpty(d.Name) && d.Name.ToLower().Contains(filter.SearchQuery.ToLower()))
                        .ToList();
            }

            return this;
        }

        public int IndexOf(CategoryModel item)
        {
            return ((IList<CategoryModel>)InnerList).IndexOf(item);
        }

        public void Insert(int index, CategoryModel item)
        {
            ((IList<CategoryModel>)InnerList).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<CategoryModel>)InnerList).RemoveAt(index);
        }

        public void Add(CategoryModel item)
        {
            ((IList<CategoryModel>)InnerList).Add(item);
        }

        public void Clear()
        {
            ((IList<CategoryModel>)InnerList).Clear();
        }

        public bool Contains(CategoryModel item)
        {
            return ((IList<CategoryModel>)InnerList).Contains(item);
        }

        public void CopyTo(CategoryModel[] array, int arrayIndex)
        {
            ((IList<CategoryModel>)InnerList).CopyTo(array, arrayIndex);
        }

        public bool Remove(CategoryModel item)
        {
            return ((IList<CategoryModel>)InnerList).Remove(item);
        }

        public IEnumerator<CategoryModel> GetEnumerator()
        {
            return ((IList<CategoryModel>)InnerList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<CategoryModel>)InnerList).GetEnumerator();
        }
    }
}