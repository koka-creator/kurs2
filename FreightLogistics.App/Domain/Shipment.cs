using System;

namespace FreightLogistics.App.Domain
{
    public class Shipment
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Cargo Cargo { get; set; } = new Cargo();
        public int? TruckId { get; set; }
        public int? DriverId { get; set; }
        public double DistanceKm { get; set; }
        public DateTime PlannedDate { get; set; } = DateTime.Today;
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Planned;
        public decimal Cost { get; set; }
    }
}
