<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XWPF.UserModel</Namespace>
</Query>

void Main()
{
	var docPath = @"D:\Code\zyst\Yfty\docs\2024.09.14-休闲场所\bjhtzy.docx";
	using var ifs = new FileStream(docPath, FileMode.Open, FileAccess.Read);

	var doc = new XWPFDocument(ifs);
	var tables = doc.Tables;

	foreach (XWPFTable table in tables)
	{
		foreach (XWPFTableRow row in table.Rows)
		{
			foreach (XWPFTableCell cell in row.GetTableCells())
			{
				var cellText = cell.GetText();
				
				Console.WriteLine(cellText);
			}
		}
	}
}

