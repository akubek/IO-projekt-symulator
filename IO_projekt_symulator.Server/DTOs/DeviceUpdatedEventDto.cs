namespace IO_projekt_symulator.Server.DTOs
{
    public sealed class DeviceUpdatedEventDto
    {
        public Guid DeviceId { get; init; }
        public double? Value { get; init; }
        public string? Unit { get; init; }
        public bool? Malfunctioning { get; init; }
    }
}