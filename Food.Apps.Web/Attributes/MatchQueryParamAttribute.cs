using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Food.Apps.Web.Attributes
{
    /// <summary>
    /// Ограничивает выполнение экшена только для случаев когда в query
    /// string присутствует указанный параметр и, опционально, если его значение
    /// равно указанному.
    /// </summary>
    public class MatchQueryParamAttribute : Attribute, IActionConstraint
    {
        private readonly string _name;
        private readonly string _expected;

        /// <summary>
        /// Конструирует ограничение.
        /// </summary>
        /// <param name="paramName">Название обязательного параметра</param>
        /// <param name="expectedValue">Значение которому должен равняться параметр.
        /// Если null - на равенство не проверяется.
        /// </param>
        public MatchQueryParamAttribute(string paramName, string expectedValue = null)
        {
            _name = paramName ?? throw new ArgumentNullException(nameof(paramName));
            _expected = expectedValue;
        }

        public bool Accept(ActionConstraintContext context)
        {
            var q = context.RouteContext.HttpContext.Request.Query;
            return q.TryGetValue(_name, out var value) && (_expected == null || value == _expected);
        }

        public int Order => 10;
    }
}
