namespace Broker;
/*
 * "username": "guest",
   "password": "guest",
   "virtualHost": "/",
   "hostname": "localhost",
   "port": 5672
 */
public class RabbitMqOptions
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? VirtualHost { get; set; }
    public string? Hostname { get; set; }
    public int Port { get; set; }
}