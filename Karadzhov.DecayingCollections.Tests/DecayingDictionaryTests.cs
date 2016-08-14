using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karadzhov.DecayingCollections.Tests
{
    [TestClass]
    public class DecayingDictionaryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IndexerGet_NullKey_Throws()
        {
            using (var dict = new DecayingDictionary<object, object>(1, 5))
            {
                var test = dict[null];
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IndexerSet_NullKey_Throws()
        {
            using (var dict = new DecayingDictionary<object, object>(1, 5))
            {
                dict[null] = new object();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IndexerGet_NonExistentKey_Throws()
        {
            using (var dict = new DecayingDictionary<int, object>(1, 5))
            {
                var test = dict[1];
            }
        }

        [TestMethod]
        public void IndexerGet_ExistentKey_RetrievesValue()
        {
            var timer = new FakeTimer();
            var item = new object();
            using (var dict = new DecayingDictionary<int, object>(timer, 1, 5))
            {
                dict.Add(1, item);
                timer.Execute();
                var retreived = dict[1];

                Assert.AreSame(item, retreived);
            }
        }

        [TestMethod]
        public void IndexerSet_NonExistentKey_Added()
        {
            var timer = new FakeTimer();
            var item = new object();
            using (var dict = new DecayingDictionary<int, object>(timer, 1, 5))
            {
                dict[1] = item;
                timer.Execute();

                Assert.AreEqual(1, dict.Count);
            }
        }

        [TestMethod]
        public void IndexerSet_NonExistentKey_Overridden()
        {
            var timer = new FakeTimer();
            var item1 = new object();
            var item2 = new object();
            using (var dict = new DecayingDictionary<int, object>(timer, 1, 3))
            {
                dict[1] = item1;
                timer.Execute();
                timer.Execute();
                dict[1] = item2;
                Assert.AreEqual(1, dict.Count);

                timer.Execute();
                timer.Execute();

                Assert.AreEqual(1, dict.Count);
                Assert.AreSame(item2, dict[1]);
            }
        }

        [TestMethod]
        public void Keys_TwoEntries_GetsTheKeys()
        {
            var timer = new FakeTimer();
            var item1 = new object();
            var item2 = new object();
            using (var dict = new DecayingDictionary<int, object>(timer, 1, 3))
            {
                dict[1] = item1;
                dict[2] = item2;

                var keys = dict.Keys;

                Assert.IsNotNull(keys);
                Assert.AreEqual(2, keys.Count);
                CollectionAssert.Contains((ICollection)keys, 1);
                CollectionAssert.Contains((ICollection)keys, 2);
            }
        }

        [TestMethod]
        public void Values_ThreeEntriesWithDuplicates_GetsTheValues()
        {
            var timer = new FakeTimer();
            var item1 = "item 1";
            var item2 = "item 2";
            using (var dict = new DecayingDictionary<int, string>(timer, 1, 3))
            {
                dict[1] = item1;
                dict[2] = item2;
                dict[3] = item1;

                var values = dict.Values;

                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                CollectionAssert.Contains((ICollection)values, item1);
                Assert.AreEqual(2, values.Where(v => v == item1).Count());
            }
        }
    }
}
