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
	<HostName value="localhost"/> <!-- Default to localhost -->
	<VirtualHost value="/"/> <!-- Default to / -->
	<UserName value="guest"/> <!-- Default to guest -->
	<Password value="guest"/> <!-- Default to guest -->
	<RequestedHeartbeat value="0"/> <!-- Value in seconds, default to 0 that mean no heartbeat -->
	<Port value="5672"/> <!-- Default to 5672 -->
	<Exchange value="logs"/> <!-- Default to logs -->
	<RoutingKey value=""/> <!-- Default to empty -->
	<FlushInterval value="5"/> <!-- Seconds to wait between message send. Default to 5 seconds -->
	<MaxBufferSize value="10000"/> <!-- The maximum size of the buffer used to hold the logging events. Whan this size is reached logs are discarded. Default to 10.000 -->
</appender>
```

License
-------

[APACHE 2](https://raw.github.com/gimmi/Log4Rabbit/master/LICENSE)
