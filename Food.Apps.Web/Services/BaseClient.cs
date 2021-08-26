using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace ITWebNet.Food.Site.Services
{
    public abstract class BaseClient<TClient> : IDisposable
        where TClient : BaseClient<TClient>
    {
        internal HttpClient HttpClient { get; set; }

        internal HttpMessageHandler Handler { get; set; }

        public BaseClient(string baseAddress)
        {
            CookieContainer cookies = new CookieContainer();
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri(baseAddress);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public BaseClient(string baseAddress, IHttpContextAccessor contextAccessor)
            : this(baseAddress)
        {
            var context = contextAccessor.HttpContext;
            if (context == null)
                return;

            if (context.User.Identity.IsAuthenticated) {
                var token = context.User.Identity.GetJwtToken();
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public BaseClient(string baseAddress, string token)
            : this(baseAddress)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public TClient AddAuthorization(string token)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return (TClient)this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }


        protected virtual async Task PostAsync(string address, object model)
        {
            try
            {
                await HttpClient.PostAsJsonAsync(address, model);
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException)
                    throw ex.InnerException;
                else
                    throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        protected virtual async Task<HttpResult<TResult>> PostAsync<TResult>
            (string address, object model)
        {
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsJsonAsync(address, model);
            }
            catch (HttpRequestException ex)
            {
                return HttpResult<TResult>.Failure(ex.Message);
            }

            return await GetResponseResult<TResult>(response);
        }

        protected virtual async Task<HttpResult<TResult>> GetAsync<TResult>(string address, string query = null, long? value = null)
        {
            HttpResponseMessage response;
            try
            {
                System.Diagnostics.Debug.WriteLine($">>> {address} - {query} - {value}");
                if (query == null && value == null)
                    response = await HttpClient.GetAsync(address).ConfigureAwait(false);
                else
                {
                    if (query != null)
                        response = await HttpClient.GetAsync($"{address}?{query}").ConfigureAwait(false);
                    else
                        response = await HttpClient.GetAsync($"{address}/{value}").ConfigureAwait(false);
                }
            }
            catch (HttpRequestException ex)
            {
                return HttpResult<TResult>.Failure(ex.Message);
            }
            return await GetResponseResult<TResult>(response);
        }

        protected virtual async Task DeleteAsync(string address)
        {
            try
            {
                await HttpClient.DeleteAsync(address);
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is WebException)
                    throw ex.InnerException;
                else
                    throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected virtual async Task<HttpResult<TResult>> DeleteAsync<TResult>(string address)
        {
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.DeleteAsync(address);
            }
            catch (HttpRequestException ex)
            {
                return HttpResult<TResult>.Failure(ex.Message);
            }
            return await GetResponseResult<TResult>(response);
        }

        protected virtual async Task<HttpResult<TResult>> PutAsync<TResult>(string address, object model)
        {
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PutAsJsonAsync(address, model);
            }
            catch (HttpRequestException ex)
            {
                return HttpResult<TResult>.Failure(ex.Message);
            }

            return await GetResponseResult<TResult>(response);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && HttpClient != null)
            {
                HttpClient.Dispose();
                HttpClient = null;
            }
        }

        protected async Task<TResult> GetResponseData<TResult>(HttpResponseMessage response)
        {
            HttpResult<TResult> result = new HttpResult<TResult>() { StatusCode = response.StatusCode };
            if (response.IsSuccessStatusCode)
                result.Content = await response.Content.ReadAsAsync<TResult>();
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<TResult>();
            else
            {
                ErrorResponse error = await response.Content.ReadAsAsync<ErrorResponse>();
                throw error.GetException();
            }
        }

        protected async Task<HttpResult<TResult>> GetResponseResult<TResult>(HttpResponseMessage response)
        {
            HttpResult<TResult> result;
            if (response.IsSuccessStatusCode)
            {
                result = HttpResult<TResult>.Success(await response.Content.ReadAsAsync<TResult>());
            }
            else
            {
                try {
                    var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    result = error != null
                        ? HttpResult<TResult>.Failure(error.GetAllErrors())
                        : HttpResult<TResult>.Failure(response.ReasonPhrase);
                } catch {
                    result = HttpResult<TResult>.Failure(response.ReasonPhrase);
                }
            }
            result.StatusCode = response.StatusCode;
            return result;
        }

        protected BaseClient(bool debug)
        {
        }
    }
}