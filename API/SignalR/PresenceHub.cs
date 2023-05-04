using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

namespace API.SignalR
{
    [Authorize] //we only want authorized user to access this specific hub
   // using websocket, but we don't have a access to the http headers without using http request, we need 
        //to do the authentication in a different way =>token in the param not in the header, go to IdentityServiceExtension jwtBearer
    public class PresenceHub : Hub//get who is present
    {
        private readonly PresenceTracker _tracker;
        private readonly IUnitOfWork _uow;

        public PresenceHub(PresenceTracker tracker, IUnitOfWork uow)
        {
            _uow = uow;
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(),Context.ConnectionId);
            if(isOnline)
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());//hub context get access to user claims principle
            
            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers",currentUsers);
            var UnreadNum = await _uow.MessageRepository.GetUnreadMessagesNumber(Context.User.GetUsername());
            await Clients.Caller.SendAsync("GetUnreadMessagesNumber",UnreadNum);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(),Context.ConnectionId);
            if(isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            await base.OnDisconnectedAsync(exception);
        }
        
    }
}
