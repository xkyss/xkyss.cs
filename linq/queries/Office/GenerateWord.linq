<Query Kind="Program">
  <NuGetReference>NPOI</NuGetReference>
  <Namespace>NPOI.XWPF.UserModel</Namespace>
</Query>

void Main()
{
	// 创建一个新的 Word 文档对象
	var document = new XWPFDocument();

	// 创建一个段落
	var paragraph = document.CreateParagraph();
	paragraph.Alignment = ParagraphAlignment.CENTER; // 设置段落居中

	// 在段落中创建一个运行对象（即文本块）
	var run = paragraph.CreateRun();
	run.SetText("Hello World"); // 设置运行对象的文本
	run.FontFamily = "Arial"; // 设置字体
	run.FontSize = 20; // 设置字体大小
	run.IsBold = true; // 设置粗体

	// 创建另一个段落
	var paragraph2 = document.CreateParagraph();
	var run2 = paragraph2.CreateRun();
	run2.SetText("这是第二段文本。");

	// 创建一个表格
	var table = document.CreateTable(2, 2); // 创建一个 2x2 的表格
	table.GetRow(0).GetCell(0).SetText("表头1");
	table.GetRow(0).GetCell(1).SetText("表头2");
	table.GetRow(1).GetCell(0).SetText("单元格1");
	table.GetRow(1).GetCell(1).SetText("单元格2");

	// 将 Word 文档写入到文件
	var filePath = "CreatedWordDocument.docx";
	using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

	document.Write(fileStream);

	Console.WriteLine("Word document created successfully.");
}

