namespace IO_projekt_symulator.Server.Contracts
{
    // Ta klasa definiuje, co Panel może do Ciebie wysłać
    public class SetDeviceStateCommand
    {
        public Guid DeviceId { get; set; }
        public double Value { get; set; }
        // Opcjonalnie możesz dodać Unit, jeśli ustalicie, że Panel może zmieniać jednostki
    }
}