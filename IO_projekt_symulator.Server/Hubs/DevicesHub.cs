using Microsoft.AspNetCore.SignalR;

namespace IO_projekt_symulator.Server.Hubs
{
    /// <summary>
    /// SignalR Hub responsible for real-time communication with connected clients (Frontend).
    /// Used primarily to broadcast server-to-client notifications about device state changes.
    /// </summary>
    public class DevicesHub : Hub
    {
        // Currently empty as the communication is unidirectional (Server -> Client notifications).
        // Clients receive updates via "UpdateReceived", "MalfunctionUpdate", etc.
    }
}