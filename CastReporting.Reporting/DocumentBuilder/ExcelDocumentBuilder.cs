﻿/*
 *   Copyright (c) 2016 CAST
 *
 * Licensed under a custom license, Version 1.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License, accessible in the main project
 * source code: Empowerment.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using CastReporting.Reporting.Builder.BlockProcessing;
using CastReporting.Reporting.ReportingModel;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using CastReporting.BLL;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Web;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using System.Text;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.Globalization;
using System.Windows;
using CastReporting.Reporting.Languages;
using CastReporting.BLL.Computing;
using CastReporting.Reporting.Atrributes;
using System.Text.RegularExpressions;
using CastReporting.Domain;
using CastReporting.Reporting.Helper;


namespace CastReporting.Reporting.Builder
{
    internal class ExcelDocumentBuilder : DocumentBuilderBase
    {
        string strFinalTempFile = "";
        ReportData reportData;

        #region CONSTRUCTORS
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public ExcelDocumentBuilder(ReportData client, string tmpRepFlexi)
            : base(client)
        { 
            strFinalTempFile = tmpRepFlexi;
            reportData = client;
        }
        #endregion CONSTRUCTORS

        #region METHODS


        ///// <summary>
        ///// Returns the block configuration of the block item given in parameter.
        ///// </summary>
        ///// <param name="block">Block where the block configuration parameters will be found.</param>
        ///// <returns>The block configuration of the block item given in parameter.</returns>
        protected override BlockConfiguration GetBlockConfiguration(BlockItem block)
        {
            // TODO : Finalize Excel alimentation
            //throw new NotImplementedException();
            return null;
        }

        protected virtual BlockConfiguration GetBlockConfiguration(string Description)
        {
            return GetBlockConfiguration(Description, null);
        }


        protected BlockConfiguration GetBlockConfiguration(string alias, string tag)
        {
            BlockConfiguration back = new BlockConfiguration();

            string[] optionList = null;
            string blockOptionStr = "";
            if (!string.IsNullOrWhiteSpace(alias))
            {
                optionList = alias.Replace(@"\r\n", string.Empty).Split(';');
                blockOptionStr = !string.IsNullOrWhiteSpace(tag) ? tag.Replace(@"\r\n", string.Empty) : string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(tag))
        {
                optionList = tag.Replace(@"\r\n", string.Empty).Split(';');
                if (optionList.Length >= 3)
            {
                    blockOptionStr = optionList[2];
            }
        }
            if (null != optionList && optionList.Length >= 2)
            {
                back.Type = optionList[0];
                back.Name = optionList[1];
                if (optionList.Length > 2 && string.IsNullOrWhiteSpace(blockOptionStr))
        {
                    blockOptionStr += string.Format(",{0}", optionList.Skip(2).Aggregate((current, next) => string.Format("{0},{1}", current, next)));
                }
                back.Options = string.IsNullOrWhiteSpace(blockOptionStr) ? new Dictionary<string, string>() : ParseOptions(blockOptionStr);
            }
            return back;
        }

        /// <summary>
        /// Returns all block contained into the container given in argument.
        /// </summary>
        /// <param name="container">Container where the block items will be found.</param>
        /// <returns>All block contained into the container given in argument.</returns>
        protected override List<BlockItem> GetBlocks(OpenXmlPartContainer container)
        {
            // TODO : Finalize Excel alimentation


            throw new NotImplementedException();

        }


        public override void BuildDocument()
        {
            var excelDoc = (SpreadsheetDocument)base.Package;
            var settings = SettingsBLL.GetSetting();

            string strFilePath = settings.ReportingParameter.GeneratedFilePath;
            string strTemplatePath = settings.ReportingParameter.TemplatePath;
            string strTargetFile = ReportData.FileName;


            string fileName = strFinalTempFile;
            //File.Copy(strTargetFile, fileName, true);

            if (strTargetFile != "")
            {
                BuildReportTemplateEFPFlexi(fileName);
            }
            else
            {
                throw new InvalidOperationException("Unable to file the Workbook");
            }
        }


        private static void SetCellValue(Cell cell, string Value)
        {
            decimal dx;
            if (decimal.TryParse(Value, out dx))
            {
                cell.CellValue = new CellValue(dx.ToString("G", NumberFormatInfo.InvariantInfo));
                cell.DataType = CellValues.Number;
            }
            else
            {
                cell.CellValue = new CellValue(Value);
                cell.DataType = CellValues.String;
            }
        }

        private const string FLEXI_PREFIX = "RepGen:";

        private class TableInfo
        {
            public Cell cell;
            public TableDefinition table;
        }

        private void BuildReportTemplateEFPFlexi(string strTargetFile)
        {
            string fileName = strTargetFile;

            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var tableTargets = new List<TableInfo>();

            using (SpreadsheetDocument workbook = SpreadsheetDocument.Open(fileName, true))
            {
                var workbookPart = workbook.WorkbookPart;
                var sharedStringPart = workbookPart.SharedStringTablePart;
                var values = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                foreach (WorksheetPart worksheetpart in workbookPart.WorksheetParts)
                {
                    foreach (var sheetData in worksheetpart.Worksheet.Elements<SheetData>())
                    {
                        // reset accross sheets
                        //Cell FinaleCell = null;
                        //TableDefinition FinaleTable = null;
                        tableTargets.Clear();

                        #region TextPopulate
                        foreach (var cell in sheetData.Descendants<Cell>())
                        {
                            if (cell.CellValue != null)
                            {
                                if (cell.CellFormula != null)
                                {
                                    // force recompute
                                    cell.CellValue.Remove();
                                }
                                else if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                                {
                                    var index = int.Parse(cell.CellValue.Text);
                                    if (values[index].InnerText.StartsWith(FLEXI_PREFIX))
                                    {
                                        string strBlockTypeAndName = values[index].InnerText.Substring(FLEXI_PREFIX.Length);

                                        BlockConfiguration config = GetBlockConfiguration(strBlockTypeAndName);

                                        if (TextBlock.IsMatching(config.Type))
                                        {
                                            TextBlock instance = BlockHelper.GetAssociatedBlockInstance<TextBlock>(config.Name);
                                            if (instance != null)
                                            {
                                                SetCellValue(cell, instance.GetContent(reportData, config.Options));
                                            }
                                        }
                                        else if (TableBlock.IsMatching(config.Type))
                                        {
                                            TableBlock instance = BlockHelper.GetAssociatedBlockInstance<TableBlock>(config.Name);
                                            if (instance != null)
                                            {
                                                tableTargets.Add(new TableInfo
                                                {
                                                    cell = cell,
                                                    table = instance.GetContent(reportData, config.Options)
                                                });
                                                //FinaleCell = cell;
                                                //FinaleTable = instance.GetContent(reportData, config.Options);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion TextPopulate


                        #region TablePopulate
                        foreach (var tableInfo in tableTargets)
                        {
                            var FinaleCell = tableInfo.cell;
                            var FinaleTable = tableInfo.table;

                            int intColumns = FinaleTable.NbColumns;
                            int intRows = FinaleTable.NbRows;

                            // TODO: handle cell references after 'Znn' (AA1, AB1...)
                            // TODO: current limitation: the generated cells must be in the range "A-Z"

                            char firstLetter = FinaleCell.CellReference.InnerText[0];
                            int firstColIdx = alphabet.IndexOf(firstLetter) + 1;
                            int lastColIdx = firstColIdx + intColumns - 1;
                            int curColIdx = firstColIdx;

                            uint firstRowIdx = uint.Parse(FinaleCell.CellReference.InnerText.Substring(1));
                            uint curRowIdx = firstRowIdx;

                            // create first row
                            Row curRow = new Row();

                            foreach (var result in FinaleTable.Data)
                            {
                                // append cell to current row
                                Cell c = new Cell();
                                SetCellValue(c, result.ToString());
                                c.CellReference = alphabet[curColIdx - 1] + curRowIdx.ToString();
                                c.StyleIndex = 0;
                                curRow.Append(c);

                                if (curColIdx == lastColIdx)
                                {
                                    // add row to current worksheet
                                    InsertRow(curRowIdx, worksheetpart, curRow, false);
                                    // create new row for next data
                                    curRow = new Row();

                                    // first cell on next row
                                    curRowIdx++;
                                    curColIdx = firstColIdx;
                                }
                                else
                                {
                                    // next cell
                                    curColIdx++;
                                }
                            }
                            FinaleCell.Parent.RemoveChild(FinaleCell);
                        }

                        workbookPart.Workbook.Save();

                        #endregion TablePopulate
                    }
                }

            }
        }


        private static void UpdateRowIndexes(WorksheetPart worksheetPart, uint rowIndex, bool isDeletedRow)
        {
            // Get all the rows in the worksheet with equal or higher row index values than the one being inserted/deleted for reindexing.
            IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex.Value >= rowIndex);

            foreach (Row row in rows)
            {
                uint newIndex = (isDeletedRow ? row.RowIndex - 1 : row.RowIndex + 1);
                string curRowIndex = row.RowIndex.ToString();
                string newRowIndex = newIndex.ToString();

                foreach (Cell cell in row.Elements<Cell>())
                {
                    // Update the references for the rows cells.
                    cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curRowIndex, newRowIndex));
                }

                // Update the row index.
                row.RowIndex = newIndex;
            }
        }

        private static void UpdateMergedCellReferences(WorksheetPart worksheetPart, uint rowIndex, bool isDeletedRow)
        {
            if (worksheetPart.Worksheet.Elements<MergeCells>().Count() > 0)
            {
                MergeCells mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().FirstOrDefault();

                if (mergeCells != null)
                {
                    // Grab all the merged cells that have a merge cell row index reference equal to or greater than the row index passed in
                    List<MergeCell> mergeCellsList = mergeCells.Elements<MergeCell>()
                                .Where(r => r.Reference.HasValue &&
                                            (GetRowIndex(r.Reference.Value.Split(':').ElementAt(0)) >= rowIndex ||
                                             GetRowIndex(r.Reference.Value.Split(':').ElementAt(1)) >= rowIndex)).ToList();

                    // Need to remove all merged cells that have a matching rowIndex when the row is deleted
                    if (isDeletedRow)
                    {
                        List<MergeCell> mergeCellsToDelete = mergeCellsList.Where(r => GetRowIndex(r.Reference.Value.Split(':').ElementAt(0)) == rowIndex ||
                                                                                       GetRowIndex(r.Reference.Value.Split(':').ElementAt(1)) == rowIndex).ToList();

                        // Delete all the matching merged cells
                        foreach (MergeCell cellToDelete in mergeCellsToDelete)
                        {
                            cellToDelete.Remove();
                        }

                        // Update the list to contain all merged cells greater than the deleted row index
                        mergeCellsList = mergeCells.Elements<MergeCell>()
                                    .Where(r => r.Reference.HasValue &&
                                                (GetRowIndex(r.Reference.Value.Split(':').ElementAt(0)) > rowIndex ||
                                                 GetRowIndex(r.Reference.Value.Split(':').ElementAt(1)) > rowIndex)).ToList();
                    }

                    // Either increment or decrement the row index on the merged cell reference
                    foreach (MergeCell mergeCell in mergeCellsList)
                    {
                        string[] cellReference = mergeCell.Reference.Value.Split(':');

                        if (GetRowIndex(cellReference.ElementAt(0)) >= rowIndex)
                        {
                            string columnName = GetColumnName(cellReference.ElementAt(0));
                            cellReference[0] = isDeletedRow ? columnName + (GetRowIndex(cellReference.ElementAt(0)) - 1).ToString() : IncrementCellReference(cellReference.ElementAt(0), CellReferencePartEnum.Row);
                        }

                        if (GetRowIndex(cellReference.ElementAt(1)) >= rowIndex)
                        {
                            string columnName = GetColumnName(cellReference.ElementAt(1));
                            cellReference[1] = isDeletedRow ? columnName + (GetRowIndex(cellReference.ElementAt(1)) - 1).ToString() : IncrementCellReference(cellReference.ElementAt(1), CellReferencePartEnum.Row);
                        }

                        mergeCell.Reference = new StringValue(cellReference[0] + ":" + cellReference[1]);
                    }
                }
            }
        }

        private static void UpdateHyperlinkReferences(WorksheetPart worksheetPart, uint rowIndex, bool isDeletedRow)
        {
            Hyperlinks hyperlinks = worksheetPart.Worksheet.Elements<Hyperlinks>().FirstOrDefault();

            if (hyperlinks != null)
            {
                Match hyperlinkRowIndexMatch;
                uint hyperlinkRowIndex;

                foreach (Hyperlink hyperlink in hyperlinks.Elements<Hyperlink>())
                {
                    hyperlinkRowIndexMatch = Regex.Match(hyperlink.Reference.Value, "[0-9]+");
                    if (hyperlinkRowIndexMatch.Success && uint.TryParse(hyperlinkRowIndexMatch.Value, out hyperlinkRowIndex) && hyperlinkRowIndex >= rowIndex)
                    {
                        // if being deleted, hyperlink needs to be removed or moved up
                        if (isDeletedRow)
                        {
                            // if hyperlink is on the row being removed, remove it
                            if (hyperlinkRowIndex == rowIndex)
                            {
                                hyperlink.Remove();
                            }
                            // else hyperlink needs to be moved up a row
                            else
                            {
                                hyperlink.Reference.Value = hyperlink.Reference.Value.Replace(hyperlinkRowIndexMatch.Value, (hyperlinkRowIndex - 1).ToString());

                            }
                        }
                        // else row is being inserted, move hyperlink down
                        else
                        {
                            hyperlink.Reference.Value = hyperlink.Reference.Value.Replace(hyperlinkRowIndexMatch.Value, (hyperlinkRowIndex + 1).ToString());
                        }
                    }
                }

                // Remove the hyperlinks collection if none remain
                if (hyperlinks.Elements<Hyperlink>().Count() == 0)
                {
                    hyperlinks.Remove();
                }
            }
        }

        public static string IncrementCellReference(string reference, CellReferencePartEnum cellRefPart)
        {
            string newReference = reference;

            if (cellRefPart != CellReferencePartEnum.None && !String.IsNullOrEmpty(reference))
            {
                string[] parts = Regex.Split(reference, "([A-Z]+)");

                if (cellRefPart == CellReferencePartEnum.Column || cellRefPart == CellReferencePartEnum.Both)
                {
                    List<char> col = parts[1].ToCharArray().ToList();
                    bool needsIncrement = true;
                    int index = col.Count - 1;

                    do
                    {
                        // increment the last letter
                        col[index] = Letters[Letters.IndexOf(col[index]) + 1];

                        // if it is the last letter, then we need to roll it over to 'A'
                        if (col[index] == Letters[Letters.Count - 1])
                        {
                            col[index] = Letters[0];
                        }
                        else
                        {
                            needsIncrement = false;
                        }

                    } while (needsIncrement && --index >= 0);

                    // If true, then we need to add another letter to the mix. Initial value was something like "ZZ"
                    if (needsIncrement)
                    {
                        col.Add(Letters[0]);
                    }

                    parts[1] = new String(col.ToArray());
                }

                if (cellRefPart == CellReferencePartEnum.Row || cellRefPart == CellReferencePartEnum.Both)
                {
                    // Increment the row number. A reference is invalid without this componenet, so we assume it will always be present.
                    parts[2] = (int.Parse(parts[2]) + 1).ToString();
                }

                newReference = parts[1] + parts[2];
            }

            return newReference;
        }



        private static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        public enum CellReferencePartEnum
        {
            None,
            Column,
            Row,
            Both
        }

        private static List<char> Letters = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', ' ' };

        public static uint GetRowIndex(string cellReference)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellReference);

            return uint.Parse(match.Value);
        }


        public static Row InsertRow(uint rowIndex, WorksheetPart worksheetPart, Row insertRow, bool isNewLastRow = false)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            Row retRow = !isNewLastRow ? sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex) : null;

            // If the worksheet does not contain a row with the specified row index, insert one.
            if (retRow != null)
            {
                // if retRow is not null and we are inserting a new row, then move all existing rows down.
                if (insertRow != null)
                {
                    UpdateRowIndexes(worksheetPart, rowIndex, false);
                    UpdateMergedCellReferences(worksheetPart, rowIndex, false);
                    UpdateHyperlinkReferences(worksheetPart, rowIndex, false);

                    // actually insert the new row into the sheet
                    retRow = sheetData.InsertBefore(insertRow, retRow);  // at this point, retRow still points to the row that had the insert rowIndex

                    //string curIndex = retRow.RowIndex.ToString();
                    string curIndex = rowIndex.ToString();
                    string newIndex = rowIndex.ToString();

                    foreach (Cell cell in retRow.Elements<Cell>())
                    {
                        // Update the references for the rows cells.
                        cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curIndex, newIndex));
                    }

                    // Update the row index.
                    retRow.RowIndex = rowIndex;
                }
            }
            else
            {
                // Row doesn't exist yet, shifting not needed.
                // Rows must be in sequential order according to RowIndex. Determine where to insert the new row.
                Row refRow = !isNewLastRow ? sheetData.Elements<Row>().FirstOrDefault(row => row.RowIndex > rowIndex) : null;

                // use the insert row if it exists
                retRow = insertRow ?? new Row() { RowIndex = rowIndex };

                IEnumerable<Cell> cellsInRow = retRow.Elements<Cell>();

                if (cellsInRow.Any())
                {
                    string curIndex = retRow.RowIndex.ToString();
                    string newIndex = rowIndex.ToString();

                    foreach (Cell cell in cellsInRow)
                    {
                        // Update the references for the rows cells.
                        cell.CellReference = new StringValue(cell.CellReference.Value.Replace(curIndex, newIndex));
                    }

                    // Update the row index.
                    retRow.RowIndex = rowIndex;
                }

                sheetData.InsertBefore(retRow, refRow);
            }

            return retRow;
        }





        private static String GetApplicationQualification(ReportData reportData, double value)
        {
            if (value < reportData.Parameter.ApplicationSizeLimitSupSmall)
                return Labels.SizeS;
            else if (value < reportData.Parameter.ApplicationSizeLimitSupMedium)
                return Labels.SizeM;
            else if (value < reportData.Parameter.ApplicationSizeLimitSupLarge)
                return Labels.SizeL;
            else
                return Labels.SizeXL;
        }

        private dynamic GetResultSet(string strBlockName)
        {
            if (strBlockName == "HF_BY_MODULE")
            {
                var resultCurrentSnapshot = BusinessCriteriaUtility.GetBusinessCriteriaGradesModules(reportData.CurrentSnapshot, false);
                return resultCurrentSnapshot;
            }
            else
            {
                return null;
            }
        }


        private char GetNextAlphabet(char letter)
        {
            if (letter == 'z')
                return 'a';
            else if (letter == 'Z')
                return 'A';
            else
                return (char)(((int)letter) + 1);
        }



        private string MakeAnotherWorkingCopy(string strTemplatePath, string strTargetFile)
        {
            if (strTargetFile.Contains("Template_Efp") && strTargetFile.Contains(".xlsx"))
            {
                string fileName = Path.GetTempPath() + "Template_Efp.xlsx";
                File.Copy(strTemplatePath + "\\Template_Efp.xlsx", fileName, true);
                return fileName;
            }
            else
            {
                return "";
            }
        }

        private void BuildReportTemplateEFP(string strNewTargetFile)
        {
            string fileName = strNewTargetFile;
            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileName, true))
            {
                var workbookPart = spreadSheetDocument.WorkbookPart;
                var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "Summary");
                if (sheet == null)
                {
                    throw new InvalidOperationException("Unable to file the Summary Sheet");
                }

                var workSheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                var sharedStringPart = workbookPart.SharedStringTablePart;
                var values = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                var cells = workSheetPart.Worksheet.Descendants<Cell>();

                foreach (var cell in cells)
                {
                    if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                    {
                        var index = int.Parse(cell.CellValue.Text);
                        if (values[index].InnerText == "AFP")
                        {
                            cell.CellValue = new CellValue("100");
                            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                        if (values[index].InnerText == "MFP")
                        {
                            cell.CellValue = new CellValue("150");
                            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                        if (values[index].InnerText == "DFP")
                        {
                            cell.CellValue = new CellValue("200");
                            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                        if (values[index].InnerText == "TOTAL")
                        {
                            cell.CellValue = new CellValue("450");
                            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                    }
                }
                workbookPart.Workbook.Save();
            }
        }


        private string CopySheet(string strTemplatePath, string strTargetFile, string keyName)
        {
            if (strTargetFile.Contains("Template_Efp"))
            {
                string fileName = Path.GetTempPath() + "Template_Efp.xlsx";
                File.Copy(strTemplatePath + "\\Template_Efp.xlsx", fileName, true);
                string keyValue = string.Empty;
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileName, true))
                {
                    var workbookPart = spreadSheetDocument.WorkbookPart;
                    var sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == "Summary");
                    if (sheet == null)
                    {
                        throw new InvalidOperationException("Unable to file the Summary Sheet");
                    }

                    var workSheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    var sharedStringPart = workbookPart.SharedStringTablePart;
                    var values = sharedStringPart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                    var cells = workSheetPart.Worksheet.Descendants<Cell>();

                    foreach (var cell in cells)
                    {
                        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                        {
                            var index = int.Parse(cell.CellValue.Text);
                            if (values[index].InnerText == keyName)
                            {
                                cell.CellValue = new CellValue("123");
                                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                                workbookPart.Workbook.Save();
                            }
                        }
                    }
                }

                return keyValue;
            }
            else
            {
                throw new InvalidOperationException("Unable to file the Workbook");
            }
        }




        public void BuildExcelDocument(DataTable source, string targetFile, string tabName)
        {
            using (OpenXmlMemoryStreamDocument streamDoc = OpenXmlMemoryStreamDocument.CreateSpreadsheetDocument())
            {
                using (SpreadsheetDocument doc = streamDoc.GetSpreadsheetDocument())
                {
                    WorksheetAccessor.CreateDefaultStyles(doc);
                    // We could use TableName as tabName:   source.TableName
                    WorksheetPart sheetPart = WorksheetAccessor.AddWorksheet(doc, tabName);

                    WriteDatasheet(doc, sheetPart, source, tabName);
                }
                //streamDoc.GetModifiedSmlDocument().SaveAs(targetFile);
                streamDoc.GetModifiedSmlDocument().SaveAs(targetFile);
            }
        }

        private void WriteDatasheet(SpreadsheetDocument doc, WorksheetPart worksheetPart, DataTable source, string tabName)
        {
            MemorySpreadsheet spreadSheet = new MemorySpreadsheet();
            int rowNum = 1;
            int colNum = 1;
            int maxRow = source.Rows.Count + 1; // 1 for header
            int maxCol = source.Columns.Count;

            foreach (DataColumn oneColumn in source.Columns)
            {
                spreadSheet.SetCellValue(rowNum, colNum, oneColumn.ColumnName,
                                             GetStyleIndex(doc, rowNum, colNum, maxRow, maxCol));
                colNum += 1;
            }
            rowNum += 1;
            foreach (DataRow oneRow in source.Rows)
            {
                for (colNum = 1; colNum <= source.Columns.Count; colNum += 1)
                    spreadSheet.SetCellValue(rowNum, colNum, oneRow[colNum - 1] /* Index starts at zero */,
                                             GetStyleIndex(doc, rowNum, colNum, maxRow, maxCol));
                rowNum += 1;
            }
            WorksheetAccessor.SetSheetContents(doc, worksheetPart, spreadSheet);
            WorksheetAccessor.SetRange(doc, tabName, tabName, 1, 1, rowNum - 1, source.Columns.Count);
        }

        private void SetColumns(SpreadsheetDocument doc, WorksheetPart worksheetPart, int maxRow, int maxCol)
        {
            var worksheet = worksheetPart.Worksheet;

            var columns = new DocumentFormat.OpenXml.Spreadsheet.Columns();
            var col = new DocumentFormat.OpenXml.Spreadsheet.Column();
            col.Min = 1;
            col.Max = Convert.ToUInt32(maxCol);
            col.BestFit = true;

            columns.Append(col);
            worksheet.Append(columns);
            worksheet.Save();
        }

        private int GetStyleIndex(SpreadsheetDocument doc, int row, int col, int maxRow, int maxCol)
        {
            int numFmt = 0;
            int font = 0;
            int fill = 0;
            WorksheetAccessor.Border border = new WorksheetAccessor.Border();
            WorksheetAccessor.CellAlignment alignment = new WorksheetAccessor.CellAlignment();
            bool hidden = false;
            bool locked = false;
            string colorHtmlStr = "FF3F3F3F";

            // Header
            if (row == 1)
            {
                //numFmt = 3;
                font = WorksheetAccessor.GetFontIndex(doc, new WorksheetAccessor.Font
                {
                    Size = 12,
                    Color =
                        new WorksheetAccessor.ColorInfo(
                        WorksheetAccessor.ColorInfo.ColorType.Indexed, 0),
                    Name = "Calibri",
                    Family = 2,
                    Scheme = WorksheetAccessor.Font.SchemeType.Minor
                });

                border.Top = new WorksheetAccessor.BorderLine(
                    WorksheetAccessor.BorderLine.LineStyle.Medium,
                    new WorksheetAccessor.ColorInfo(colorHtmlStr));

                alignment.HorizontalAlignment = WorksheetAccessor.CellAlignment.Horizontal.Center;

                fill = WorksheetAccessor.GetFillIndex(doc, new WorksheetAccessor.PatternFill(
                    WorksheetAccessor.PatternFill.PatternType.Solid,
                        new WorksheetAccessor.ColorInfo(WorksheetAccessor.ColorInfo.ColorType.Indexed, 1),
                        new WorksheetAccessor.ColorInfo(WorksheetAccessor.ColorInfo.ColorType.Theme, 4)
                        ));

                //fill = WorksheetAccessor.GetFillIndex(doc,
                //                                      new WorksheetAccessor.PatternFill(
                //                                          WorksheetAccessor.PatternFill.PatternType.Solid,
                //                                          new WorksheetAccessor.ColorInfo(
                //                                              WorksheetAccessor.ColorInfo.ColorType.Theme, 2),
                //                                          new WorksheetAccessor.ColorInfo(
                //                                              WorksheetAccessor.ColorInfo.ColorType.Theme, 2)));
            }
            else
            {
                //numFmt = 1;
                //font = WorksheetAccessor.GetFontIndex(doc, new WorksheetAccessor.Font
                //                                               {
                //                                                   Bold = true,
                //                                                   Size = 8,
                //                                                   Color = 
                //                                                       new WorksheetAccessor.ColorInfo(WorksheetAccessor.ColorInfo.ColorType.Indexed, 0),
                //                                                   Name = "Calibri",
                //                                                   Family = 1
                //                                               });
                border.Top = new WorksheetAccessor.BorderLine(
                    WorksheetAccessor.BorderLine.LineStyle.Thin,
                    new WorksheetAccessor.ColorInfo(colorHtmlStr));

                if (row % 2 == 0)
                {
                    fill = WorksheetAccessor.GetFillIndex(doc,
                                                          new WorksheetAccessor.PatternFill(
                                                              WorksheetAccessor.PatternFill.PatternType.Solid,
                                                              null,
                                                              new WorksheetAccessor.ColorInfo(2, 0.79998168889431442)));
                }
                else
                {
                    fill = WorksheetAccessor.GetFillIndex(doc,
                                                          new WorksheetAccessor.PatternFill(
                                                              WorksheetAccessor.PatternFill.PatternType.Solid,
                                                              null,
                                                              new WorksheetAccessor.ColorInfo(2, 0.59999389629810485)));
                }
            }

            // Left / Right in bold
            if (col == 1)
                border.Left = new WorksheetAccessor.BorderLine(
                    WorksheetAccessor.BorderLine.LineStyle.Medium,
                    new WorksheetAccessor.ColorInfo(colorHtmlStr));
            else
                border.Left = new WorksheetAccessor.BorderLine(
                    WorksheetAccessor.BorderLine.LineStyle.Thin,
                    new WorksheetAccessor.ColorInfo(colorHtmlStr));

            if (col == maxCol)
                border.Right = new WorksheetAccessor.BorderLine(
                    WorksheetAccessor.BorderLine.LineStyle.Medium,
                    new WorksheetAccessor.ColorInfo(colorHtmlStr));
            else
                border.Right = new WorksheetAccessor.BorderLine(
                        WorksheetAccessor.BorderLine.LineStyle.Thin,
                        new WorksheetAccessor.ColorInfo(colorHtmlStr));

            // Last Row or Bottom row of the Header
            if (row == maxRow || row == 1)
                border.Bottom =
                    new WorksheetAccessor.BorderLine(
                        WorksheetAccessor.BorderLine.LineStyle.Medium,
                        new WorksheetAccessor.ColorInfo(colorHtmlStr));
            else
                border.Bottom =
                            new WorksheetAccessor.BorderLine(
                                WorksheetAccessor.BorderLine.LineStyle.Thin,
                                new WorksheetAccessor.ColorInfo(colorHtmlStr));

            return WorksheetAccessor.GetStyleIndex(doc, numFmt, font, fill, WorksheetAccessor.GetBorderIndex(doc, border), alignment, hidden, locked);
        }




        #endregion METHODS
    }
}
