using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Site.Areas.Manager.Models;
using ITWebNet.Food.Site.Helpers;
using ITWebNet.Food.Site.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodDishModel = ITWebNet.Food.Core.DataContracts.Common.FoodDishModel;

namespace ITWebNet.Food.Site.Areas.Manager.Controllers
{
    public class TagsController : BaseController
    {
        //Food.Services.Proxies.FoodServiceManagerClient client;
        IDishCategoryService dishCategoryService;
        IDishService dishServiceClient;
        ITagService tagServiceClient;

        public TagsController(
            IMemoryCache cache,
            ICafeService cafeService,
            IDishCategoryService dishCategoryService,
            IDishService dishServiceClient,
            ITagService tagServiceClient)
            : base(cache, cafeService)
        {
            this.dishCategoryService = dishCategoryService;
            this.dishServiceClient = dishServiceClient;
            this.tagServiceClient = tagServiceClient;
        }

        public async Task<ActionResult> Index(long cafeId)
        {
            FilterObjects model = new FilterObjects()
            {
                CafeId = cafeId,
                Objects = await dishServiceClient.GetFoodCategoryAndFoodDishesByCafeIdAndDate(cafeId, null),
                SearchString = string.Empty
            };
            return View(model);
        }

        public async Task<ActionResult> GetTags(long id, ObjectTypesEnum objectType, long cafeId)
        {
            TagsModel model = new TagsModel();
            model.ObjectTags = await tagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(id, (long)objectType);

            return PartialView("_ObjectTags", model);
        }

        public async Task<ActionResult> GetObjects(FilterObjects model)
        {

            model.Objects = await dishServiceClient.GetFoodCategoryAndFoodDishesByCafeIdAndDate(model.CafeId, null);
            if (!string.IsNullOrEmpty(model.SearchString))
            {
                string search = model.SearchString.ToLower();
                Dictionary<FoodCategoryModel, List<FoodDishModel>> filter =
                    model.Objects.Where(k => k.Key.Name.ToLower().Contains(search))
                    .ToDictionary(i => i.Key, i => i.Value);

                if (filter.Count == 0)
                {
                    model.Objects =
                        model.Objects
                        .ToDictionary(i => i.Key, i => i.Value.Where(l => l.Name.ToLower().Contains(search)).ToList())
                        .Where(i => i.Value.Count > 0)
                        .ToDictionary(i => i.Key, i => i.Value);
                }
                else
                    model.Objects = filter;
            }

            return PartialView("_ListObjects", model);
        }

        public async Task<ActionResult> EditTags(ObjectTypesEnum objectType, long id, long cafeId)
        {
            TagsModel model = new TagsModel();
            model.ObjectTags = await tagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(id, (long)objectType);

            model.Id = id;
            model.ObjectType = objectType;
            var alltags = await tagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(cafeId, (long)ObjectTypesEnum.CAFE);
            model.AvailableTags = TagsTreeBuilder.BuildTagsTree(alltags);
            model.ObjectTags = await tagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(id, (long)objectType);

            switch (objectType)
            {
                case ObjectTypesEnum.DISH:
                    var dish = await dishServiceClient.GetFoodDishById(id);
                    model.ObjectName = dish.Name;
                    break;
                case ObjectTypesEnum.CATEGORY:
                    var cat = await dishCategoryService.GetFoodCategoryById(id);
                    model.ObjectName = cat.Name;
                    break;
                case ObjectTypesEnum.CAFE:
                    var cafe = await _cafeService.GetCafeById(cafeId);
                    model.ObjectName = cafe.Name;
                    break;
            }

            return View("Tags", model);
        }



        public async Task<ActionResult> AddTag(long id, long tagId, ObjectTypesEnum objectType)
        {
            switch (objectType)
            {
                case ObjectTypesEnum.DISH:
                    bool result = await tagServiceClient.AddTagToDish(id, tagId);
                    break;
            }

            TagsModel model = new TagsModel();
            model.Id = id;
            model.ObjectType = objectType;
            model.ObjectTags = await tagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(id, (long)objectType);

            return PartialView("_ObjectTags", model);
        }

        public async Task<ActionResult> RemoveTag(long id, long tagId, ObjectTypesEnum objectType)
        {
            await tagServiceClient.RemoveTagObjectLink(id, (int)objectType, tagId);
            TagsModel model = new TagsModel();
            model.Id = id;
            model.ObjectType = objectType;
            model.ObjectTags = await tagServiceClient.GetListOfTagsConnectedWithObjectAndHisChild(id, (long)objectType);

            return PartialView("_ObjectTags", model);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = User.Identity.GetJwtToken();
            //client = new Food.Services.Proxies.FoodServiceManagerClient()
            //    .AddAuthorization(token);

            ((DishService)dishServiceClient)?.AddAuthorization(token);
            ((DishCategoryService)dishCategoryService)?.AddAuthorization(token);
            ((TagService)tagServiceClient)?.AddAuthorization(token);
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing && client != null)
            //{
            //    client.Close();
            //    client = null;
            //}

            base.Dispose(disposing);
        }
    }
}