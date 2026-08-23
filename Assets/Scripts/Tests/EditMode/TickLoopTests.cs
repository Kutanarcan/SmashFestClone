using System;
using System.Collections.Generic;
using Game.Core;
using Game.Tests.EditMode.Fakes;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class TickLoopTests
    {
        [Test]
        public void TicksEveryTickableOncePerTick()
        {
            var first = new FakeTickable();
            var second = new FakeTickable();
            var loop = new TickLoop(new ITickable[] { first, second });

            loop.Tick(0.016f);

            Assert.AreEqual(1, first.TickCount);
            Assert.AreEqual(1, second.TickCount);
        }

        [Test]
        public void PassesDeltaTimeThrough()
        {
            var tickable = new FakeTickable();
            var loop = new TickLoop(new ITickable[] { tickable });

            loop.Tick(0.25f);

            Assert.AreEqual(0.25f, tickable.LastDeltaTime, 0.0001f);
        }

        [Test]
        public void PreservesRegistrationOrder()
        {
            var log = new List<string>();
            var loop = new TickLoop(new ITickable[]
            {
                new FakeTickable("a", log),
                new FakeTickable("b", log),
                new FakeTickable("c", log)
            });

            loop.Tick(0.016f);

            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, log);
        }

        [Test]
        public void RepeatedTicks_Accumulate()
        {
            var tickable = new FakeTickable();
            var loop = new TickLoop(new ITickable[] { tickable });

            loop.Tick(0.016f);
            loop.Tick(0.016f);
            loop.Tick(0.016f);

            Assert.AreEqual(3, tickable.TickCount);
        }

        [Test]
        public void EmptyLoop_IsSafeToTick()
        {
            var loop = new TickLoop(Array.Empty<ITickable>());

            Assert.AreEqual(0, loop.Count);
            Assert.DoesNotThrow(() => loop.Tick(0.016f));
        }

        [Test]
        public void NullArray_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TickLoop(null));
        }

        [Test]
        public void NullElement_ThrowsAtConstruction()
        {
            Assert.Throws<ArgumentException>(
                () => new TickLoop(new ITickable[] { new FakeTickable(), null }));
        }
    }
}
