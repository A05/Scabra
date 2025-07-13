Purpose of this document is to store ideas and thoughts about some technical decisions.

# Adding timeout for an RPC call

* The idea is generate source code for "enriched" clone interface of the original service interface that will contain the additional parameter of the new ```ScabraRpcOptions``` type which will contain ```Timeout``` property for each call and will be passed as an argument for the channel InvokeMethod().
* To switch from the original interface to the "enriched" one, the extension method ```WithOptions``` for the original inteface will be generated. This method will cast the original interface into "enriched" one.
  ```
  interface IService 
  { 
    void Method(int p); 
  } 
  
  intercase IWithOptionsService 
  { 
    void Method(int p, ScabraRpcOptions scabraRpcOptions); 
  }

  static class IServiceExtension 
  {
    static IWithOptionsService WithOptions(static IService service)
    {
        return (IWithOptionsService) service;
    }
  }
 
  IService service = ...;
  service.WithOptions().Method(p, new ScabraRpcOptions { Timeout = 25 });
  ```