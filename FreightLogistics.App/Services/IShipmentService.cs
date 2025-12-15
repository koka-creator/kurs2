using System;
using System.Collections.Generic;
using FreightLogistics.App.Domain;

namespace FreightLogistics.App.Services
{
    public interface IShipmentService
    {
        Shipment CreateShipment(string cargoDescription, double cargoWeightTons, bool refrigerated, double distanceKm, DateTime plannedDate);
        Shipment? AssignResources(int shipmentId, int truckId, int driverId);
        Shipment StartShipment(int shipmentId);
        Shipment CompleteShipment(int shipmentId);
        IEnumerable<Shipment> GetShipmentsByPeriod(DateTime from, DateTime to);
        IEnumerable<Truck> GetAvailableTrucks(double minCapacity);
        IEnumerable<Driver> GetAvailableDrivers();
    }
}

