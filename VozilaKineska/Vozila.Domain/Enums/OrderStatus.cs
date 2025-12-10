namespace Vozila.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,   // Order created
        Approved,  // Transporter submitted TruckPlateNo
        Finished,  // DispatchNote matched
        Cancelled  // By admin or auto-expired
    }
}
