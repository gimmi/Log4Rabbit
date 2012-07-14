RabbitMQ appender for log4net
-----------------------------

The title says it all. Check [Log4Net site](http://logging.apache.org/log4net/) or [RabbitMQ site](http://www.rabbitmq.com/) if you need more info.

Installation
------------

[Get it on NuGet](http://nuget.org/packages/Log4Rabbit), or download sources and run build.cmd to build

Appender configuration sample
-----------------------------

```xml
<appender name="RabbitMQAppender" type="log4net.Appender.RabbitMQAppender, Log4Rabbit">
	<HostName value="localhost" /> <!-- Optional, default to RabbitMQ.Client.ConnectionFactory default -->
	<VirtualHost value="/" /> <!-- Optional, default to RabbitMQ.Client.ConnectionFactory default -->
	<UserName value="guest" /> <!-- Optional, default to RabbitMQ.Client.ConnectionFactory default -->
	<Password value="guest" /> <!-- Optional, default to RabbitMQ.Client.ConnectionFactory default -->
	<RequestedHeartbeat value="0" /> <!-- Optional, default to RabbitMQ.Client.ConnectionFactory default -->
	<Port value="5672" /> <!-- Optional, default to RabbitMQ.Client.ConnectionFactory default -->
	<Exchange value="logs" /> <!-- Optional, Default to logs -->
	<RoutingKey value="" /> <!-- Optional, Default to empty -->
</appender>
```

License
-------

[APACHE 2](https://raw.github.com/gimmi/Log4Rabbit/master/LICENSE)
