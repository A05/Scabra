docker build -f ".\Docker.Server\Dockerfile" --force-rm -t scabra-example-server "..\.."
docker build -f ".\Docker.Client\Dockerfile" --force-rm -t scabra-example-client "..\.."