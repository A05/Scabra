using Microsoft.Extensions.Configuration;
using System;

namespace Scabra.Rpc.Client.Hosting
{
    internal class ScabraRpcChannelProvider : IScabraRpcChannelProvider
    {
        private readonly (string name, IScabraRpcChannel channel)[] _channels;

        internal ScabraRpcChannelProvider(IConfigurationSection configuration, IScabraSecurityHandler securityHandler)
        {
            var options = configuration.Get<ClientsOptions>();

            _channels = new (string, IScabraRpcChannel)[options.Clients.Length];

            for (int i = 0; i < options.Clients.Length; i++)
            {
                var client = options.Clients[i];

                var channelOptions = new ScabraRpcChannelOptions() { Address = client.Address };
                var channel = new ScabraRpcChannel(channelOptions, securityHandler);

                _channels[i] = (client.Name, channel);
            }
        }

        public IScabraRpcChannel GetChannel(string name)
        {
            foreach ((string name, IScabraRpcChannel channel) i in _channels)
                if (i.name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return i.channel;

            throw new ArgumentOutOfRangeException(nameof(name), $"'{name}' channel is not found.");
        }
    }
}
