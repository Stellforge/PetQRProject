namespace SimpleProject.Domain.Dtos
{
    public class ScanEventDto : EntityDto
    {
        public int QrCodeId { get; set; }
        public int? PetId { get; set; }
        public int LostReportId { get; set; }
        public int ResultType { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public decimal? ScanLat { get; set; }
        public decimal? ScanLng { get; set; }
        public string? Notes { get; set; }
        public string? FinderPhone { get; set; }
        public string? FinderPhoto { get; set; }
    }
}
