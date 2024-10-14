using RabbitMQ.Client; // RabbitMQ istemci kütüphanesini içe aktarıyoruz

namespace RabbitMQ.Web.WatermarkApp.Services
{
    public class RabbitMQClientService : IDisposable // RabbitMQ ile etkileşim kuracak bir sınıf tanımlıyoruz
    {
        private readonly IConnection _connection; // RabbitMQ bağlantısını temsil eden bir alan
        private readonly IModel _model; // RabbitMQ modelini (kanalını) temsil eden bir alan
        private readonly ILogger<RabbitMQClientService> _logger; // Loglama için bir ILogger nesnesi

        // RabbitMQ ile bağlantı için gerekli bazı sabit değerler
        public static string ExchangeName = "ImageDirectExchange"; // Kullanılacak exchange adı
        public static string RoutingWatermark = "watermark-route-image"; // Routing key
        public static string QueueName = "queue-watermark-image"; // Kuyruk adı

        // Yapıcı metod (constructor)
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _logger = logger; // Logger'ı ayarlıyoruz

            // Bağlantıyı oluşturuyoruz
            _connection = connectionFactory.CreateConnection();
            // Yeni bir kanal (model) oluşturuyoruz
            _model = _connection.CreateModel();

            // Exchange ve kuyruk yapılandırmasını burada yapıyoruz
            _model.ExchangeDeclare(ExchangeName, type: "direct", durable: true, autoDelete: false); // Exchange'i tanımlıyoruz
            _model.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null); // Kuyruğu tanımlıyoruz
            _model.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark); // Exchange ve kuyruk arasında bağlama işlemi yapıyoruz

            _logger.LogInformation("RabbitMQ bağlantısı kuruldu."); // Bağlantı başarılı olduğunda log kaydı
        }

        // Kanalı döndüren bir metod
        public IModel GetModel() => _model; // Kanalı döndürmek için bir metod

        // IDisposable arayüzü implementasyonu
        public void Dispose() // RabbitMQ ile olan bağlantıları kapatmak için implement ettik
        {
            _model?.Close(); // Modeli kapat
            _model?.Dispose(); // Model kaynaklarını serbest bırak
            _connection?.Close(); // Bağlantıyı kapat
            _connection?.Dispose(); // Bağlantı kaynaklarını serbest bırak

            _logger.LogInformation("RabbitMQ bağlantısı kapatıldı."); // Bağlantı kapatıldığında log kaydı
        }
    }
}
