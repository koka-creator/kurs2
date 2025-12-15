using System;
using System.Collections.Generic;
using System.Linq;
using FreightLogistics.App.Domain;
using FreightLogistics.App.Repositories;

namespace FreightLogistics.App.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IShipmentRepository _shipmentRepository;

        private const decimal BaseRatePerKm = 1.20m;
        private const decimal WeightRatePerTonPerKm = 0.50m;

        public ShipmentService(ITruckRepository truckRepository, IDriverRepository driverRepository, IShipmentRepository shipmentRepository)
        {
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _shipmentRepository = shipmentRepository;
        }

        public Shipment CreateShipment(string cargoDescription, double cargoWeightTons, bool refrigerated, double distanceKm, DateTime plannedDate)
        {
            if (cargoWeightTons <= 0) throw new ArgumentException("Cargo weight must be positive");
            if (distanceKm <= 0) throw new ArgumentException("Distance must be positive");

            var shipment = new Shipment
            {
                OrderNumber = "",
                Cargo = new Cargo
                {
                    Description = cargoDescription,
                    WeightTons = cargoWeightTons,
                    RequiresRefrigeration = refrigerated
                },
                DistanceKm = distanceKm,
                PlannedDate = plannedDate,
                Status = ShipmentStatus.Planned
            };

            shipment.Cost = CalculateCost(shipment.DistanceKm, shipment.Cargo.WeightTons);
            return _shipmentRepository.Add(shipment);
        }

        public Shipment? AssignResources(int shipmentId, int truckId, int driverId)
        {
            var shipment = _shipmentRepository.GetById(shipmentId);
            if (shipment == null) return null;
            if (shipment.Status != ShipmentStatus.Planned)
                throw new InvalidOperationException("Only planned shipments can be assigned");

            var truck = _truckRepository.GetById(truckId) ?? throw new InvalidOperationException("Truck not found");
            var driver = _driverRepository.GetById(driverId) ?? throw new InvalidOperationException("Driver not found");

            if (truck.Status != TruckStatus.Available) throw new InvalidOperationException("Truck is not available");
            if (!driver.Available) throw new InvalidOperationException("Driver is not available");
            if (shipment.Cargo.WeightTons > truck.CapacityTons) throw new InvalidOperationException("Cargo exceeds truck capacity");

            shipment.TruckId = truckId;
            shipment.DriverId = driverId;
            _shipmentRepository.Update(shipment);
            return shipment;
        }

        public Shipment StartShipment(int shipmentId)
        {
            var shipment = _shipmentRepository.GetById(shipmentId) ?? throw new InvalidOperationException("Shipment not found");
            if (shipment.Status != ShipmentStatus.Planned) throw new InvalidOperationException("Shipment is not in planned state");
            if (shipment.TruckId == null || shipment.DriverId == null) throw new InvalidOperationException("Assign truck and driver first");

            var truck = _truckRepository.GetById(shipment.TruckId.Value) ?? throw new InvalidOperationException("Truck not found");
            var driver = _driverRepository.GetById(shipment.DriverId.Value) ?? throw new InvalidOperationException("Driver not found");

            shipment.DepartureTime = DateTime.Now;
            shipment.Status = ShipmentStatus.InTransit;

            truck.Status = TruckStatus.OnRoute;
            driver.Available = false;
            _truckRepository.Update(truck);
            _driverRepository.Update(driver);
            _shipmentRepository.Update(shipment);
            return shipment;
        }

        public Shipment CompleteShipment(int shipmentId)
        {
            var shipment = _shipmentRepository.GetById(shipmentId) ?? throw new InvalidOperationException("Shipment not found");
            if (shipment.Status != ShipmentStatus.InTransit) throw new InvalidOperationException("Shipment is not in transit");

            shipment.ArrivalTime = DateTime.Now;
            shipment.Status = ShipmentStatus.Delivered;

            if (shipment.TruckId != null)
            {
                var truck = _truckRepository.GetById(shipment.TruckId.Value);
                if (truck != null)
                {
                    truck.Status = TruckStatus.Available;
                    _truckRepository.Update(truck);
                }
            }
            if (shipment.DriverId != null)
            {
                var driver = _driverRepository.GetById(shipment.DriverId.Value);
                if (driver != null)
                {
                    driver.Available = true;
                    _driverRepository.Update(driver);
                }
            }
            _shipmentRepository.Update(shipment);
            return shipment;
        }

        public IEnumerable<Shipment> GetShipmentsByPeriod(DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date;
            return _shipmentRepository.GetAll()
                .Where(s => s.PlannedDate.Date >= fromDate && s.PlannedDate.Date <= toDate)
                .OrderBy(s => s.PlannedDate)
                .ToList();
        }

        public IEnumerable<Truck> GetAvailableTrucks(double minCapacity)
        {
            return _truckRepository.GetAll()
                .Where(t => t.Status == TruckStatus.Available && t.CapacityTons >= minCapacity)
                .OrderBy(t => t.CapacityTons)
                .ToList();
        }

        public IEnumerable<Driver> GetAvailableDrivers()
        {
            return _driverRepository.GetAll()
                .Where(d => d.Available)
                .OrderBy(d => d.FullName)
                .ToList();
        }

        private static decimal CalculateCost(double distanceKm, double weightTons)
        {
            var distance = (decimal)distanceKm;
            var weight = (decimal)weightTons;
            var cost = distance * BaseRatePerKm + distance * weight * WeightRatePerTonPerKm;
            return Math.Round(cost, 2);
        }
    }
}
