using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace SimpleProject.Services;
public interface IExcelService : IServiceBase, IScopedService
{
    Task<Result<ExcelUpload>> SaveExcelUpload(ExcelUploadDto data);
    Task<Result> DeleteExcelUpload(int id);
    internal Task DeleteExcelUpload(ExcelUpload entity, bool isTransactional);

    Task<Result> SaveExcel<TDto, TEntity>(ExcelUploadDto data, List<ExcelColumn<TDto>> columns, Func<TDto, Task<Result<TEntity>>> method, Action<TDto>? setValueMethod = null) where TDto : EntityDto, new() where TEntity : Entity, new();

    Result Save<T>(Stream stream, IEnumerable<T> data, IEnumerable<ExcelColumn<T>> columns) where T : new();
    Result<List<ExcelData<T>>> GetData<T>(string excelPath, IEnumerable<ExcelColumn<T>> columns, out int columnCount) where T : new();
    Result<int> GetRowCount(string excelPath);
    Result UpdateCell(string excelPath, Dictionary<int, string?> errors, int columnIndex);
    Result UpdateColumn(string excelPath, Dictionary<string, object?> columns);
}

public partial class ExcelService : ServiceBase, IExcelService
{
    private IExcelService Self => this;
    private readonly IRepository<ExcelUpload> _repositoryExcelUpload;
    private readonly IRepository<AdminUser> _repositoryAdminUser;

