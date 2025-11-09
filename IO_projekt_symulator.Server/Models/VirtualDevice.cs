namespace IO_projekt_symulator.Server.Models
{
    public class VirtualDevice
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }

        // Elastyczny sposob przechowywania stanu urzadzenia.
        // Np. dla swiatla: { "power": "on", "brightness": 100 }
        // Np. dla czujnika: { "temperature": 21.5 }
        public Dictionary<string, object> State { get; set; }

        public VirtualDevice()
        {
            Id = Guid.NewGuid();
            State = new Dictionary<string, object>();
        }
    }
}
