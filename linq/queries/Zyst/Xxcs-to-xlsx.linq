<Query Kind="Statements">
  <NuGetReference>Microsoft.Office.Interop.Excel</NuGetReference>
  <Namespace>Microsoft.Office.Interop.Excel</Namespace>
</Query>

var inputPath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.09.23-休闲场所\xxzb0924.xls";
var outputPath = @"E:\xk\Code\zyaj\jwt_v3\ydjwv3\trunk\working\gits\Yfty\docs\2024.09.23-休闲场所\xxzb0924.xlsx";

var excel = new Application();

var wb = excel.Workbooks.Open(inputPath);

wb.SaveAs(outputPath, XlFileFormat.xlExcel8);
wb.Close();
excel.Quit();

// TODO: 转换的文件用脚本读不出来