    public ExcelService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _repositoryExcelUpload = _serviceProvider.GetRequiredService<IRepository<ExcelUpload>>();
        _repositoryAdminUser = _serviceProvider.GetRequiredService<IRepository<AdminUser>>();
    }

    private readonly string _decimalSeperator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

    public async Task<Result<ExcelUpload>> SaveExcelUpload(ExcelUploadDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<ExcelUpload>(validationResult);
            }

            var entity = (ExcelUpload)data;
            var oldEntity = default(ExcelUpload);
            if (entity.Id > 0)
            {
                oldEntity = await _repositoryExcelUpload.Get(a => a.Id == entity.Id);
                if (oldEntity == null)
                {
                    throw new BusException("Kayıt bulunamadı");
                }
                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity == null || oldEntity.AdminUserId != entity.AdminUserId)
            {
                var exists = await _repositoryAdminUser.Any(a => a.Id == entity.AdminUserId);
                if (!exists)
                {
                    throw new BusException("Sistem kullanıcısı bulunamadı");
                }
            }

            if (entity.Id > 0)
            {
                await _repositoryExcelUpload.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryExcelUpload.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            return new Result<ExcelUpload>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<ExcelUpload>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteExcelUpload(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var entity = await _repositoryExcelUpload.Get(a => a.Id == id, a=> new ExcelUpload()
            {
                Id = a.Id
            }) ?? throw new BusException("Kayıt bulunamaıd");

            await Self.DeleteExcelUpload(entity, isTransactional);

            return new Result();
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result(await _logService.LogException(ex));
            }
            throw;
        }
    }
    async Task IExcelService.DeleteExcelUpload(ExcelUpload entity, bool isTransactional)
    {
        await _repositoryExcelUpload.Delete(entity);

        await _logService.LogDeleteHistory(entity);
    }

    public async Task<Result> SaveExcel<TDto, TEntity>(ExcelUploadDto data, List<ExcelColumn<TDto>> columns, Func<TDto, Task<Result<TEntity>>> method, Action<TDto>? setValueMethod = null) where TDto : EntityDto, new() where TEntity : Entity, new()
    {
        try
        {
            var excelData = GetData(data.FilePath!, columns, out int columnCount);
            if (excelData.HasError)
            {
                return new Result(excelData);
            }

            var errors = new Dictionary<int, string?>();
            var row = data.Success + data.Fail + 1; //+1 -> header

            var convertMethod = Domain.Extensions.GetConvertExpression<TEntity, TDto>().Compile();
            foreach (var rowData in excelData.Data!.Skip(row - 1).ToList())
            {
                row++;
                if (!string.IsNullOrEmpty(rowData.Error) || rowData.Columns.Count == 0)
                {
                    string? error = rowData.Error;
                    if (rowData.Columns.Count == 0)
                    {
                        error += "Kolon bulunamadı";
                    }
                    errors.Add(row, error);
                    data.Fail++;
                }
                else
                {
                    var hasError = false;
                    if (rowData.Data.Id > 0)
                    {
                        var entity = await Get<TEntity>(a => a.Id == rowData.Data.Id);
                        if (entity.Data == null)
                        {
                            data.Fail++;
                            errors.Add(row, "Kayıt bulunamadı");
                            hasError = true;
                        }
                        else
                        {
                            rowData.Data = ExcelColumn<TDto>.SetObjectValues(convertMethod(entity.Data), rowData, columns);
                        }
                    }
                    if (!hasError)
                    {
                        setValueMethod?.Invoke(rowData.Data);
                        var saveResult = await method(rowData.Data);
                        if (saveResult.HasError)
                        {
                            data.Fail++;
                            errors.Add(row, string.Join(", ", saveResult.Errors));
                        }
                        else
                        {
                            data.Success++;
                        }
                    }
                }
                if (data.Id > 0)
                {
                    await SaveExcelUpload(data);
                }
            }

            if (errors.Count > 0)
            {
                UpdateCell(data.FilePath!, errors, columnCount + 1);
                data.ErrorFilePath = "/" + data.FilePath![data.FilePath!.IndexOf("upload" + Path.DirectorySeparatorChar + "excel")..].Replace(Path.DirectorySeparatorChar, '/');
            }
            data.Completed = true;

            if (data.Id > 0)
            {
                await SaveExcelUpload(data);
            }
            return new Result();
        }
        catch (Exception ex)
        {
            if (data.Id > 0)
            {
                data.ErrorMessage = ex.Message;
                data.Completed = true;
                await SaveExcelUpload(data);
            }
            return new Result(await _logService.LogException(ex));
        }
    }

    public Result Save<T>(Stream stream, IEnumerable<T> data, IEnumerable<ExcelColumn<T>> columns) where T : new()
    {
        //0   General
        //1   0
        //2   0.00
        //3   #,##0
        //4   #,##0.00
        //9   0 %
        //10  0.00 %
        //11  0.00E+00
        //12  # ?/?
        //13  # ??/??
        //14  d / m / yyyy
        //15  d - mmm - yy
        //16  d - mmm
        //17  mmm - yy
        //18  h: mm tt
        //19  h: mm: ss tt
        //20  H: mm
        //21  H: mm: ss
        //22  m / d / yyyy H: mm
        //37  #,##0 ;(#,##0)
        //38  #,##0 ;[Red](#,##0)
        //39  #,##0.00;(#,##0.00)
        //40  #,##0.00;[Red](#,##0.00)
        //45  mm: ss
        //46[h]:mm: ss
        //47  mmss.0
        //48  ##0.0E+0
        //49  @
        try
        {
            using (var spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                var workbookStylesPart = workbookpart.AddNewPart<WorkbookStylesPart>();
                using (var writer = OpenXmlWriter.Create(workbookStylesPart))
                {
                    writer.WriteElement(new Stylesheet
                    {
                        Fonts = new Fonts(new Font()),
                        Fills = new Fills(new Fill()),
                        Borders = new Borders(new Border()),
                        CellStyleFormats = new CellStyleFormats(new CellFormat()),
                        CellFormats = new CellFormats(
                            new CellFormat(),
                            new CellFormat
                            {
                                NumberFormatId = 22,
                                ApplyNumberFormat = true
                            }
                        )
                    });
                    writer.Close();
                }

                uint row = 1;
                var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                using (var writer = OpenXmlWriter.Create(worksheetPart))
                {
                    writer.WriteStartElement(new Worksheet());
                    writer.WriteStartElement(new SheetData());
                    writer.WriteElement(new SheetDimension() { Reference = new StringValue("A1:" + GetColumnName(columns.Count()) + (data.Count() + 1)) });

                    writer.WriteStartElement(new Row() { RowIndex = row });
                    for (int i = 0; i < columns.Count(); i++)
                    {
                        writer.WriteElement(new Cell() { CellValue = new CellValue(columns.ElementAt(i).Title ?? string.Empty), DataType = CellValues.String, CellReference = GetColumnName(i + 1) + row });
                    }
                    writer.WriteEndElement(); //Row


                    foreach (var item in data)
                    {
                        row++;
                        writer.WriteStartElement(new Row() { RowIndex = row });

                        for (int i = 0; i < columns.Count(); i++)
                        {
                            var column = columns.ElementAt(i);

                            var dataCell = GetDataCell(column.DataType, column.Value(item));
                            if (dataCell != null)
                            {
                                dataCell.CellReference = GetColumnName(i + 1) + row;
                                writer.WriteElement(dataCell);
                            }
                        }
                        writer.WriteEndElement();//Row
                    }

                    writer.WriteEndElement();//SheetData
                    writer.WriteElement(new AutoFilter() { Reference = new StringValue("A1:" + GetColumnName(columns.Count()) + (data.Count() + 1)) });

                    writer.WriteEndElement();//WorkSheet
                    writer.Close();
                }

                Sheets sheets = workbookpart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new()
                {
                    Id = workbookpart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Data"
                };
                sheets.Append(sheet);
                spreadsheetDocument.Dispose();
            }

            return new Result();
        }
        catch (Exception ex)
        {
            return new Result(ex.Message);
        }
    }
    public Result<List<ExcelData<T>>> GetData<T>(string excelPath, IEnumerable<ExcelColumn<T>> columns, out int columnCount) where T : new()
    {
        var list = new List<ExcelData<T>>();
        columnCount = 0;
        try
        {
            using (SpreadsheetDocument xlRead = SpreadsheetDocument.Open(excelPath, false))
            {
                var workbookPart = xlRead.WorkbookPart ?? throw new ArgumentException("workbookPart");
                Sheet firstSheet = workbookPart.Workbook.Descendants<Sheet>().First();
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id!);

                var row = 0;
                var col = 0;
                var header = new Dictionary<int, ExcelColumn<T>>();
                var headerTitle = new Dictionary<int, string?>();

                using (var reader = OpenXmlReader.Create(worksheetPart))
                {
                    IEnumerable<SharedStringItem>? sharedStrings = null;
                    if (workbookPart.SharedStringTablePart != null)
                    {
                        sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>();
                    }

                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(Row))
                        {
                            col = 0;
                            var refCol = 0;
                            reader.ReadFirstChild();
                            var hasRef = false;
                            var data = new ExcelData<T>();

                            do
                            {
                                if (reader.ElementType == typeof(Cell))
                                {
                                    if (reader.LoadCurrentElement() is not Cell cell)
                                    {
                                        continue;
                                    }

                                    refCol = col;
                                    if (cell.CellReference != null && cell.CellReference.HasValue)
                                    {
                                        col = GetColumnIndex(cell.CellReference?.Value!);
                                        hasRef = true;
                                    }

                                    if (cell.CellFormula != null)
                                    {
                                        cell.CellFormula = null;
                                    }

                                    string? value;
                                    if (cell.DataType != null && cell.DataType == CellValues.SharedString && sharedStrings != null)
                                    {
                                        SharedStringItem ssi = sharedStrings.ElementAt(int.Parse(cell.CellValue!.Text));
                                        value = ssi.Text?.Text;
                                        //hata exceline dogru yazabilmek icin
                                        cell.DataType = CellValues.String;
                                        cell.CellValue = new CellValue(value ?? string.Empty);
                                    }
                                    else if (cell.CellValue != null)
                                    {
                                        value = cell.CellValue.Text;
                                    }
                                    else
                                    {
                                        value = null;
                                    }

                                    if (!string.IsNullOrEmpty(value) && value.StartsWith('='))
                                    {
                                        value = value.TrimStart('=');
                                    }

                                    if (row == 0)
                                    {
                                        headerTitle[col] = value;
                                        var excelColumn = columns.FirstOrDefault(a => a.Title == value);
                                        if (excelColumn == null)
                                        {
                                            //data.Error += value + " kolonu eşleştirimemiş.";
                                            //columnCount++;
                                            continue;
                                        }
                                        header[col] = excelColumn;
                                        columnCount++;
                                    }
                                    else
                                    {
                                        if (header.TryGetValue(col, out ExcelColumn<T>? column))
                                        {
                                            try
                                            {
                                                if (column.DataType != ExcelDataType.STRING && column.DataType != ExcelDataType.BOOLEAN)
                                                {
                                                    value = NuberClearRegex().Replace(value ?? string.Empty, "");
                                                }
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    var columnType = column.PropertyType!;
                                                    if (columnType.IsNullableType())
                                                    {
                                                        columnType = Nullable.GetUnderlyingType(columnType)!;
                                                    }

                                                    if (column.DataType == ExcelDataType.DATE)
                                                    {
                                                        value = value.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                                        column.SetValue(data.Data, DateTime.FromOADate(Convert.ToDouble(value)));
                                                    }
                                                    else if (column.DataType == ExcelDataType.BOOLEAN)
                                                    {
                                                        value = value.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                                        column.SetValue(data.Data, value == "1" || value.Equals("true", StringComparison.InvariantCultureIgnoreCase) || value.Equals("doğru", StringComparison.InvariantCultureIgnoreCase));
                                                    }
                                                    else if (column.DataType == ExcelDataType.FLOAT)
                                                    {
                                                        value = value.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                                        value = double.Parse(value, System.Globalization.NumberStyles.Float).ToString();
                                                        column.SetValue(data.Data, Convert.ChangeType(value, columnType));
                                                    }
                                                    else if (column.DataType == ExcelDataType.NUMBER)
                                                    {
                                                        if (columnType.IsEnum)
                                                        {
                                                            column.SetValue(data.Data, Enum.ToObject(columnType, Convert.ChangeType(value, columnType)));
                                                        }
                                                        else
                                                        {
                                                            column.SetValue(data.Data, Convert.ChangeType(value, columnType));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        column.SetValue(data.Data, Convert.ChangeType(value, columnType));
                                                    }
                                                }
                                                data.Columns.Add(column.Name!);
                                            }
                                            catch (BusException ex)
                                            {
                                                data.Error += headerTitle[col] + ex.Message + ", ";
                                            }
                                            catch
                                            {
                                                data.Error += headerTitle[col] + " yanlış formatta, ";
                                            }
                                        }
                                        //else if (!header.ContainsKey(col) && headerTitle.ContainsKey(col))
                                        //{
                                        //    data.Error += headerTitle[col] + " kolonu bulunamadı.";
                                        //}
                                    }
                                    if (hasRef)
                                    {
                                        col = refCol;
                                    }
                                    col++;
                                }
                            } while (reader.ReadNextSibling());
                            if (row > 0)
                            {
                                list.Add(data);
                            }
                            row++;
                        }
                    }
                    reader.Close();
                }
                xlRead.Dispose();
            }

            return new Result<List<ExcelData<T>>>() { Data = list };
        }
        catch (Exception ex)
        {
            return new Result<List<ExcelData<T>>>(ex.Message);
        }
    }
    public Result<int> GetRowCount(string excelPath)
    {
        try
        {
            var rowCount = 0;
            using (SpreadsheetDocument xlRead = SpreadsheetDocument.Open(excelPath, false))
            {
                var workbookPart = xlRead.WorkbookPart ?? throw new ArgumentException("workbookPart");
                Sheet firstSheet = workbookPart.Workbook.Descendants<Sheet>().First();
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id!);
                var sheetDatas = worksheetPart.Worksheet.Elements<SheetData>();
                if (sheetDatas != null && sheetDatas.Any())
                {
                    var sheetData = sheetDatas.First();
                    rowCount = sheetData.Elements<Row>().Count();
                }
                xlRead.Dispose();
            }
            return new Result<int>() { Data = rowCount };
        }
        catch (Exception ex)
        {
            return new Result<int>(ex.Message);
        }
    }
    public Result UpdateCell(string excelPath, Dictionary<int, string?> errors, int columnIndex)
    {
        try
        {
            using (SpreadsheetDocument xlRead = SpreadsheetDocument.Open(excelPath, true))
            {
                var workbookPart = xlRead.WorkbookPart;
                if (workbookPart != null)
                {
                    var worksheetPart = workbookPart.WorksheetParts.First();
                    foreach (var item in errors)
                    {
                        var cell = GetCell(worksheetPart.Worksheet, columnIndex, item.Key);
                        if (cell != null)
                        {
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(item.Value ?? string.Empty);
                        }
                    }

                    worksheetPart.Worksheet.Save();
                }
            }
            return new Result();
        }
        catch (Exception ex)
        {
            return new Result(ex.Message);
        }
    }
    public Result UpdateColumn(string excelPath, Dictionary<string, object?> columns)
    {
        try
        {
            if (columns.Count == 0)
            {
                return new Result();
            }
            using (SpreadsheetDocument xlRead = SpreadsheetDocument.Open(excelPath, true))
            {
                var workbookPart = xlRead.WorkbookPart;
                if (workbookPart != null)
                {
                    var worksheetPart = workbookPart.WorksheetParts.First();
                    var rowCount = worksheetPart.Worksheet.GetFirstChild<SheetData>()?.Elements<Row>().Count();
                    if (rowCount.GetValueOrDefault() == 0)
                    {
                        return new Result();
                    }

                    var header = GetRow(worksheetPart.Worksheet, 1);
                    if (header == null)
                    {
                        return new Result();
                    }
                    var headerCells = header.Elements<Cell>();
                    var lastColumnIndex = headerCells.Max(a => GetColumnIndex(a.CellReference?.Value ?? "") + 2);
                    if (lastColumnIndex <= 0 || lastColumnIndex < headerCells.Count() + 1)
                    {
                        lastColumnIndex = headerCells.Count() + 1;
                    }

                    var columnIndexes = new List<(ExcelDataType Type, int Index, object? Value)>();
                    foreach (var item in columns)
                    {
                        var index = -1;
                        var count = 1;
                        foreach (var cell in headerCells)
                        {
                            if (item.Key == cell.CellValue?.Text)
                            {
                                if (cell.CellReference != null && cell.CellReference.HasValue)
                                {
                                    index = GetColumnIndex(cell.CellReference.Value!) + 1;
                                }
                                else
                                {
                                    index = count;
                                }
                                break;
                            }
                            count++;
                        }
                        if (index == -1)
                        {
                            var columnName = GetColumnName(lastColumnIndex);

                            header.AppendChild(new Cell() { CellReference = columnName + 1, DataType = CellValues.String, CellValue = new CellValue(item.Key) });
                            index = lastColumnIndex;
                            lastColumnIndex++;
                        }
                        columnIndexes.Add((GetDataType(item.Value), index, item.Value));
                    }

                    for (int i = 2; i < rowCount.GetValueOrDefault() + 1; i++)
                    {
                        foreach (var item in columnIndexes)
                        {
                            var cell = GetCell(worksheetPart.Worksheet, item.Index, i);
                            if (cell != null)
                            {
                                if (item.Type == ExcelDataType.STRING)
                                {
                                    cell.DataType = CellValues.String;
                                }
                                else if (item.Type == ExcelDataType.NUMBER || item.Type == ExcelDataType.FLOAT)
                                {
                                    cell.DataType = CellValues.Number;
                                }
                                else if (item.Type == ExcelDataType.DATE)
                                {
                                    cell.DataType = CellValues.Number;
                                    cell.StyleIndex = 1;
                                }
                                else if (item.Type == ExcelDataType.BOOLEAN)
                                {
                                    cell.DataType = CellValues.Boolean;
                                }
                                cell.CellValue = GetCellValue(item.Type, item.Value);
                            }
                        }
                    }
                    worksheetPart.Worksheet.Save();
                }
            }
            return new Result();
        }
        catch (Exception ex)
        {
            return new Result(ex.Message);
        }
    }

    private Cell? GetDataCell(ExcelDataType? dataType, object? value)
    {
        var cellValue = GetCellValue(dataType, value);
        if (dataType == ExcelDataType.STRING)
        {
            return new Cell() { CellValue = cellValue, DataType = CellValues.String };
        }
        else if (dataType == ExcelDataType.NUMBER || dataType == ExcelDataType.FLOAT)
        {
            return new Cell() { CellValue = cellValue, DataType = CellValues.Number };
        }
        else if (dataType == ExcelDataType.DATE)
        {
            return new Cell() { StyleIndex = 1, CellValue = cellValue, DataType = CellValues.Number };
        }
        else if (dataType == ExcelDataType.BOOLEAN)
        {
            return new Cell() { CellValue = cellValue, DataType = CellValues.Boolean };
        }
        return default;
    }
    private CellValue GetCellValue(ExcelDataType? dataType, object? value)
    {
        if (dataType == ExcelDataType.STRING)
        {
            var strValue = value?.ToString();
            if (strValue != null && strValue.StartsWith('='))
            {
                strValue = strValue.TrimStart('=');
            }
            return new CellValue(strValue == null ? "" : ToValidXmlString(strValue));
        }
        else if (dataType == ExcelDataType.NUMBER || dataType == ExcelDataType.FLOAT)
        {
            return new CellValue(value == null ? "" : Convert.ToString(value)!.Replace(_decimalSeperator, "."));
        }
        else if (dataType == ExcelDataType.DATE)
        {
            return new CellValue(value == null || (DateTime)value == DateTime.MinValue ? "" : Convert.ToString(((DateTime)value).ToOADate()).Replace(_decimalSeperator, "."));
        }
        else if (dataType == ExcelDataType.BOOLEAN)
        {
            return new CellValue(value == null ? "" : value.Equals(true) ? "1" : "0");
        }
        return new CellValue("");
    }
    private static string GetColumnName(int columnNumber)
    {
        int dividend = columnNumber;
        string columnName = string.Empty;
        int modulo;

        while (dividend > 0)
        {
            modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }
    private static int GetColumnIndex(string cellReference)
    {
        if (string.IsNullOrEmpty(cellReference))
        {
            return -1;
        }

        string columnReference = CellReferenceRegex().Replace(cellReference.ToUpper(), "");

        int columnNumber = -1;
        int mulitplier = 1;

        foreach (char c in columnReference.ToCharArray().Reverse())
        {
            columnNumber += mulitplier * (c - 64);
            mulitplier *= 26;
        }

        return columnNumber;
    }
    private static Cell? GetCell(Worksheet worksheet, int columnIndex, int rowIndex)
    {
        var row = GetRow(worksheet, rowIndex);
        if (row == null)
        {
            return null;
        }
        var columnName = GetColumnName(columnIndex);
        var cells = row.Elements<Cell>();
        var cell = cells.FirstOrDefault(c => string.Compare(c.CellReference?.Value, columnName + rowIndex, true) == 0);
        if (cell == null && cells.Count() > columnIndex - 1)
        {
            var tmp = cells.ElementAt(columnIndex - 1);
            if (string.IsNullOrEmpty(tmp.CellReference?.Value))
            {
                cell = tmp;
            }
        }
        if (cell == null)
        {
            cell = new Cell() { CellReference = columnName + rowIndex, DataType = CellValues.String };
            row.AppendChild(cell);
        }
        return cell;
    }
    private static Row? GetRow(Worksheet worksheet, int rowIndex)
    {
        var rows = worksheet.GetFirstChild<SheetData>()!.Elements<Row>();
        var row = rows.FirstOrDefault(a => a.RowIndex?.Value == rowIndex);
        if (row == null && rows.Count() > rowIndex - 1)
        {
            var tmp = rows.ElementAt(rowIndex - 1);
            if ((tmp.RowIndex?.Value).HasValue)
            {
                row = tmp;
            }
        }
        return row;
    }
    private static string ToValidXmlString(string source)
    {
        if (source == null)
        {
            return string.Empty;
        }
        return new string([.. source.Where(ch => System.Xml.XmlConvert.IsXmlChar(ch))]);
    }
    private static ExcelDataType GetDataType(object? obj)
    {
        if (obj == null)
        {
            return ExcelDataType.STRING;
        }

        var type = obj.GetType();
        if (type == typeof(string))
        {
            return ExcelDataType.STRING;
        }
        else
        {
            var nulllable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (nulllable)
            {
                type = Nullable.GetUnderlyingType(type)!;
            }

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            {
                return ExcelDataType.NUMBER;
            }
            else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
            {
                return ExcelDataType.FLOAT;
            }
            else if (type == typeof(DateTime))
            {
                return ExcelDataType.DATE;
            }
            else if (type == typeof(bool))
            {
                return ExcelDataType.BOOLEAN;
            }
        }
        return ExcelDataType.STRING;
    }

    [GeneratedRegex("[^0-9.]")]
    private static partial Regex NuberClearRegex();

    [GeneratedRegex("[\\d]")]
    private static partial Regex CellReferenceRegex();
}
