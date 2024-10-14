namespace RabbitMQ.Web.WatermarkApp.Services
{
    public class ProductImageCreated //Rabbitmq da göndereceğimiz enent classı oluşturduk  
    {
        //Burası publisher ın içeriği gönderenin ne gönderdiği
        public string ImageName { get; set; }
    }
}
