namespace Ks.Exp.AutoMapper.Models;

public class FooViewModel
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public List<FooModel> FooModels { get; set; } = new List<FooModel>();
}
