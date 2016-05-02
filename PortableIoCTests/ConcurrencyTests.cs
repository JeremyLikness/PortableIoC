using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PortableIoC;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PortableIoCTests.TestHelpers;

namespace PortableIoCTests
{
    /// <summary>
    /// These tests check for high-concurrency situations and are obviously more valid when run on processors 
    /// with multiple CPUs 
    /// </summary>
    /// <remarks>
    /// Will run more slowly in the debugger but will allow you to see the results of various concurrent operations
    /// </remarks>
    [TestClass]
    public class ConcurrencyTests
    {
        private IPortableIoC _target;

        private Task ResolveSomething()
        {
            return Task.Run(() =>
            {
                IBar instance;
                var result = _target.TryResolve(out instance);                
            });
        }

        private Task ResolveSomethingNew()
        {
            return Task.Run(() =>
            {
                IBar instance;
                var result = _target.TryResolve(out instance, true);                
            });
        }

        private Task DestroySomething()
        {
            return Task.Run(() => _target.Destroy<IBar>());
        }

        private Task UnregisterSomething()
        {
            return Task.Run(() =>
            {
                var actual = _target.Unregister<IBar>();                
                try
                {
                    _target.Register<IBar>(ioc => new SimpleBar());                    
                }
                catch
                {
                    // already registered is fine to throw exception
                }
            });
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new PortableIoc();
        }

        [TestMethod]
        public async Task GivenThereAreMultipleThreadsRunningWhenDestroyAndResolveAreCalledThenTheResultShouldBeThreadSafeOperation()
        {
            _target.Register<IBar>(ioc => new SimpleBar());
            var tasks = new List<Task>();
            var random = new Random();
            for (var x = 0; x < 10000; x++)
            {
                var randomValue = random.Next(100);
                if (randomValue < 50)
                {
                    tasks.Add(ResolveSomething());
                }
                else if (randomValue < 75)
                {
                    tasks.Add(ResolveSomethingNew());
                }
                else if (randomValue < 95)
                {
                    tasks.Add(DestroySomething());
                }
                else
                {
                    tasks.Add(UnregisterSomething());
                }
            }
            await Task.WhenAll(tasks);
        }
    }
}