<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XSSF.UserModel</Namespace>
  <Namespace>NPOI.SS.UserModel</Namespace>
  <Namespace>NPOI.HSSF.Util</Namespace>
</Query>

void Main()
{
	var filePath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.09.13-休闲场所\休闲\休闲场所总表.xlsx";
	var fixedPath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.09.13-休闲场所\休闲\休闲场所总表-fixed.xlsx";
	using var ifs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

	// 创建工作簿 (.xlsx)
	using var workbook = new XSSFWorkbook(ifs);
	// 获取工作表
	var sheet = workbook.GetSheetAt(0);

	var cellStyle = workbook.CreateCellStyle();
	SetCellStyle(cellStyle);

	// 在第一行添加列名 "id"
	var headerRow = sheet.GetRow(0) ?? sheet.CreateRow(0);
	// 在第一列添加单元格
	var idCell = headerRow.CreateCell(8);
	idCell.SetCellValue("id");
	idCell.CellStyle = cellStyle;

	// 遍历所有行，在每行的第一列添加 "id" 值
	for (int i = 1; i <= sheet.LastRowNum; i++)
	{
		var row = sheet.GetRow(i) ?? sheet.CreateRow(i);
		var cell = row.CreateCell(8);
		cell.SetCellValue(Guid.NewGuid().ToString("N"));
		cell.CellStyle = cellStyle;
	}

	// 写入文件
	using var ofs = new FileStream(fixedPath, FileMode.Create, FileAccess.Write);
	workbook.Write(ofs);
}

static void SetCellStyle(ICellStyle cellStyle)
{
	// 设置边框
	cellStyle.BorderTop = BorderStyle.Thin;
	cellStyle.BorderBottom = BorderStyle.Thin;
	cellStyle.BorderLeft = BorderStyle.Thin;
	cellStyle.BorderRight = BorderStyle.Thin;

	// 设置边框颜色
	cellStyle.TopBorderColor = HSSFColor.Black.Index;
	cellStyle.BottomBorderColor = HSSFColor.Black.Index;
	cellStyle.LeftBorderColor = HSSFColor.Black.Index;
	cellStyle.RightBorderColor = HSSFColor.Black.Index;
}
