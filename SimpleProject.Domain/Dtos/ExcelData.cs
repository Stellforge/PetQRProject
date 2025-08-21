namespace SimpleProject.Domain.Dtos;
public class ExcelData<T> where T : new()
{
    public T Data { get; set; }
    public List<string> Columns { get; set; }
    public string? Error { get; set; }

    public ExcelData()
    {
        Data = new T();
        Columns = [];
    }
}