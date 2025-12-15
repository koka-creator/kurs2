using FreightLogistics.App.Domain;

namespace FreightLogistics.App.Repositories
{
    public interface ITruckRepository : IRepository<Truck> {}
    public interface IDriverRepository : IRepository<Driver> {}
    public interface IShipmentRepository : IRepository<Shipment> {}

    public class TruckRepository : InMemoryRepository<Truck>, ITruckRepository
    {
        public TruckRepository() : base(t => t.Id, (t, id) => t.Id = id) { }
    }

    public class DriverRepository : InMemoryRepository<Driver>, IDriverRepository
    {
        public DriverRepository() : base(d => d.Id, (d, id) => d.Id = id) { }
    }

    public class ShipmentRepository : InMemoryRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository() : base(s => s.Id, (s, id) => s.Id = id) { }
    }
}
