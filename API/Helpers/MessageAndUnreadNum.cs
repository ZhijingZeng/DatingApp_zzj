using API.Interfaces;

namespace API.DTOs
{
    public class MessageAndUnreadNum
    {
        public List<MessageDto> messageDtos { get; set; }
        public int unreadNum { get; set; }
    }
}