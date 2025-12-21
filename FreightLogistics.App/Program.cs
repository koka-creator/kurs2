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
            // Грузовики (10 штук)
            truckRepo.Add(new Truck { RegistrationNumber = "AA1001-BC", CapacityTons = 10, FuelConsumptionPer100Km = 24 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1002-BC", CapacityTons = 20, FuelConsumptionPer100Km = 28 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1003-BC", CapacityTons = 15, FuelConsumptionPer100Km = 22 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1004-BC", CapacityTons = 25, FuelConsumptionPer100Km = 30 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1005-BC", CapacityTons = 8, FuelConsumptionPer100Km = 20 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1006-BC", CapacityTons = 12, FuelConsumptionPer100Km = 25 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1007-BC", CapacityTons = 18, FuelConsumptionPer100Km = 27 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1008-BC", CapacityTons = 5, FuelConsumptionPer100Km = 18, Status = TruckStatus.Maintenance });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1009-BC", CapacityTons = 30, FuelConsumptionPer100Km = 32 });
            truckRepo.Add(new Truck { RegistrationNumber = "AA1010-BC", CapacityTons = 22, FuelConsumptionPer100Km = 29 });

            // Водители (10 штук)
            driverRepo.Add(new Driver { FullName = "Иван Петров", LicenseNumber = "DRV-001" });
            driverRepo.Add(new Driver { FullName = "Петр Иванов", LicenseNumber = "DRV-002" });
            driverRepo.Add(new Driver { FullName = "Сергей Сидоров", LicenseNumber = "DRV-003", Available = false });
            driverRepo.Add(new Driver { FullName = "Алексей Смирнов", LicenseNumber = "DRV-004" });
            driverRepo.Add(new Driver { FullName = "Дмитрий Козлов", LicenseNumber = "DRV-005" });
            driverRepo.Add(new Driver { FullName = "Михаил Новиков", LicenseNumber = "DRV-006" });
            driverRepo.Add(new Driver { FullName = "Андрей Морозов", LicenseNumber = "DRV-007" });
            driverRepo.Add(new Driver { FullName = "Владимир Павлов", LicenseNumber = "DRV-008" });
            driverRepo.Add(new Driver { FullName = "Николай Волков", LicenseNumber = "DRV-009", Available = false });
            driverRepo.Add(new Driver { FullName = "Олег Соколов", LicenseNumber = "DRV-010" });

            // Рейсы (10 штук)
            shipmentService.CreateShipment("Стальные катушки", 8.5, false, 320, DateTime.Today);
            shipmentService.CreateShipment("Замороженная рыба", 6.0, true, 150, DateTime.Today.AddDays(1));
            shipmentService.CreateShipment("Деревянные доски", 12.0, false, 450, DateTime.Today.AddDays(2));
            shipmentService.CreateShipment("Медицинское оборудование", 3.5, false, 280, DateTime.Today.AddDays(-1));
            shipmentService.CreateShipment("Овощи и фрукты", 9.0, true, 200, DateTime.Today.AddDays(3));
            shipmentService.CreateShipment("Строительные материалы", 15.0, false, 380, DateTime.Today.AddDays(4));
            shipmentService.CreateShipment("Электроника", 4.5, false, 520, DateTime.Today.AddDays(-2));
            shipmentService.CreateShipment("Химические вещества", 7.0, false, 300, DateTime.Today.AddDays(5));
            shipmentService.CreateShipment("Одежда", 5.5, false, 250, DateTime.Today.AddDays(6));
            shipmentService.CreateShipment("Мебель", 11.0, false, 400, DateTime.Today.AddDays(7));
        }
    }
}
