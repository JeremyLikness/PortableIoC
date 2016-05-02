using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PortableIoC;
using PortableIoCTests.TestHelpers;

namespace PortableIoCTests
{
    [TestClass]
    public class IoCTests
    {
        private IPortableIoC _target;
        private const string TestLabel = "TestLabel";

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new PortableIoc();
        }

        /// <summary>
        /// When I request an instance of <see cref="IPortableIoC"/> then I should
        /// receive the instance I am resolving from
        /// </summary>
        [TestMethod]
        public void GivenNewInstanceWhenInterfaceRequestedThenShouldReturnInstance()
        {
            var actual = _target.Resolve<IPortableIoC>();
            Assert.AreSame(_target, actual, "Test failed: container did not resolve to self.");
        }

        /// <summary>
        /// When I request an instance of <see cref="IPortableIoC"/> from a label then
        /// I should receive the instance I am resolving from
        /// </summary>
        [TestMethod]
        public void GivenNewInstanceWhenInterfaceRequestedWithLabelThenShouldReturnInstance()
        {
            var actual = _target.Resolve<IPortableIoC>(TestLabel);
            Assert.AreSame(_target, actual, "Test failed: container did not resolve to self.");
        }

        [TestMethod]
        public void GivenTypeNotRegisteredWhenRequestedThenShouldThrowInvalidOperationException()
        {
            var expected = false;
            try {
                _target.Resolve<IBar>();
            }
            catch(InvalidOperationException)
            {
                expected = true;
            }
            Assert.IsTrue(expected, "Test failed: container did not throw exception for unregistered type.");
        }

        [TestMethod]
        public void GivenTypeNotRegisteredWhenRegisteredWithNullThenShouldThrowArgumentNullException()
        {
            var expected = false;
            try
            {
                _target.Register<IBar>(null);
            }
            catch(ArgumentNullException)
            {
                expected = true;
            }
            Assert.IsTrue(expected, "Test failed: container did not throw argument null exception for empty registration.");
        }

        [TestMethod]
        public void GivenTypeRegisteredWhenRegisteredSecondTimeThenShouldThrowInvalidOperationException()
        {
            var expected = false;
            try
            {
                _target.Register<IBar>(ioc => new SimpleBar());
                _target.Register<IBar>(ioc => new SimpleBar());
            }
            catch(InvalidOperationException)
            {
                expected = true;
            }
            Assert.IsTrue(expected, "Test failed: second registration of same type should through InvalidOperationException.");
        }

        [TestMethod]
        public void GivenTypeRegisteredWhenRequestedThenShouldReturnInstance()
        {
            var expected = new SimpleBar();
            _target.Register<IBar>(ioc => expected);
            var actual = _target.Resolve<IBar>();
            Assert.AreSame(expected, actual, "Test failed: same instance was not returned.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenMultipleCallsMadeThenShouldReturnSharedInstanceByDefault()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual1 = _target.Resolve<IBar>();
            var actual2 = _target.Resolve<IBar>();
            Assert.AreSame(actual1, actual2, "Test failed: multiple calls should return the same shared instance.");
        }

        [TestMethod]
        public void GivenTypeNotRegisteredWhenCanResolveCalledThenShouldReturnFalse()
        {
            Assert.IsFalse(_target.CanResolve<IBar>(),
                           "Test failed: can resolve should return false when type not registered.");
        }

        [TestMethod]
        public void GivenTypeRegisteredWhenCanResolveCalledThenShouldReturnTrue()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            Assert.IsTrue(_target.CanResolve<IBar>(),
                            "Test failed: can resolve should return true when the type is registered.");
        }

