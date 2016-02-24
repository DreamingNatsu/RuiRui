using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RuiRuiBot.Botplugins.Triggers {
    internal class RpcClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly QueueingBasicConsumer _consumer;

        public RpcClient()
        {
            var factory = new ConnectionFactory{ HostName = "dolha.in", UserName = "admin",Password = "admin"};
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(queue: _replyQueueName,
                noAck: true,
                consumer: _consumer);
        }

        public List<DateTime> Call(string message)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = corrId;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes);

            while (true)
            {
                var ea = _consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId != corrId) continue;
                var s = Encoding.UTF8.GetString(ea.Body);

                return JsonConvert.DeserializeObject<List<DateTime>>(s);


            }
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}