namespace IO_projekt_symulator.Server.Contracts
{
    public class DeviceUpdatedEvent
    {
        public Guid DeviceId { get; set; }
        public double NewValue { get; set; }
    }
}
