using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Karadzhov.DecayingCollections.Tests
{
    [TestClass]
    public class DecayingCollectionTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullTimer_Throws()
        {
            using (new SampleDecayingCollection(null, 0, 0)) { }
        }

        [TestMethod]
        public void Add_3Items_CountIs3()
        {
            using (var collection = new SampleDecayingCollection(new FakeTimer(), 1000, 1))
            {
                collection.Add(new object());
                collection.Add(new object());
                collection.Add(new object());

                Assert.AreEqual(3, collection.Count);
            }
        }

        [TestMethod]
        public void Add_3ItemsDifferentSteps_Count3AllContained()
        {
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var timer = new FakeTimer();

            using (var collection = new SampleDecayingCollection(timer, 0, 5))
            {
                collection.Add(item1);
                timer.Execute();
                collection.Add(item2);
                timer.Execute();
                collection.Add(item3);
                timer.Execute();

                Assert.AreEqual(3, collection.Count);
                Assert.IsTrue(collection.Contains(item1));
                Assert.IsTrue(collection.Contains(item2));
                Assert.IsTrue(collection.Contains(item3));
            }
        }

        [TestMethod]
        public void Add_3ItemsAfterDecay_Count2CheckContains()
        {
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var timer = new FakeTimer();
            object eventArgsItem = null;

            using (var collection = new SampleDecayingCollection(timer, 0, 5))
            {
                collection.ItemDecayed += (s, e) => eventArgsItem = e.Item;
                collection.Add(item1);
                timer.Execute();
                collection.Add(item2);
                timer.Execute();
                collection.Add(item3);
                timer.Execute();
                timer.Execute();
                timer.Execute();

                Assert.AreEqual(2, collection.Count);
                Assert.IsFalse(collection.Contains(item1));
                Assert.IsTrue(collection.Contains(item2));
                Assert.IsTrue(collection.Contains(item3));

                Assert.IsNotNull(eventArgsItem);
                Assert.AreSame(item1, eventArgsItem);
            }
        }

        [TestMethod]
        public void Clear_3Items_Count0CheckContains()
        {
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var timer = new FakeTimer();

            using (var collection = new SampleDecayingCollection(timer, 0, 5))
            {
                collection.Add(item1);
                timer.Execute();
                collection.Add(item2);
                timer.Execute();
                collection.Add(item3);
                timer.Execute();

                collection.Clear();

                Assert.AreEqual(0, collection.Count);
                Assert.IsFalse(collection.Contains(item1));
                Assert.IsFalse(collection.Contains(item2));
                Assert.IsFalse(collection.Contains(item3));
            }
        }

        [TestMethod]
        public void ForEach_3Items_AllReturned()
        {
            var items = new List<object>(3) { 1, 2, 3 };
            var timer = new FakeTimer();

            using (var collection = new SampleDecayingCollection(timer, 0, 5))
            {
                for (var i = 0; i < items.Count; i++)
                {
                    collection.Add(items[i]);
                    timer.Execute();
                }

                foreach (var item in collection)
                    Assert.IsTrue(items.Remove(item));

                Assert.AreEqual(0, items.Count);
            }
        }

        [TestMethod]
        public void Remove_MiddleItem_Count2CheckContains()
        {
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var timer = new FakeTimer();

            using (var collection = new SampleDecayingCollection(timer, 0, 5))
            {
                collection.Add(item1);
                timer.Execute();
                collection.Add(item2);
                timer.Execute();
                collection.Add(item3);
                timer.Execute();

                Assert.IsTrue(collection.Remove(item2));
                Assert.IsFalse(collection.Remove(item2));

                Assert.AreEqual(2, collection.Count);
                Assert.IsTrue(collection.Contains(item1));
                Assert.IsFalse(collection.Contains(item2));
                Assert.IsTrue(collection.Contains(item3));
            }
        }
    }
}
