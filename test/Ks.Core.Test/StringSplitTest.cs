namespace Ks.Core.Test
{
	public class StringSplitTest
	{
		[Fact(DisplayName = "默认分割Split函数")]
		public void Test01()
		{
			var splits = "Aaaa Bbb   Cccc".Split(' ');
			Assert.Equal(5, splits.Length);
			Assert.Equal("Aaaa", splits[0]);
			Assert.Equal("Bbb", splits[1]);
			Assert.Equal("", splits[2]);
			Assert.Equal("", splits[3]);
			Assert.Equal("Cccc", splits[4]);
		}

		[Fact(DisplayName = "默认分割Split函数-移除空白结果")]
		public void Test02()
		{
			var splits = "Aaaa Bbb   Cccc".Split(' ', StringSplitOptions.RemoveEmptyEntries);
			Assert.Equal(3, splits.Length);
			Assert.Equal("Aaaa", splits[0]);
			Assert.Equal("Bbb", splits[1]);
			Assert.Equal("Cccc", splits[2]);
		}
	}
}