        [TestMethod]
        public void GivenTypeIsNotRegisteredWhenTryResolveCalledThenShouldReturnFalse()
        {
            IBar barInstance;
            var result = _target.TryResolve(out barInstance);
            Assert.IsNull(barInstance, "Test failed: bar instance should be null when type is not registered");
            Assert.IsFalse(result, "Test failed: result should be false when type is not registered.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenTryResolveCalledForDifferentLabelThenShouldReturnFalse()
        {
            IBar barInstance;
            _target.Register<IBar>(ioc => new SimpleBar());
            var result = _target.TryResolve(out barInstance, TestLabel);
            Assert.IsNull(barInstance, "Test failed: bar instance should be null when type is not registered to label.");
            Assert.IsFalse(result, "Test failed: result should be false when type is not registered to label.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenTryResolveCalledForDifferentLabelWithCreateNewThenShouldReturnFalse()
        {
            IBar barInstance;
            _target.Register<IBar>(ioc => new SimpleBar());
            var result = _target.TryResolve(out barInstance, true, TestLabel);
            Assert.IsNull(barInstance, "Test failed: bar instance should be null when type is not registered to label.");
            Assert.IsFalse(result, "Test failed: result should be false when type is not registered to label.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenTryResolveCalledThenShouldReturnTrueAndTheConfiguredInstance()
        {
            IBar barInstance;
            _target.Register<IBar>(ioc => new SimpleBar());
            var result = _target.TryResolve(out barInstance);
            Assert.IsInstanceOfType(barInstance, typeof(SimpleBar), "Test failed: returned instance is not instance of SimpleBar.");
            Assert.IsTrue(result, "Test failed: result should be true when the type is registered.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenTryResolveCatchesExceptionThenShouldReturnFalseAndNull()
        {
            IBar barInstance;
            _target.Register<IBar>(ioc => new SimpleBar(new InvalidOperationException()));
            var result = _target.TryResolve(out barInstance);
            Assert.IsNull(barInstance, "Test failed: bar instance should be null when exception thrown during resolution.");
            Assert.IsFalse(result, "Test failed: result should be false when exception thrown during resolution.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredNotToALabelWhenResolveCalledForLabelThenShouldThrowInvalidOperationException()
        {
            var expected = false;
            try
            {
                _target.Register<IBar>(ioc => new SimpleBar());
                _target.Resolve<IBar>(TestLabel);
            }
            catch(InvalidOperationException)
            {
                expected = true;
            }
            Assert.IsTrue(expected, "Test failed: should have raised invalid operation exception when not registered to the label.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredToALabelWhenResolveCalledForLabelThenShouldReturnConfiguredInstance()
        {
            _target.Register<IBar>(ioc => new SimpleBar(), TestLabel);
            Assert.IsTrue(_target.CanResolve<IBar>(TestLabel),
                          "Test failed: can resolve should return true when the type is registered.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredToDefaultAndLabelWhenResolveCalledForEachThenInstancesShouldBeDifferent()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            _target.Register<IBar>(ioc => new SimpleBar(), TestLabel);
            var actual1 = _target.Resolve<IBar>();
            var actual2 = _target.Resolve<IBar>(TestLabel);
            Assert.AreNotSame(actual1, actual2, "Test failed: different labels should not return the same shared instance.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenNewInstanceIsRequestedThenShouldReturnNewInstance()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual1 = _target.Resolve<IBar>();
            var actual2 = _target.Resolve<IBar>(true);
            Assert.AreNotSame(actual1, actual2, "Test failed: create new should not return the same shared instance.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredThenUnregisteredWhenCanResolveCalledThenShouldReturnFalse()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            _target.Unregister<IBar>();
            Assert.IsFalse(_target.CanResolve<IBar>(), "Test failed: can resolve should return false after type is unregistered.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredThenUnregisteredThenRegisteredWhenResolveCalledThenShouldReturnADifferentInstance()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual1 = _target.Resolve<IBar>();
            _target.Unregister<IBar>();
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual2 = _target.Resolve<IBar>();
            Assert.AreNotSame(actual1, actual2, "Test failed: when unregistered and re-registered a call to resolve should return a different instance.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredThenUnregisteredThenRegisteredWithLabelWhenResolveCalledThenShouldReturnADifferentInstance()
        {
            _target.Register<IBar>(ioc => new SimpleBar(), TestLabel);
            var actual1 = _target.Resolve<IBar>(TestLabel);
            _target.Unregister<IBar>(TestLabel);
            _target.Register<IBar>(ioc => new SimpleBar(), TestLabel);
            var actual2 = _target.Resolve<IBar>(TestLabel);
            Assert.AreNotSame(actual1, actual2, "Test failed: when unregistered and re-registered a call to resolve should return a different instance.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredThenResolvedThenDestroyedWhenResolveCalledThenShouldReturnADifferentInstance()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual1 = _target.Resolve<IBar>();
            _target.Destroy<IBar>();
            var actual2 = _target.Resolve<IBar>();
            Assert.AreNotSame(actual1, actual2, "Test failed: when destroy is called, a subsequent call to resolve should return a different instance.");
        }

        [TestMethod]
        public void GivenTypeIsNotRegisteredWhenUnregisterCalledThenShouldReturnFalse()
        {
            var actual = _target.Unregister<IBar>();
            Assert.IsFalse(actual, "Test failed: call to unregister non-registered type should return false.");
        }

        [TestMethod]
        public void GivenTypeIsNotRegisteredWhenDestroyCalledThenShouldReturnFalse()
        {
            var actual = _target.Destroy<IBar>();
            Assert.IsFalse(actual, "Test failed: call to destroy non-registered type should return false.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredAndResolvedWhenDestroyCalledForDifferentLabelThenShouldReturnFalse()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            _target.Resolve<IBar>();
            var actual = _target.Destroy<IBar>(TestLabel);
            Assert.IsFalse(actual, "Test failed: destroy should return false for label that was not registered nor an instance created.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenUnregisterCalledTwiceThenFirstCallShouldReturnTrueSecondCallShouldReturnFalse()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual = _target.Unregister<IBar>();
            Assert.IsTrue(actual, "Test failed: first call to unregister a registered type should return true.");
            actual = _target.Unregister<IBar>();
            Assert.IsFalse(actual, "Test failed: second call to unregister a previously unregistered type should return false.");
        }

        [TestMethod]
        public void GivenTypeIsRegisteredWhenDestroyCalledTwiceThenFirstCallShouldReturnTrueSecondCallShouldReturnFalse()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var actual = _target.Destroy<IBar>();
            Assert.IsFalse(actual, "Test failed: first call to destroy a registered type not yet resolved should return false.");
            _target.Resolve<IBar>();
            actual = _target.Destroy<IBar>();
            Assert.IsTrue(actual, "Test failed: first call to destroy a registered type after resolution should return true.");
            actual = _target.Destroy<IBar>();
            Assert.IsFalse(actual, "Test failed: second call to destroy a type without intermediate resolution should return false.");
        }

        [TestMethod]
        public void GivenConstructorDependencyWhenInstanceRequestedThenShouldReturnInstanceWithDependencyResolved()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            _target.Register<IFoo>(ioc => new SimpleFoo(ioc.Resolve<IBar>()));
            var actual = _target.Resolve<IFoo>();
            Assert.IsNotNull(actual, "Test failed: actual should not be null.");
            Assert.IsNotNull(actual.Bar, "Test failed; actual property for Bar should not be null.");
        }

        [TestMethod]
        public void GivenPropertyInjectionWhenInstanceRequestedThenShouldReturnInstanceWithDependencyResolved()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            _target.Register<IFoo>(ioc => new SimpleFoo { Bar = ioc.Resolve<IBar>() });
            var actual = _target.Resolve<IFoo>();
            Assert.IsNotNull(actual, "Test failed: actual should not be null.");
            Assert.IsNotNull(actual.Bar, "Test failed; actual property for Bar should not be null.");
        }
    }
}
