using System.Text;
using System.Text.Json;

namespace RabbitMQ.Web.WatermarkApp.Services
{
    public class RabbitMQPublisher  //Bu ise gönderen yani publisher yada producer 
    {
        private readonly RabbitMQClientService _rabbitmqClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitmqClientService)
        {
            _rabbitmqClientService = rabbitmqClientService;
        }

        public void Publish(ProductImageCreated productImageCreated) //gönderenin oluşturduğu mesajı eventi geçtik burada 
        {
            var channel = _rabbitmqClientService.GetModel(); //Burada bağlantı kurulu bir şekilde gelsin dedik 

            var bodyString = JsonSerializer.Serialize(productImageCreated);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();

            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWatermark, true, basicProperties: properties, body: bodyByte);

            //Mandatory özelliğini true yapma sebebimiz publisherdan çıkan mesaj kuyruğunu bulamazsa kaybolmasın bize geri gelsin diye true yaptık
        }
    }
}
