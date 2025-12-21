using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FreightLogistics.App.Domain;

namespace FreightLogistics.App.Repositories
{
    public class DataStorage
    {
        private const string DataFile = "data.txt";

        public static void Save(
            ITruckRepository truckRepo,
            IDriverRepository driverRepo,
            IShipmentRepository shipmentRepo)
        {
            using var writer = new StreamWriter(DataFile);

            writer.WriteLine("[ГРУЗОВИКИ]");
            foreach (var t in truckRepo.GetAll())
            {
                writer.WriteLine($"{t.Id},{t.RegistrationNumber},{t.CapacityTons},{t.FuelConsumptionPer100Km},{(int)t.Status}");
            }

            writer.WriteLine("[ВОДИТЕЛИ]");
            foreach (var d in driverRepo.GetAll())
            {
                writer.WriteLine($"{d.Id},{d.FullName},,{d.Available}");
            }

            writer.WriteLine("[РЕЙСЫ]");
            foreach (var s in shipmentRepo.GetAll())
            {
                var truckId = s.TruckId?.ToString() ?? "";
                var driverId = s.DriverId?.ToString() ?? "";
                var departure = s.DepartureTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                var arrival = s.ArrivalTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                writer.WriteLine($"{s.Id},,{s.Cargo.Description},{s.Cargo.WeightTons},{s.Cargo.RequiresRefrigeration},{s.DistanceKm},{s.PlannedDate:yyyy-MM-dd},{truckId},{driverId},{departure},{arrival},{(int)s.Status},{s.Cost}");
            }
        }

        public static void Load(
            TruckRepository truckRepo,
            DriverRepository driverRepo,
            ShipmentRepository shipmentRepo)
        {
            if (!File.Exists(DataFile)) return;

            try
            {
                var lines = File.ReadAllLines(DataFile);
                var trucks = new List<Truck>();
                var drivers = new List<Driver>();
                var shipments = new List<Shipment>();
                string currentSection = "";

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line == "[ГРУЗОВИКИ]")
                    {
                        currentSection = "Trucks";
                        continue;
                    }
                    if (line == "[ВОДИТЕЛИ]")
                    {
                        currentSection = "Drivers";
                        continue;
                    }
                    if (line == "[РЕЙСЫ]")
                    {
                        currentSection = "Shipments";
                        continue;
                    }

                    var parts = line.Split(',');
                    if (parts.Length == 0) continue;

                    if (currentSection == "Trucks" && parts.Length >= 5)
                    {
                        trucks.Add(new Truck
                        {
                            Id = int.Parse(parts[0]),
                            RegistrationNumber = parts[1],
                            CapacityTons = double.Parse(parts[2]),
                            FuelConsumptionPer100Km = double.Parse(parts[3]),
                            Status = (TruckStatus)int.Parse(parts[4])
                        });
                    }
                    else if (currentSection == "Drivers" && parts.Length >= 3)
                    {
                        drivers.Add(new Driver
                        {
                            Id = int.Parse(parts[0]),
                            FullName = parts[1],
                            LicenseNumber = "",
                            Available = parts.Length >= 4 ? bool.Parse(parts[3]) : true
                        });
                    }
                    else if (currentSection == "Shipments" && parts.Length >= 12)
                    {
                        shipments.Add(new Shipment
                        {
                            Id = int.Parse(parts[0]),
                            OrderNumber = "",
                            Cargo = new Cargo
                            {
                                Description = parts[2],
                                WeightTons = double.Parse(parts[3]),
                                RequiresRefrigeration = bool.Parse(parts[4])
                            },
                            DistanceKm = double.Parse(parts[5]),
                            PlannedDate = DateTime.Parse(parts[6]),
                            TruckId = string.IsNullOrEmpty(parts[7]) ? null : int.Parse(parts[7]),
                            DriverId = string.IsNullOrEmpty(parts[8]) ? null : int.Parse(parts[8]),
                            DepartureTime = string.IsNullOrEmpty(parts[9]) ? null : DateTime.Parse(parts[9]),
                            ArrivalTime = string.IsNullOrEmpty(parts[10]) ? null : DateTime.Parse(parts[10]),
                            Status = (ShipmentStatus)int.Parse(parts[11]),
                            Cost = decimal.Parse(parts[12])
                        });
                    }
                }

                var maxTruckId = trucks.Any() ? trucks.Max(t => t.Id) : 0;
                var maxDriverId = drivers.Any() ? drivers.Max(d => d.Id) : 0;
                var maxShipmentId = shipments.Any() ? shipments.Max(s => s.Id) : 0;

                if (trucks.Any()) truckRepo.LoadData(trucks, maxTruckId);
                if (drivers.Any()) driverRepo.LoadData(drivers, maxDriverId);
                if (shipments.Any()) shipmentRepo.LoadData(shipments, maxShipmentId);
            }
            catch
            {
                // Игнорируем ошибки загрузки
            }
        }
    }
}

