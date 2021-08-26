using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace ITWebNet.Food.Site
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        const string _myCompanyNameForOrder = "MyCompanyNameForOrder";
        const string _myCompanyIdForOrder = "MyCompanyIdForOrder";
        private const string _deliveryAddressKey = "CurrentDeliveryAddress";
        private const string _currentCityKey = "CurrentCity";
        private const string _currentCityIsConfirmedKey = "IsCityConfirmed";
        /// <summary> 
        /// Get value. 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="session"></param> 
        /// <param name="key"></param> 
        /// <returns></returns> 
        public static T Get<T>(this ISession session, string key)
        {
            if (key == null)
                return default(T);
            var result = session.GetString(key);
            return result == null ? default(T) : JsonConvert.DeserializeObject<T>(result, _jsonSerializerSettings);
        }
        /// <summary> 
        /// Set value. 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="session"></param> 
        /// <param name="key"></param> 
        /// <param name="value"></param> 
        public static void Set<T>(this ISession session, string key, T value)
        {
            if (key == null) return;
            var json = JsonConvert.SerializeObject(value);
            session.SetString(key, json);
        }

        public static void SetCurrentDeliveryAddress(this ISession session, string newAddress)
        {
            session.SetString(_deliveryAddressKey, newAddress);
        }

        public static string GetCurrentDeliveryAddress(this ISession session)
        {
            return session.GetString(_deliveryAddressKey);
        }

        public static void SetCurrentCity(this ISession session, CityModel city, bool isConfirmed)
        {
            session.Set(_currentCityKey, city);
            session.Set(_currentCityIsConfirmedKey, isConfirmed);
        }

        public static CityModel GetCurrentCity(this ISession session) => session.Get<CityModel>(_currentCityKey);

        public static bool IsCityConfirmed(this ISession session) => session.Get<bool>(_currentCityIsConfirmedKey);

        public static void RemoveCity(this ISession session)
        {
            session.Remove(_currentCityIsConfirmedKey);
            session.Remove(_currentCityKey);
        }
    }
}