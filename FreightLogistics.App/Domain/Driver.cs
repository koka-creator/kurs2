namespace FreightLogistics.App.Domain
{
    public class Driver
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public bool Available { get; set; } = true;
    }
}
