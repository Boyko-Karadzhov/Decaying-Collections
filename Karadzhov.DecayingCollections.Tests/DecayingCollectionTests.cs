using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
