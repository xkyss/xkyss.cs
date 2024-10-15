<Query Kind="Program">
  <NuGetReference>AutoMapper</NuGetReference>
  <NuGetReference>AutoMapper.Extensions.Microsoft.DependencyInjection</NuGetReference>
  <NuGetReference>Microsoft.Extensions.DependencyInjection</NuGetReference>
  <Namespace>Xunit</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>AutoMapper</Namespace>
</Query>

#load "xunit"

void Main()
{
	RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.
}


#region private::Tests
public class AutoMapperTest
{
	private readonly IServiceProvider sp;


	public AutoMapperTest()
	{
		var services = new ServiceCollection();
		services.AddAutoMapper(typeof(ModelsProfile));
		services.AddTransient<Target>();

		sp = services.BuildServiceProvider();
	}

	[Fact]
	public void Test01()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);

		var s = new Source()
		{
			Id = 111,
		};
		var t = mapper.Map(s, sp.GetRequiredService<Target>());

		Assert.Equal(s.Id, t.Id);
	}
}
#endregion

#region private::Models
class Source
{
	public int Id { get; set; }
}

class Target
{
	public int Id { get; set; }

	public string Name { get; set; }
}

#endregion

#region private::Profile
class ModelsProfile : Profile
{
	public ModelsProfile()
	{
		CreateMap<Source, Target>().ReverseMap();
	}
}
#endregion