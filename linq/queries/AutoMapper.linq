<Query Kind="Program">
  <NuGetReference>AutoMapper</NuGetReference>
  <NuGetReference>AutoMapper.Extensions.Microsoft.DependencyInjection</NuGetReference>
  <NuGetReference>Microsoft.Extensions.DependencyInjection</NuGetReference>
  <Namespace>AutoMapper</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Xunit</Namespace>
  <Namespace>System.ComponentModel</Namespace>
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
	
	private static readonly FooModel DefaultFoo = new FooModel()
	{
		Id = 1,
		Name = $"Foo-1",
	};

	private static readonly BarModel DefaultBar = new BarModel()
	{
		Id = 100,
		Name = $"Bar-100",
		FooList = new List<FooModel>() { DefaultFoo },
	};
	
	private static readonly FooChildModel DefaultFooChild = new FooChildModel()
	{
		Id = 1,
		Name = $"Foo-1",
		Value = $"Value-1",
	};

	public AutoMapperTest()
	{
		var services = new ServiceCollection();
		services.AddAutoMapper(typeof(ModelsProfile));
		
		sp = services.BuildServiceProvider();
	}
	
	[Fact]
	[DisplayName("关联类")]
	public void Test01()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);

		var foo = DefaultFoo;
		var fooVm = mapper.Map<FooViewModel>(foo);
		
		//foo.Dump();
		//fooVm.Dump();

		Assert.Equal(foo.Id, fooVm.Id);
		Assert.Equal(foo.Name, fooVm.Name);
		Assert.Null(fooVm.Desc);
	}

	[Fact]
	[DisplayName("关联类 + 数组")]
	public void Test02()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);
		
		var bar = DefaultBar;
		var barVm = mapper.Map<BarViewModel>(bar);

		//bar.Dump();
		//barVm.Dump();

		Assert.Equal(bar.Id, barVm.Id);
		Assert.Equal(bar.Name, barVm.Name);
		Assert.Null(barVm.Desc);
		
		Assert.NotEmpty(barVm.FooList);
		var foo = bar.FooList.First();
		var fooVm = barVm.FooList.First();
		//foo.Dump();
		//fooVm.Dump();

		Assert.Equal(foo.Id, fooVm.Id);
		Assert.Equal(foo.Name, fooVm.Name);
		Assert.Null(fooVm.Desc);
	}
	
	[Fact]
	[DisplayName("子类")]
	public void Test03()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);
		
		var foo = DefaultFoo;
		var fooChild = DefaultFooChild;

		var fooVm = mapper.Map<FooViewModel>(foo);
		var fooChildVm = mapper.Map<FooChildViewModel>(fooChild);

		Assert.NotNull(fooVm);
		Assert.Equal(foo.Id, fooVm.Id);
		
		Assert.NotNull(fooChildVm);
		Assert.Equal(fooChild.Value, fooChildVm.Value);
	}

	[Fact]
	[DisplayName("子类-列表")]
	public void Test04()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);

		var fooList = new List<FooModel>() { DefaultFoo, DefaultFooChild };
		
		var fooVmList = mapper.Map<List<FooModel>, List<FooViewModel>>(fooList);

		Assert.NotEmpty(fooVmList);
		Assert.True(fooList[0] is FooModel);
		Assert.True(fooVmList[0] is FooViewModel);
		
		Assert.True(fooList[1] is FooChildModel);
		Assert.True(fooVmList[1] is FooChildViewModel);
	}

	[Fact]
	[DisplayName("子类-数组")]
	public void Test05()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);

		var fooArray = new[] { DefaultFoo, DefaultFooChild };

		var fooVmArray = mapper.Map<FooViewModel[]>(fooArray);

		Assert.NotEmpty(fooVmArray);
		Assert.True(fooArray[0] is FooModel);
		Assert.True(fooVmArray[0] is FooViewModel);

		Assert.True(fooArray[1] is FooChildModel);
		Assert.True(fooVmArray[1] is FooChildViewModel);
	}

	[Fact]
	public void Test06()
	{
		var mapper = sp.GetRequiredService<IMapper>();
		Assert.NotNull(mapper);

		var sources = new[]
		{
			new ParentSource(),
			new ChildSource(),
			new ParentSource()
		};

		var destinations = mapper.Map<ParentSource[], ParentDestination[]>(sources);

		Assert.True(destinations[0] is ParentDestination);
		Assert.True(destinations[1] is ChildDestination);
		Assert.True(destinations[2] is ParentDestination);
	}

}
#endregion

public class ParentSource
{
	public int Value1 { get; set; }
}

public class ChildSource : ParentSource
{
	public int Value2 { get; set; }
}

public class ParentDestination
{
	public int Value1 { get; set; }
}

public class ChildDestination : ParentDestination
{
	public int Value2 { get; set; }
}


#region private::Models
public class FooModel
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public class FooViewModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Desc { get; set; }
}

public class BarModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public List<FooModel> FooList { get; set; }
}
public class BarViewModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Desc { get; set; }
	public List<FooViewModel> FooList { get; set; }
}

public class FooChildModel : FooModel
{
	public string Value { get; set; }
}
public class FooChildViewModel : FooViewModel
{
	public string Value { get; set; }
}
#endregion

#region private::Profile
class ModelsProfile : Profile
{
	public ModelsProfile()
	{
		CreateMap<FooModel, FooViewModel>().Include<FooChildModel, FooChildViewModel>().ReverseMap();
		CreateMap<BarModel, BarViewModel>().ReverseMap();
		CreateMap<FooChildModel, FooChildViewModel>().ReverseMap();

		CreateMap<ParentSource, ParentDestination>()
			.Include<ChildSource, ChildDestination>()
			.ReverseMap();
		CreateMap<ChildSource, ChildDestination>().ReverseMap();
	}
}
#endregion
