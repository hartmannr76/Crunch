using System.Reflection;
using Crunch.Services;
using NUnit.Framework;

namespace Crunch.Tests.Services
{
    [TestFixture]
    public class NavigationDataProviderTests
    {
        const string Prefix = "NUnit.Runner.Test.Navigation.NavigationTestData";

        IVariantPicker _service;

        [SetUp]
        public void SetUp()
        {
            _service = new VariantPicker();
        }

        [Test]
        public void SelectVariant_() {
            //var newMock = new Mock<IVariantPicker>();
        }
    }
}