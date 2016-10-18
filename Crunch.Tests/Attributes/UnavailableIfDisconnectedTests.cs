using Crunch.Services;
using NUnit.Framework;
using Crunch.Attributes;
using Moq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Collections.Generic;

namespace Crunch.Tests.Attributes
{
    [TestFixture]
    public class UnavailableIfDisconnectedTests
    {

        UnavailableIfDisconnectedFilter _service;
        ActionExecutingContext _actionExecutingContext;
        private bool _dbConnected;

        [SetUp]
        public void SetUp()
        {
            var mockDbConnector = new Mock<IDBConnector>();
            _dbConnected = false;
            mockDbConnector.SetupGet(x => x.IsConnected).Returns(() => _dbConnected);
            SetupActionContext();

            _service = new UnavailableIfDisconnectedFilter(mockDbConnector.Object);
        }

        [Test]
        public void OnActionExecuting_NotConnected_Is503() {
            _service.OnActionExecuting(_actionExecutingContext);

            Assert.IsInstanceOf(typeof(StatusCodeResult), _actionExecutingContext.Result);
            
            var statusCodeResult = _actionExecutingContext.Result as StatusCodeResult; 
            Assert.AreEqual(503, statusCodeResult.StatusCode);
        }

        [Test]
        public void OnActionExecuting_IsConnected_NoResult() {
            _dbConnected = true;
            _service.OnActionExecuting(_actionExecutingContext);

            Assert.IsNull(_actionExecutingContext.Result);
        }

        private void SetupActionContext() {
            var context = new Mock<HttpContext>();

            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext {
                HttpContext = context.Object,
                RouteData = routeData,
                ActionDescriptor = actionDescriptor
            };
            var filters = new List<IFilterMetadata>();
            var arguments = new Dictionary<string, object>();
            _actionExecutingContext = new ActionExecutingContext(actionContext, filters, arguments, null);
        }
    }
}