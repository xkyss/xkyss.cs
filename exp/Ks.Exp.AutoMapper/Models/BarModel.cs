namespace Ks.Exp.AutoMapper.Models;

public class BarModel
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public override string ToString()
	{
		return $@"""
		Id: {Id},
		Name: {Name},
		""";
	}
}
