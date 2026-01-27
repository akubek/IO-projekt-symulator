namespace IO_projekt_symulator.Server.Contracts
{
    /// <summary>
    /// Represents a command sent by external systems (e.g. Control Panel) via message bus 
    /// to request a device state change.
    /// </summary>
    public class SetDeviceStateCommand
    {
        public Guid DeviceId { get; set; }
        public double Value { get; set; }
    }
}