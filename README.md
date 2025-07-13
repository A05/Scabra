Scabra is a .net library based on [ZeroMQ/NetMQ](https://github.com/zeromq/netmq) for organizing interactions between components of distributed systems via TCP. Currently _RPC_ and _Observer_ patterns are supported. Various messaging patterns, protocols and middleware can be quite easily added.

Scabra is a project for developers' fun and pleasure. If you're looking for something like Scabra, please consider truly mature and stable libraries, frameworks and tools such as [pure ZeroMQ/NetMQ](https://github.com/zeromq/netmq), [gRPC](https://github.com/grpc/grpc-dotnet), [ASP.NET](https://github.com/dotnet/aspnetcore), [Redis](https://github.com/redis/redis), [RabbitMQ](https://github.com/rabbitmq/rabbitmq-dotnet-client), [Kafka](https://github.com/apache/kafka), etc. 

This project is supported by [A05](https://github.com/a05/).

## Features

* Scabra works only within .NET ecosystem.

### RPC pattern

* Service contracts may be defined in a separated assembly.
* Client proxy may be made manually or be automatically generated and is fully debuggable.
* Overhead of RPC call is comparable with gRPC.
* _TODO: There are some more ...__

### Observer pattern

* _TODO: There are some more ..._

## Installation

You can download Scabra via [NuGet](https://github.com/a05/scabra/).

## Documentation

Currently, the only way to get started with Scabra is by the following examples:

* [RPC example](https://github.com/a05/scabra/) shows how to invoke service methods remotely.
* [Observer example](https://github.com/a05/scabra/) shows how to implement _Observer_ pattern.
* [Security example](https://github.com/a05/scabra/) shows how the secure communication can be implemented with Scabra.
* [Docker example](https://github.com/a05/scabra/) shows how to run applications communicating with Scabra in Docker containers. 

## Contributing

Once you have identified an issue to work on:

* [Fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo) the repository. 
* Work in a [new branch](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-and-deleting-branches-within-your-repository).
* Open and propose your changes with a [pull request](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request).

## Reporting security issues

To report a security issue, please use the GitHub Security Advisory ["Report a Vulnerability"](https://github.com/a05/scabra/security/advisories/new) tab.

## Areas of improvement by ISO 9126

### Functionality

* Add the ability to _timeout_ for RPC calls. RPC client/server disposing should take into account timeouts of all calls in progress.
* Add the ability to cancel RPC calls.
* Add the ability for RPC service interfaces and proxies to be ```internal```. 
* Add ```async/await``` support in RPC.
* Make payload serialization configurable. 
* Add the ability to customize resilience and transient-fault-handling strategies.
* Secure communication in _Observer_ pattern.
* Make RPC adhere to the ```zerorpc``` standard.	
* Add _XPub/XSub_ pattern support.
* Add _Push/Pull_ pattern support.

### Reliability

* Add endpoint ```Crashed``` event.
* Add template ```EndpointRoutine``` method with ```Crashed``` event.
* Add heartbeating support.
* Add the ability to reconnect after network failures.
* Add resilience and transient-fault-handling strategies.

### Usability

* Ensure all error messages are concise and clear.
* Create users(developers) documentation.
* Design configuration management process.

### Efficiency

* Add resource utilization metrics like internal endpoint queues, call duration, avarage message bytes, etc.
* Use ```Trie``` data structure in Observer ```MessageHandlers``` and RPC ```Marshaler``` classes.
* Create RPC & Observer load and performance tests.
* Try optimizing the proxy generator's performance even further by making it truly incremental.
* Try optimizing RPC performance by adding various _sending/receiving_ strategies like _1-send/1-receive_, _all-send/all-receive_, _2-send/1-receive_ etc. Think about runtime adjustable client endpoint strategy.
* Try to send an RPC call to socket directly without the intermidiate pending queue.

### Maintainability

* Add distributed tracing for _RPC_ and _Observer_ patterns.
* Make logging work in unit tests.
* Make Scabra testable at the same level as gRPC.
* Make Scabra's tools similar to those in gRPC.
* Make Github workflows.
* Upgrade all infrastructure projects to the latest .NET.

### Portability

* Add k8s rolling update support.