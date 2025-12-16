# Kubernetes RPC

This example demonstrates the compatibility of Scabra's RPC implementation with Kubernetes.

To run this example do the following:

1. Create ```scabra``` network using the command: \
   ```docker network create -d bridge scabra```
2. Build the server and client images using the commands: \
   ```
   docker build -f ".\k8s.Rpc.Server\Dockerfile" --force-rm -t scabra-example-k8s-rpc-server "..\.."
   docker build -f ".\k8s.Rpc.Client\Dockerfile" --force-rm -t scabra-example-k8s-rpc-client "..\.."
   ```
   
3. Run the server and client containers using the commands: \
   ```
   docker rm -f scabra-example-k8s-rpc-server
   docker run -dt --name scabra-example-k8s-rpc-server --network=scabra scabra-example-k8s-rpc-server:latest
   
   docker rm -f scabra-example-k8s-rpc-client
   docker run -dt --name scabra-example-k8s-rpc-client --network=scabra scabra-example-k8s-rpc-client:latest
   ```

# Kubernetes Observer

... TODO ...