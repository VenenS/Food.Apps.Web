using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

namespace ITWebNet.Food.Site.Services
{
    public class ModelValidationException : Exception
    {
        public ModelValidationException() { }
        public ModelValidationException(string message) : base(message) { }
        public ModelValidationException(string message, Exception inner) : base(message, inner) { }
        public ModelValidationException(string message, string messageDetail, Dictionary<string, string[]> modelState)
            : base(message)
        {
            MessageDetail = MessageDetail;
            if (modelState == null)
                ModelState = new Dictionary<string, string[]>();
            else
                ModelState = CleanUpModelState(modelState);
        }

        public string MessageDetail { get; set; }
        public Dictionary<string, string[]> ModelState { get; set; }

        protected ModelValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }

        private Dictionary<string, string[]> CleanUpModelState(Dictionary<string, string[]> modelState)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();

            foreach (var item in modelState)
            {
                var newKey = item.Key.Replace("model.", string.Empty);
                result.Add(newKey, item.Value);
            }

            return result;
        }

    }

    public class ErrorResponse
    {
        public string Message { get; set; }

        public string MessageDetail { get; set; }

        public Dictionary<string, string[]> ModelState { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionType { get; set; }

        public string StackTrace { get; set; }

        public ErrorResponse InnerException { get; set; }

        public bool IsException { get { return !string.IsNullOrWhiteSpace(ExceptionMessage); } }

        private static Dictionary<string, string[]> CleanUpModelState(Dictionary<string, string[]> modelState)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>();

            if (modelState == null)
                return result;

            foreach (var item in modelState)
            {
                var newKey = item.Key.Replace("model.", string.Empty);
                result.Add(newKey, item.Value);
            }

            return result;
        }

        public IList<string> GetAllErrors()
        {
            List<string> errorsList = new List<string>();

            if (IsException)
            {
                errorsList.Add(ExceptionMessage);
                if (InnerException != null)
                    errorsList.AddRange(InnerException.GetAllErrors());
            }
            else
            {
                var modelState = CleanUpModelState(ModelState);

                foreach (string[] modelErrors in modelState.Values)
                {
                    errorsList.AddRange(modelErrors);
                }
                if (Message != null)
                {
                    errorsList.Add(Message);
                }
            }

            return errorsList;
        }

        public Exception GetException()
        {
            if (IsException)
            {
                Type exType = Type.GetType(ExceptionType);

                Exception exception;
                if (exType == null)
                    exception = new Exception(ExceptionMessage, InnerException.GetException());
                else if (InnerException != null)
                    exception = (Exception)Activator.CreateInstance(
                    exType,
                    new object[]
                    {
                        ExceptionMessage,
                        InnerException.GetException()
                    });
                else
                    exception = (Exception)Activator.CreateInstance(
                    exType,
                    new object[]
                    {
                        ExceptionMessage
                    });

                return exception;
            }
            return null;
        }
    }
}
