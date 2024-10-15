namespace Ks.Exp.AutoMapper.Models;

public class BarViewModel
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public override string ToString()
	{
		return $"""
		Id: {Id},
		Name: {Name},
		Description: {Description},
		""";
	}
}
