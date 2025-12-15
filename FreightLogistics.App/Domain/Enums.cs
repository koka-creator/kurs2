using System;

namespace FreightLogistics.App.Domain
{
    public enum ShipmentStatus
    {
        Planned = 0,
        InTransit = 1,
        Delivered = 2,
        Cancelled = 3
    }

    public enum TruckStatus
    {
        Available = 0,
        OnRoute = 1,
        Maintenance = 2
    }
}
