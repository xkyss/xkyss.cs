using MessagePack;

namespace Ks.Serializer.Test;

public class MessageTest
{
    [Fact]
    public void Test01()
    {
        var serializer = new Ks.Serializer.MessagePack.Serializer();
        var message = new Message()
        {
            UniId = 12345,
        };
        
        var bin = serializer.Serialize(message);
        var message2 = serializer.Deserialize<Message>(bin);
        
        Assert.NotNull(bin);
        Assert.Equal(message.UniId, message2.UniId);
    }
    
    [MessagePackObject(true)]
    public class Message
    {
        /// <summary>
        /// 消息唯一id
        /// </summary>
        public int UniId { get; set; }
        [IgnoreMember]
        public virtual int MsgId { get; }
    }

}