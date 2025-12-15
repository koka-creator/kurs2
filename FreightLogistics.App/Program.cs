using System;
using System.Globalization;
using System.Windows.Forms;
using FreightLogistics.App.Domain;
using FreightLogistics.App.Repositories;
using FreightLogistics.App.Services;
using FreightLogistics.App.UI;

namespace FreightLogistics.App
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var truckRepo = new TruckRepository();
            var driverRepo = new DriverRepository();
            var shipmentRepo = new ShipmentRepository();
            var shipmentService = new ShipmentService(truckRepo, driverRepo, shipmentRepo);

            DataStorage.Load(truckRepo, driverRepo, shipmentRepo);
            
            if (!truckRepo.GetAll().Any())
            {
                Seed(truckRepo, driverRepo, shipmentService);
            }

            Application.Run(new MainForm(truckRepo, driverRepo, shipmentRepo, shipmentService));
        }

        private static void Seed(
            ITruckRepository truckRepo,
            IDriverRepository driverRepo,
            IShipmentService shipmentService)
        {
            truckRepo.Add(new Truck { RegistrationNumber = "AA1001-BC", CapacityTons = 10, FuelConsumptionPer100Km = 24 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1002-BC", CapacityTons = 20, FuelConsumptionPer100Km = 28 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1003-BC", CapacityTons = 5, FuelConsumptionPer100Km = 18, Status = TruckStatus.Maintenance });

            driverRepo.Add(new Driver { FullName = "Ivan Petrov", LicenseNumber = "DRV-001" });
            driverRepo.Add(new Driver { FullName = "Petr Ivanov", LicenseNumber = "DRV-002" });
            driverRepo.Add(new Driver { FullName = "Sergey Sidorov", LicenseNumber = "DRV-003", Available = false });

            shipmentService.CreateShipment("Steel coils", 8.5, false, 320, DateTime.Today);
            shipmentService.CreateShipment("Frozen fish", 6.0, true, 150, DateTime.Today.AddDays(1));
        }
    }
}
