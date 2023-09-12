namespace Ks.Exp.AutoMapper.Models;


public class FooModel
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public List<BarModel> BarModels { get; set; } = new List<BarModel>();
}
