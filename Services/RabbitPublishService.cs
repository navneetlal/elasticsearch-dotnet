using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using RabbitMQ.Client;

using ElasticSearch.Models;
using ElasticSearch.Policies;

namespace ElasticSearch.Services
{

    public class RabbitManager : IRabbitManager  
    {  
        private readonly DefaultObjectPool<IModel> _objectPool;  
    
        public RabbitManager(IPooledObjectPolicy<IModel> objectPolicy)  
        {  
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);  
        }  
    
        public void Publish<T>(T message, string exchangeName, string exchangeType, string routeKey)   
            where T : class  
        {  
            if (message == null)  
                return;  
    
            var channel = _objectPool.Get();  
    
            try  
            {  
                channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);  
    
                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));  
    
                var properties = channel.CreateBasicProperties();  
                properties.Persistent = true;  
                channel.QueueDeclare("Boards", true, false, false, null);
                channel.QueueBind("Boards", exchangeName, routeKey);
    
                channel.BasicPublish(exchangeName, routeKey, properties, sendBytes);
            }  
            catch (Exception ex)  
            {  
                throw ex;  
            }  
            finally  
            {  
                _objectPool.Return(channel);                  
            }  
        }  
    } 

    public interface IRabbitManager  
    {  
        void Publish<T>(T message, string exchangeName, string exchangeType, string routeKey)   
            where T : class;  
    }

    public static class RabbitServiceCollectionExtensions  
    {  
        public static IServiceCollection AddRabbit(this IServiceCollection services, IConfiguration configuration)  
        {  
            var rabbitConfig = configuration.GetSection("rabbit");  
            services.Configure<RabbitOptions>(rabbitConfig);  
    
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();  
            services.AddSingleton<IPooledObjectPolicy<IModel>, RabbitModelPooledObjectPolicy>();  
    
            services.AddSingleton<IRabbitManager, RabbitManager>();  
    
            return services;  
        }  
    }

}