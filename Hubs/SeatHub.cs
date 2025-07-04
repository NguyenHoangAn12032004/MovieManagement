using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MovieManagement.Hubs
{
    public class SeatHub : Hub
    {
        public async Task UpdateSeatStatus(int seatId, string row, int number, string status, bool isAvailable)
        {
            await Clients.All.SendAsync("ReceiveSeatUpdate", seatId, row, number, status, isAvailable);
        }
    }
} 