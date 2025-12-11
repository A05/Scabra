Purpose of this document is to store ideas and thoughts about some technical decisions.

# Adding timeout and cancellation possibilities for RPC

## Requirements

* Timeout and cancellation must be able to be specified for each method separately.
* There is no default timeout value. Calls aren't time limited unless a timeout is specified.
* Timeout is a time span within which a call should be completed.
* If the zero timout value is used then the call immediately times out.
* The timeout is sent with a call to the service and is independently tracked by both the client and the service. It is possible that a call completes on one machine, but by the time the response has returned to the client the call has been timed out.
* Behaviour of the client and the service when a call is timed out:
    * The client should immediately abort the underlying call and throw an error.
    * On the server, the executing call should be aborted but the service method should continue to run until it completes. So, letting the method know that the call is aborting allows the method to be cancelled along with the call.
* Timeout and cancellation propagation will be implemented in future.

## Solution

RPC is asynchronous by nature. Among other things it implies having possibility to get a call's reply asynchonously, to cancel a call, to set a call's timeout and to set a call's metadata. In Scabra RPC contracts are specified with .NET interfaces. For getting a reply asynchronously .NET provides - ```Task```, for cancellation - ```CancallationToken```. Requiring for each .NET interface method to have amoung its parameters ```CancellationToken```, timeout, metadata and returning a ```Task``` is convinient for Scabra, but is not convinient and flexible for the users (developers), because, firstly, there may be needs to remotely call a method synchonosly without ability to cancell it or to be timed out and, secondly, there must be some strict rules of method signatures. gRPC solves it by defining contracs with ```protobuf``` where developers define pure functionality without asynchronous infrastructure overhead. Having such ```protobuf``` contracts gRPC generates .NET methods with all required async features. It is convinient. We will do the same.

## Design

Scabra will be able to accept a contract as a .NET interface without cancellation token, timeout parameters and even without ```Task``` return value. If a method is asynronous the return type will be ```Task``` of course, but if a method is synchronous it will be not required for the method to have ```Task``` return type. To inject additional parameters and change return type to ```Task``` new interfaces and base classes will be generated for both the client and the server based on _the contract .NET interface_ provided by the developer. Let it be named as ```IService```.

  ```
  interface IService 
  { 
    void Method1(int p); 
    byte Method2(int[] a); 
  } 
  ```

### Client

Scabra will provide the extension method of _the contract .NET interface_, named ```Upgrade```. It will return the new generated .NET interface ```IService_Upgraded``` inhereted from _the contract .NET interface_. The new interface duplicates each of the original methods and adds the new parameter of ```CallOptions``` type to its parameter list. ```CallOptions``` contains ```Timeout``` and ```CancellationToken``` properties which are used for passing timeout value and cancellation request. By default ```Timeout``` property has ```Timeout.InfiniteTimeSpan``` value which means a call is not time limited. By default ```CancellationToken``` property has ```CancellationToken.None``` value which means a call can not be cancelled. The client proxy declares the new methods as abstract ones. Each of the original interface methods of the proxy invokes the corresponding new method with the default values of ```CallOptions``` properties. Additionally Scabra will change the return type of each new methods to ```Task``` if the original type is ```void``` or to ```Task<T>``` if the original type is ```T```. If the original type is ```Task``` then the return type is left the same. Handling the return types of original interface methods is obvious.

  ```
  intercase IService_Upgraded : IService
  { 
    Task Method1(int p, CallOptions options); 
    Task<byte> Method2(int[] a, CallOptions options); 
  }

  static class IServiceExtension 
  {
    static IService_Upgraded Upgrade(static IService service)
    {
        return (IService_Upgraded) service;
    }
  }

  [RpcClientProxy]
  internal partial class ServiceScabraProxy : IService_Upgraded
  {
    public void Method1(int p) { Method1(p, CallOptions.Null).Result; }
    public byte Method2(int[] a) { return Method2(a, CallOptions.Null).Result; }

    public Task Method1(int p, CallOptions options) { ... }
    public Task<byte> Method2(int[] a, CallOptions options) { ... }
  }
 
  IService service = ...;
  await service.Upgrade().Method1(p, new CallOptions { Timeout = 25, CancellationToken = ... });
  ```

### Server

Scabra will generate the base class for the service inherited from _the contract .NET interface_. The base class name will be ```BaseService```. It will duplicates each of the original methods and adds the new parameter of ```ServerCallContext``` type to its parameter list. ```ServerCallContext``` contains ```Timeout``` and ```CancellationToken``` properties with values specified in the client. Scabra will call those duplicates. Implementation of the original methods will throw ```NotSupportedException```. The return types of the original methods will not be changed.

  ```
  [RpcService]
  internal partial class BaseService : IService
  {
    public void Method1(int p) { throw new NotSupportedException(); }
    public byte Method2(int[] a) { throw new NotSupportedException(); }

    public void Method1(int p, ServerCallContext context) { ... } // abstract
    public byte Method2(int[] a, ServerCallContext context) { ... } // abstract
  }
  ```