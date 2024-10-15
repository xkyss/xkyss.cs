using AutoMapper;
using Ks.Exp.AutoMapper.Models;

namespace Ks.Exp.AutoMapper;

internal class AutoMapperProfile : Profile
{
	public AutoMapperProfile() 
	{
		CreateMap<FooViewModel, FooModel>().ReverseMap();
		CreateMap<BarViewModel, BarModel>().ReverseMap();
	}
}
