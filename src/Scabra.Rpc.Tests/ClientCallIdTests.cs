using NUnit.Framework;
using Scabra.Rpc.Client;

namespace Scabra.Rpc
{
    [TestFixture]
    public class ClientCallIdTests
    {
        [Test]
        public void should_be_created()
        {
            var sut = new ClientCallId(index: 25, sequenceNumber: 48);

            Assert.That(sut.Index, Is.EqualTo(25));
            Assert.That(sut.SequenceNumber, Is.EqualTo(48));
        }

        [Test]
        public void should_be_serdesed()
        {
            var sut_1 = new ClientCallId(index: 25, sequenceNumber: 48);

            var (indexBytes, sequenceNumberBytes) = sut_1.ToBytes();
            var sut_2 = new ClientCallId(indexBytes, sequenceNumberBytes);

            Assert.That(sut_1.Index, Is.EqualTo(sut_2.Index));
            Assert.That(sut_1.SequenceNumber, Is.EqualTo(sut_2.SequenceNumber));
        }

        [Test]
        public void should_be_equatable()
        {
            var sut_1 = new ClientCallId(index: 25, sequenceNumber: 48);
            var sut_2 = new ClientCallId(index: 25, sequenceNumber: 48);
            var sut_3 = new ClientCallId(index: 26, sequenceNumber: 48);
            var sut_4 = new ClientCallId(index: 25, sequenceNumber: 49);

            Assert.That(sut_1 == sut_2, Is.True);
            Assert.That(sut_1 != sut_2, Is.False);
            Assert.That(sut_1.Equals(sut_2), Is.True);

            Assert.That(sut_1 == sut_3, Is.False);
            Assert.That(sut_1 != sut_3, Is.True);
            Assert.That(sut_1.Equals(sut_3), Is.False);

            Assert.That(sut_1 == sut_4, Is.False);
            Assert.That(sut_1 != sut_4, Is.True);
            Assert.That(sut_1.Equals(sut_4), Is.False);

            Assert.That(sut_3 == sut_4, Is.False);
            Assert.That(sut_3 != sut_4, Is.True);
            Assert.That(sut_1.Equals(sut_4), Is.False);
        }

        [Test]
        public void should_to_string()
        {
            var s = new ClientCallId(index: 25, sequenceNumber: 48).ToString();

            Assert.That(s, Is.Not.Null.And.Not.Empty);
        }
    }
}
