RabbitMQ appender for log4net
-----------------------------

The title says it all. Check [Log4Net site](http://logging.apache.org/log4net/) or [RabbitMQ site](http://www.rabbitmq.com/) if you need more info.

Installation
------------

[Get it on NuGet](http://nuget.org/packages/Log4Rabbit), or download sources and run build.cmd to build

Appender configuration sample
-----------------------------

	<appender name='RabbitMQAppender' type='log4net.Appender.RabbitMQAppender, Log4Rabbit'>
		<HostName value='localhost'/> <!-- Default to localhost -->
		<VirtualHost value='/'/> <!-- Default to / -->
		<UserName value='guest'/> <!-- Default to guest -->
		<Password value='guest'/> <!-- Default to guest -->
		<RequestedHeartbeat value='60'/> <!-- Default to 60 seconds -->
		<Port value='5672'/> <!-- Default to 5672 -->
		<Exchange value='logs'/> <!-- Default to logs -->
		<RoutingKey value=''/> <!-- Default to empty -->
	</appender>

License
-------

[APACHE 2](https://raw.github.com/gimmi/Log4Rabbit/master/LICENSE)
