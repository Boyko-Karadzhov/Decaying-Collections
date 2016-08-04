using System.Collections.Generic;

namespace Karadzhov.DecayingCollections.Tests
{
    public class SampleDecayingCollection : DecayingCollection<object, List<object>>
    {
        public SampleDecayingCollection(int lifespanInSeconds) : base(lifespanInSeconds)
        {
        }

        public SampleDecayingCollection(int lifespanInSeconds, int steps) : base(lifespanInSeconds, steps)
        {
        }

        public SampleDecayingCollection(ITimer timer, int lifespanInSeconds, int steps) : base(timer, lifespanInSeconds, steps)
        {
        }
    }
}
