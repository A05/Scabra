using NUnit.Framework;

namespace Scabra.Rpc
{
    [TestFixture]
    public class ArgsIndicesMarshallerTests
    {
        [Test]
        public void should_marshal_args_indices()
        {
            var sut = new Marshaller.ArgsIndicesMarshaller();

            var indices = sut.Marshal(new object[] 
            { 
                new(), null, new(), null, 
                new(), new(), null, null,
                null, null, new(), null, 
                null, new(), new(), new()
            });

            Assert.That(indices, Is.EqualTo(0b1110_0100_0011_0101));

            indices = sut.Marshal(new object[]
            {
                null, null, null, null,
                null, null, null, null,
                null, null, null, null,
                null, null, null, null
            });

            Assert.That(indices, Is.EqualTo(0));

            var exceededArgs = new object[]
            {
                new(), new(), new(), new(),
                new(), new(), new(), new(),
                new(), new(), new(), new(),
                new(), new(), new(), new(),
                null, null, null, null
            };

            Assert.That(exceededArgs.Length > Marshaller.MaxArgsLength);

            indices = sut.Marshal(exceededArgs);

            Assert.That(indices, Is.EqualTo(0b1111_1111_1111_1111));

            indices = sut.Marshal(new object[] { null, new(), null, new() });
            Assert.That(indices, Is.EqualTo(0b1010));

            indices = sut.Marshal(new object[] {});
            Assert.That(indices, Is.EqualTo(0));

            indices = sut.Marshal(null);
            Assert.That(indices, Is.EqualTo(0));
        }
    }
}
