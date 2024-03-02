using MessagePack;

namespace Ks.Serializer.Test
{
    public class NestedKeyTest
    {
        [Fact]
        public void Test01()
        {
            var serializer = new Ks.Serializer.MessagePack.Serializer();
            var request = new Request()
            {
                UniId = 111,
                Body = new Foo()
                {
                    X = 222,
                }
            };
            
            var bin = serializer.Serialize(request);
            var r2 = serializer.Deserialize<Request>(bin);
        
            Assert.NotNull(bin);
            Assert.Equal(request.UniId, r2.UniId);
            Assert.Equal(request.Body.X, r2.Body.X);
        }

        [MessagePackObject]
        public class Foo
        {
            [Key(0)]
            public int X { get; set; }
        }
        
        [MessagePackObject]
        public class Request
        {
            [Key(0)]
            public int UniId { get; set; }
            
            [Key(1)]
            public Foo Body { get; set; }
        }
    }
}