<Query Kind="Statements">
  <NuGetReference>Microsoft.Office.Interop.Word</NuGetReference>
  <Namespace>Microsoft.Office.Interop.Word</Namespace>
</Query>

string inputPath = @"D:\Code\zyst\Yfty\docs\2024.09.14-休闲场所\bjhtzy.doc";
string outputPath = @"D:\Code\zyst\Yfty\docs\2024.09.14-休闲场所\bjhtzy.docx";

var word = new Application();

var doc = word.Documents.Open(inputPath);
doc.SaveAs2(outputPath,WdSaveFormat.wdFormatXMLDocument);
doc.Close();
word.Quit();