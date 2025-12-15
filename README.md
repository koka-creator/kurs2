# Freight Logistics Management System

Windows Forms application for freight logistics management in C#.

## Features

- Truck management (add, delete, change status)
- Driver management (add, delete, change status)
- Shipment creation and management
- Assign trucks and drivers to shipments
- Reports by period
- Automatic data saving to file

## Technologies

- .NET 8.0
- Windows Forms
- C#

## Getting Started

1. Open the project in Visual Studio
2. Press F5 to run

## First Launch

On first launch, the application will automatically create sample data:
- 3 trucks
- 3 drivers
- 2 sample shipments

You can start using the application immediately or clear the data and add your own.

## Data Storage

Data is automatically saved to `data.txt` file when the application closes.
The file is excluded from Git repository (see .gitignore) to keep personal data private.

