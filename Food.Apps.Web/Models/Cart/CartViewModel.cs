using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITWebNet.Food.Site.Models
{
    public class CartMultiCafeViewModel
    {

        /// <summary>
        /// Ключ, под которым корзина пользователя будет сохраняться в сессию
        /// </summary>
        public const string CartKeyUser = "MultiCartUser";
        /// <summary>
        /// Ключ, под которым банкетная корзина будет сохраняться в сессию
        /// </summary>
        public const string CartKeyBanket = "MultiCartBanket";

        /// <summary>
        /// Идентификатор текущего кафе, в котором просматривается корзина
        /// </summary>
        public long? CurrentCafeId;

        public EnumCartType Type { get; set; }
        public Dictionary<long, CartViewModel> CafeCarts { get; set; }

        public CartMultiCafeViewModel()
        {
            CafeCarts = new Dictionary<long, CartViewModel>();
            Type = EnumCartType.Full;
        }

        public static CartMultiCafeViewModel Load(ISession session, EnumCartType type = EnumCartType.Short)
        {
            CartMultiCafeViewModel cart;
            if (type == EnumCartType.Banket)
            {
                cart = session.Get<CartMultiCafeViewModel>(CartKeyBanket) ?? new CartMultiCafeViewModel();
            }
            else
            {
                cart = session.Get<CartMultiCafeViewModel>(CartKeyUser) ?? new CartMultiCafeViewModel();
            }
            cart.Type = type;

            return cart;
        }

        public static void Save(ISession session, CartMultiCafeViewModel cartMulti)
        {
            if (cartMulti.Type == EnumCartType.Banket)
                session.Set(CartKeyBanket, cartMulti);
            else
                session.Set(CartKeyUser, cartMulti);
        }

        public static void ClearAllCarts(ISession session)
        {
            session.Set(CartKeyUser, new CartMultiCafeViewModel());
            session.Set(CartKeyBanket, new CartMultiCafeViewModel());
        }
    }

    public class CartViewModel
    {
        private const string CartNameKey = "UserCart";
        private const string BanketCartKey = "BanketCart";

        public CartViewModel()
        {
            CartItems = new List<CartItemViewModel>();
            Status = OrderStatusEnum.Cart;
            DeliverDate = DateTime.Now;
            ClientDiscount = 0;
            Type = EnumCartType.Full;
            DeliveryPriceInfo = new OrderDeliveryPriceModel();
            DeliveryPriceInfoByDates = new Dictionary<DateTime, OrderDeliveryPriceModel>();
        }

        public CafeModel Cafe { get; set; }

        public EnumCartType Type { get; set; }

        public int ClientDiscount { get; set; }

        public int Count
        {
            get { return ((IList<CartItemViewModel>)CartItems).Count; }
        }

        public DateTime DeliverDate { get; set; }

        public List<CartItemViewModel> CartItems { get; set; }

        /// <summary>
        /// Информация о стоимости доставки заказа, сгруппированная по датам. Нужна для заказов на неделю, когда один заказ может включать несколько доставок на разные даты.
        /// </summary>
        public Dictionary<DateTime, OrderDeliveryPriceModel> DeliveryPriceInfoByDates;

        public bool IsReadOnly
        {
            get { return ((IList<CartItemViewModel>)CartItems).IsReadOnly; }
        }

        public bool IsCompanyEmployee { get; set; }

        public int CountCompalyOrders { get; set; } = 0;

        /// <summary>
        /// Показывает, что сумма заказа меньше минимальной суммы, установленной для данного кафе
        /// </summary>
        public bool OrderBelowMinimal { get; set; } = false;

        public OrderStatusEnum Status { get; set; }

        public double TotalPrice
        {
            get { return CartItems.Sum(item => item.TotalPrice); }
        }

        public OrderDeliveryPriceModel DeliveryPriceInfo { get; set; }

        public CartItemViewModel this[int index]
        {
            get { return ((IList<CartItemViewModel>)CartItems)[index]; }

            set { ((IList<CartItemViewModel>)CartItems)[index] = value; }
        }

        public CartItemViewModel this[long dishId]
        {
            get { return CartItems.Find(item => item.Id == dishId); }
        }

        public CartItemViewModel this[string dishName]
        {
            get { return CartItems.Find(item => item.Name == dishName); }
        }

        public CartViewModel SetDefaults()
        {
            CartItems = new List<CartItemViewModel>();
            Status = OrderStatusEnum.Cart;
            DeliverDate = DateTime.Now;
            ClientDiscount = 0;

            return this;
        }

        public static CartViewModel Load(ISession session, EnumCartType type = EnumCartType.Short)
        {
            if (type == EnumCartType.Banket)
            {
                var cart = session.Get<CartViewModel>(BanketCartKey) ?? new CartViewModel();
                cart.Type = EnumCartType.Banket;
                return cart;
            }
            return session.Get<CartViewModel>(CartNameKey) ?? new CartViewModel();
        }

        public static void Save(ISession session, CartViewModel cart)
        {
            if (cart.Type == EnumCartType.Banket)
                session.Set(BanketCartKey, cart);
            else
                session.Set(CartNameKey, cart);
        }
    }

    public enum EnumCartType
    {
        Full,
        Short,
        Nav,
        DeliveryCost,
        Banket
    }
}