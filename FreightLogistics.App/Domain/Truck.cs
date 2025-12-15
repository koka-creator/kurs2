namespace FreightLogistics.App.Domain
{
    public class Truck
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public double CapacityTons { get; set; }
        public double FuelConsumptionPer100Km { get; set; }
        public TruckStatus Status { get; set; } = TruckStatus.Available;
    }
}
