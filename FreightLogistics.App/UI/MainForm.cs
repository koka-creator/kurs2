using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using FreightLogistics.App.Domain;
using FreightLogistics.App.Repositories;
using FreightLogistics.App.Services;

namespace FreightLogistics.App.UI
{
    public class MainForm : Form
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IShipmentService _shipmentService;

        private ListBox _listBox = null!;
        private Label _titleLabel = null!;
        private Button _btnTrucks = null!;
        private Button _btnDrivers = null!;
        private Button _btnShipments = null!;
        private Button _btnCreateShipment = null!;
        private Button _btnAssign = null!;
        private Button _btnStart = null!;
        private Button _btnComplete = null!;
        private Button _btnReport = null!;

        public MainForm(
            ITruckRepository truckRepository,
            IDriverRepository driverRepository,
            IShipmentRepository shipmentRepository,
            IShipmentService shipmentService)
        {
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _shipmentRepository = shipmentRepository;
            _shipmentService = shipmentService;

            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "Логистика грузоперевозок";
            Width = 1200;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            _titleLabel = new Label
            {
                Text = "Управление логистикой грузоперевозок",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold)
            };

            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                Padding = new Padding(10)
            };

            _btnTrucks = CreateMenuButton("Грузовики", (s, e) => ShowTrucks());
            var btnAddTruck = CreateMenuButton("+ Добавить грузовик", (s, e) => AddTruck());
            var btnDeleteTruck = CreateMenuButton("- Удалить грузовик", (s, e) => DeleteTruck());
            var btnChangeTruckStatus = CreateMenuButton("Изменить статус грузовика", (s, e) => ChangeTruckStatus());
            
            _btnDrivers = CreateMenuButton("Водители", (s, e) => ShowDrivers());
            var btnAddDriver = CreateMenuButton("+ Добавить водителя", (s, e) => AddDriver());
            var btnDeleteDriver = CreateMenuButton("- Удалить водителя", (s, e) => DeleteDriver());
            var btnChangeDriverStatus = CreateMenuButton("Изменить статус водителя", (s, e) => ChangeDriverStatus());
            
            _btnShipments = CreateMenuButton("Рейсы", (s, e) => ShowShipments());
            _btnCreateShipment = CreateMenuButton("Создать рейс", (s, e) => CreateShipment());
            _btnAssign = CreateMenuButton("Назначить ресурсы", (s, e) => AssignResources());
            _btnStart = CreateMenuButton("Начать рейс", (s, e) => StartShipment());
            _btnComplete = CreateMenuButton("Завершить рейс", (s, e) => CompleteShipment());
            _btnReport = CreateMenuButton("Отчёт по периоду", (s, e) => ReportShipments());

            var buttonsLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            buttonsLayout.Controls.AddRange(new Control[]
            {
                _btnTrucks,
                btnAddTruck,
                btnDeleteTruck,
                btnChangeTruckStatus,
                new Label { Height = 10 },
                _btnDrivers,
                btnAddDriver,
                btnDeleteDriver,
                btnChangeDriverStatus,
                new Label { Height = 10 },
                _btnShipments,
                _btnCreateShipment,
                _btnAssign,
                _btnStart,
                _btnComplete,
                new Label { Height = 10 },
                _btnReport
            });

            leftPanel.Controls.Add(buttonsLayout);

            _listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 9),
                HorizontalScrollbar = true,
                ScrollAlwaysVisible = true,
                IntegralHeight = false
            };

            Controls.Add(_listBox);
            Controls.Add(leftPanel);
            Controls.Add(_titleLabel);

            Load += (_, _) => ShowShipments();
            FormClosing += (_, e) =>
            {
                DataStorage.Save(_truckRepository, _driverRepository, _shipmentRepository);
            };
        }

        private static Button CreateMenuButton(string text, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                Width = 220,
                Height = 35,
                Margin = new Padding(3),
                Font = new System.Drawing.Font("Segoe UI", 9),
                AutoSize = false
            };
            button.Click += onClick;
            return button;
        }

        private void ShowTrucks()
        {
            _listBox.Items.Clear();
            foreach (var t in _truckRepository.GetAll())
            {
                var status = t.Status == TruckStatus.Available ? "Доступен" : 
                             t.Status == TruckStatus.OnRoute ? "В пути" : "На обслуживании";
                _listBox.Items.Add($"{t.Id}  {t.RegistrationNumber}  {t.CapacityTons} т  {status}");
            }
        }

        private void ShowDrivers()
        {
            _listBox.Items.Clear();
            foreach (var d in _driverRepository.GetAll())
            {
                var status = d.Available ? "Доступен" : "Занят";
                _listBox.Items.Add($"{d.Id}  {d.FullName}  {status}");
            }
        }

        private void ShowShipments()
        {
            _listBox.Items.Clear();
            foreach (var s in _shipmentRepository.GetAll())
            {
                var status = s.Status == ShipmentStatus.Planned ? "Запланирован" :
                             s.Status == ShipmentStatus.InTransit ? "В пути" :
                             s.Status == ShipmentStatus.Delivered ? "Доставлен" : "Отменён";
                var assigned = s.TruckId != null && s.DriverId != null ? "Назначено" : "Не назначено";
                var line = $"{s.Id}  {s.Cargo.WeightTons} т  {s.DistanceKm} км  {status}  {assigned}  {s.Cost:N2} руб.";
                _listBox.Items.Add(line);
            }
        }

        private void CreateShipment()
        {
            try
            {
                var descr = Prompt("Создать рейс", "Описание груза:");
                if (string.IsNullOrWhiteSpace(descr)) return;

                var weightStr = Prompt("Создать рейс", "Вес (тонны):");
                if (!double.TryParse(weightStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight) || weight <= 0)
                {
                    MessageBox.Show("Неверный вес.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var refrigeratedStr = Prompt("Создать рейс", "Требуется охлаждение? (д/н):");
                var refrigerated = (refrigeratedStr ?? string.Empty).Trim().ToLowerInvariant() == "д";

                var distanceStr = Prompt("Создать рейс", "Расстояние (км):");
                if (!double.TryParse(distanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var distance) || distance <= 0)
                {
                    MessageBox.Show("Неверное расстояние.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var dateStr = Prompt("Создать рейс", "Планируемая дата (гггг-ММ-дд):");
                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var plannedDate))
                {
                    plannedDate = DateTime.Today;
                }

                var shipment = _shipmentService.CreateShipment(descr, weight, refrigerated, distance, plannedDate);
                MessageBox.Show($"Рейс создан: ID {shipment.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowShipments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssignResources()
        {
            try
            {
                ShowShipments();
                var shipmentIdStr = Prompt("Назначить ресурсы", "ID рейса:");
                if (!int.TryParse(shipmentIdStr, out var shipmentId))
                {
                    MessageBox.Show("Неверный ID рейса.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var availableTrucks = _shipmentService.GetAvailableTrucks(0).ToList();
                if (!availableTrucks.Any())
                {
                    MessageBox.Show("Нет доступных грузовиков.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var availableDrivers = _shipmentService.GetAvailableDrivers().ToList();
                if (!availableDrivers.Any())
                {
                    MessageBox.Show("Нет доступных водителей.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var truckIdStr = PromptWithList("Назначить ресурсы", "ID грузовика:", 
                    string.Join(Environment.NewLine, availableTrucks.Select(t => $"{t.Id}  {t.RegistrationNumber}")));
                if (!int.TryParse(truckIdStr, out var truckId))
                {
                    MessageBox.Show("Неверный ID грузовика.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var driverIdStr = PromptWithList("Назначить ресурсы", "ID водителя:", 
                    string.Join(Environment.NewLine, availableDrivers.Select(d => $"{d.Id}  {d.FullName}")));
                if (!int.TryParse(driverIdStr, out var driverId))
                {
                    MessageBox.Show("Неверный ID водителя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = _shipmentService.AssignResources(shipmentId, truckId, driverId);
                if (result == null)
                {
                    MessageBox.Show("Рейс не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ресурсы назначены: Рейс {result.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ShowShipments();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartShipment()
        {
            try
            {
                ShowShipments();
                var shipmentIdStr = Prompt("Начать рейс", "ID рейса:");
                if (!int.TryParse(shipmentIdStr, out var shipmentId))
                {
                    MessageBox.Show("Неверный ID рейса.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var shipment = _shipmentService.StartShipment(shipmentId);
                MessageBox.Show($"Рейс начат: ID {shipment.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowShipments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CompleteShipment()
        {
            try
            {
                ShowShipments();
                var shipmentIdStr = Prompt("Завершить рейс", "ID рейса:");
                if (!int.TryParse(shipmentIdStr, out var shipmentId))
                {
                    MessageBox.Show("Неверный ID рейса.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var shipment = _shipmentService.CompleteShipment(shipmentId);
                MessageBox.Show($"Рейс завершён: ID {shipment.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowShipments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReportShipments()
        {
            try
            {
                var allShipments = _shipmentRepository.GetAll().ToList();
                if (!allShipments.Any())
                {
                    MessageBox.Show("Нет рейсов для отчёта.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var minDate = allShipments.Min(s => s.PlannedDate);
                var maxDate = allShipments.Max(s => s.PlannedDate);
                var availableDates = allShipments.Select(s => s.PlannedDate.ToString("yyyy-MM-dd")).Distinct().OrderBy(d => d).ToList();

                var dateRange = $"Доступные даты: {minDate:yyyy-MM-dd} .. {maxDate:yyyy-MM-dd}\n\n" +
                               $"Даты с рейсами:\n{string.Join("\n", availableDates)}";

                var fromDate = PromptDate("Отчёт - выбор периода", $"С (гггг-ММ-дд):\n\n{dateRange}", minDate);
                if (!fromDate.HasValue) return;

                var toDate = PromptDate("Отчёт - выбор периода", $"По (гггг-ММ-дд):\n\n{dateRange}", maxDate);
                if (!toDate.HasValue) return;

                var from = fromDate.Value;
                var to = toDate.Value;

                if (from > to)
                {
                    MessageBox.Show("Дата 'С' не может быть больше даты 'По'.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var list = _shipmentService.GetShipmentsByPeriod(from, to).ToList();

                _listBox.Items.Clear();
                _listBox.Items.Add($"Отчёт за период: {from:yyyy-MM-dd} .. {to:yyyy-MM-dd}");
                _listBox.Items.Add("----------------------------------------");
                
                if (list.Any())
                {
                    foreach (var s in list)
                    {
                        var status = s.Status == ShipmentStatus.Planned ? "Запланирован" :
                                     s.Status == ShipmentStatus.InTransit ? "В пути" :
                                     s.Status == ShipmentStatus.Delivered ? "Доставлен" : "Отменён";
                        var assigned = s.TruckId != null && s.DriverId != null ? "Назначено" : "Не назначено";
                        var line = $"{s.Id}  {s.Cargo.WeightTons} т  {s.DistanceKm} км  {status}  {assigned}  {s.Cost:N2} руб.  Дата: {s.PlannedDate:yyyy-MM-dd}";
                        _listBox.Items.Add(line);
                    }
                    _listBox.Items.Add($"----------------------------------------");
                    _listBox.Items.Add($"Найдено рейсов: {list.Count}");
                }
                else
                {
                    _listBox.Items.Add("Нет рейсов в этом периоде.");
                    _listBox.Items.Add($"Всего рейсов в системе: {allShipments.Count}");
                    foreach (var s in allShipments.Take(5))
                    {
                        _listBox.Items.Add($"  Рейс {s.Id}: дата {s.PlannedDate:yyyy-MM-dd}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DateTime? PromptDate(string title, string label, DateTime defaultValue)
        {
            using var form = new Form
            {
                Text = title,
                Width = 550,
                Height = 350,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var lbl = new Label
            {
                Text = label,
                Left = 10,
                Top = 10,
                Width = 520,
                Height = 120,
                AutoSize = false
            };

            var datePicker = new DateTimePicker
            {
                Left = 10,
                Top = 140,
                Width = 520,
                Format = DateTimePickerFormat.Short,
                Value = defaultValue
            };

            var buttonOk = new Button
            {
                Text = "ОК",
                DialogResult = DialogResult.OK,
                Left = 360,
                Width = 75,
                Top = 180
            };

            var buttonCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Left = 445,
                Width = 75,
                Top = 180
            };

            form.Controls.Add(lbl);
            form.Controls.Add(datePicker);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);

            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            return form.ShowDialog() == DialogResult.OK ? datePicker.Value.Date : null;
        }

        private static string? Prompt(string title, string label)
        {
            using var form = new Form
            {
                Text = title,
                Width = 500,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var lbl = new Label
            {
                Text = label,
                Left = 10,
                Top = 10,
                Width = 460,
                Height = 50
            };

            var textBox = new TextBox
            {
                Left = 10,
                Top = 70,
                Width = 460
            };

            var buttonOk = new Button
            {
                Text = "ОК",
                DialogResult = DialogResult.OK,
                Left = 310,
                Width = 75,
                Top = 110
            };

            var buttonCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Left = 395,
                Width = 75,
                Top = 110
            };

            form.Controls.Add(lbl);
            form.Controls.Add(textBox);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);

            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        private static string? PromptWithList(string title, string label, string listText)
        {
            using var form = new Form
            {
                Text = title,
                Width = 650,
                Height = 450,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var lbl = new Label
            {
                Text = label,
                Left = 10,
                Top = 10,
                Width = 610
            };

            var infoBox = new TextBox
            {
                Text = listText,
                Left = 10,
                Top = 35,
                Width = 610,
                Height = 280,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.SystemColors.Control
            };

            var lblInput = new Label
            {
                Text = "Введите ID:",
                Left = 10,
                Top = 325,
                Width = 100
            };

            var textBox = new TextBox
            {
                Left = 10,
                Top = 345,
                Width = 610
            };

            var buttonOk = new Button
            {
                Text = "ОК",
                DialogResult = DialogResult.OK,
                Left = 460,
                Width = 75,
                Top = 380
            };

            var buttonCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Left = 545,
                Width = 75,
                Top = 380
            };

            form.Controls.Add(lbl);
            form.Controls.Add(infoBox);
            form.Controls.Add(lblInput);
            form.Controls.Add(textBox);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);

            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        private void AddTruck()
        {
            try
            {
                var regNumber = Prompt("Добавить грузовик", "Регистрационный номер:");
                if (string.IsNullOrWhiteSpace(regNumber)) return;

                var capacityStr = Prompt("Добавить грузовик", "Грузоподъёмность (тонны):");
                if (!double.TryParse(capacityStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var capacity) || capacity <= 0)
                {
                    MessageBox.Show("Неверная грузоподъёмность.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var fuelStr = Prompt("Добавить грузовик", "Расход топлива (л/100км):");
                if (!double.TryParse(fuelStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var fuel) || fuel <= 0)
                {
                    MessageBox.Show("Неверный расход топлива.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var truck = new Truck
                {
                    RegistrationNumber = regNumber,
                    CapacityTons = capacity,
                    FuelConsumptionPer100Km = fuel,
                    Status = TruckStatus.Available
                };
                _truckRepository.Add(truck);
                MessageBox.Show($"Грузовик добавлен: ID {truck.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowTrucks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteTruck()
        {
            try
            {
                ShowTrucks();
                var idStr = Prompt("Удалить грузовик", "ID грузовика:");
                if (!int.TryParse(idStr, out var id))
                {
                    MessageBox.Show("Неверный ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var truck = _truckRepository.GetById(id);
                if (truck == null)
                {
                    MessageBox.Show("Грузовик не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _truckRepository.Delete(id);
                MessageBox.Show("Грузовик удалён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowTrucks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeTruckStatus()
        {
            try
            {
                ShowTrucks();
                var idStr = Prompt("Изменить статус грузовика", "ID грузовика:");
                if (!int.TryParse(idStr, out var id))
                {
                    MessageBox.Show("Неверный ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var truck = _truckRepository.GetById(id);
                if (truck == null)
                {
                    MessageBox.Show("Грузовик не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var statusStr = Prompt("Изменить статус", "Статус (0-Доступен, 1-В пути, 2-На обслуживании):");
                if (!int.TryParse(statusStr, out var statusInt) || statusInt < 0 || statusInt > 2)
                {
                    MessageBox.Show("Неверный статус.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                truck.Status = (TruckStatus)statusInt;
                _truckRepository.Update(truck);
                MessageBox.Show("Статус изменён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowTrucks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddDriver()
        {
            try
            {
                var name = Prompt("Добавить водителя", "ФИО:");
                if (string.IsNullOrWhiteSpace(name)) return;

                var license = Prompt("Добавить водителя", "Номер удостоверения:");
                if (string.IsNullOrWhiteSpace(license)) return;

                var driver = new Driver
                {
                    FullName = name,
                    LicenseNumber = license,
                    Available = true
                };
                _driverRepository.Add(driver);
                MessageBox.Show($"Водитель добавлен: ID {driver.Id}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowDrivers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteDriver()
        {
            try
            {
                ShowDrivers();
                var idStr = Prompt("Удалить водителя", "ID водителя:");
                if (!int.TryParse(idStr, out var id))
                {
                    MessageBox.Show("Неверный ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var driver = _driverRepository.GetById(id);
                if (driver == null)
                {
                    MessageBox.Show("Водитель не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _driverRepository.Delete(id);
                MessageBox.Show("Водитель удалён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowDrivers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeDriverStatus()
        {
            try
            {
                ShowDrivers();
                var idStr = Prompt("Изменить статус водителя", "ID водителя:");
                if (!int.TryParse(idStr, out var id))
                {
                    MessageBox.Show("Неверный ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var driver = _driverRepository.GetById(id);
                if (driver == null)
                {
                    MessageBox.Show("Водитель не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var statusStr = Prompt("Изменить статус", "Статус (д-Доступен, з-Занят):");
                driver.Available = (statusStr ?? "").Trim().ToLowerInvariant() == "д";
                _driverRepository.Update(driver);
                MessageBox.Show("Статус изменён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowDrivers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


