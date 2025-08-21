namespace SimpleProject.Domain.Dtos;

public class ResizeConfig
{
    public string? Suffix { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int LowQualitySize { get; set; } = 512; //KB
    public string? BackGround { get; set; }
}
