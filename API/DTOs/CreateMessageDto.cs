namespace API.DTOs
{
    public class CreateMessageDto
    {
        public string RecipientUsername { get; set; } //api will bind to this correnctly as it understands that 
        public string Content { get; set; }
    }
}