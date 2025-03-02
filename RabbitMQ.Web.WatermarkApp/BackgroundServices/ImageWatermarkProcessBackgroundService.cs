﻿
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Logging;
using RabbitMQ.Web.WatermarkApp.Services;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Web.WatermarkApp.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitmqClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitmqClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitmqClientService = rabbitmqClientService;
            _logger = logger;
        }



        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitmqClientService.GetModel();
            _channel.BasicQos(0, 1, false);
            return base.StartAsync(cancellationToken);

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
             Task.Delay(5000).Wait();

            try
            {


                var siteName = "www.mysite.com";

                var productImageCreated = JsonSerializer.Deserialize<ProductImageCreated>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreated.ImageName);

                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);
                var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString("www.mysite.com", font);

                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);
                var posetion = new Point(img.Width - (int)textSize.Width + 30, img.Height - (int)textSize.Height + 30);

                graphic.DrawString(siteName, font, brush, posetion);

                img.Save("wwwroot/Images/watermarks/" + productImageCreated.ImageName);

                img.Dispose();
                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return ;

        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
