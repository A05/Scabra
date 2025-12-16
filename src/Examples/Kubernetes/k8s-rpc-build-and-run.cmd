docker rm -f scabra-example-k8s-rpc-client
docker rm -f scabra-example-k8s-rpc-server

docker build -f ".\k8s.Rpc.Server\Dockerfile" --force-rm -t scabra-example-k8s-rpc-server "..\.."
docker build -f ".\k8s.Rpc.Client\Dockerfile" --force-rm -t scabra-example-k8s-rpc-client "..\.."

docker run -dt --name scabra-example-k8s-rpc-server --network=scabra scabra-example-k8s-rpc-server:latest
docker run -dt --name scabra-example-k8s-rpc-client --network=scabra scabra-example-k8s-rpc-client:latest