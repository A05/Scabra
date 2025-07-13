docker rm -f scabra-example-server
docker rm -f scabra-example-client

docker run -dt --name scabra-example-server --network=scabra scabra-example-server:latest
docker run -dt --name scabra-example-client --network=scabra scabra-example-client:latest