using System;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Scabra.Rpc;

namespace Scabra.Examples.Rpc.Security
{
    public class ExampleScabraSecurityHandler : IScabraSecurityHandler
    {
        static class AuthToken
        {
            public static string Create(string name, string[] roles, string secritKey)
            {
                var payload = name + ";" + string.Join(',', roles);
                var payloadWithSecritKey = Join(payload, secritKey);
                var sign = SignWithMD5(payloadWithSecritKey);

                var authToken = Join(payload, sign);

                return Convert.ToBase64String(Encoding.ASCII.GetBytes(authToken));
            }

            public static bool Verify(string authToken, string secritKey, out string name, out string[] roles)
            {
                name = null; roles = null;

                if (authToken == null)
                    return false;
                
                if (!TryParse(authToken, out var payload, out name, out roles, out var actualSign))
                    return false;

                var payloadWithSecritKey = Join(payload, secritKey);
                var expectedSign = SignWithMD5(payloadWithSecritKey);

                return actualSign == expectedSign;
            }

            public static void ExtractNameAndRoles(string authToken, out string name, out string[] roles)
            {
                name = null; roles = null;

                if (authToken == null)
                    throw new ArgumentNullException(nameof(authToken));

                if (!TryParse(authToken, out var _, out name, out roles, out var _))
                    throw new ArgumentException("Auth token has invalid format.", nameof(authToken));
            }

            private static bool TryParse(string authToken, out string payload, out string name, out string[] roles, out string sign)
            {
                Debug.Assert(authToken != null);

                payload = name = sign = null; roles = null;

                var nxAuthToken = Encoding.ASCII.GetString(Convert.FromBase64String(authToken));

                var i = nxAuthToken.LastIndexOf(';');
                if (i == -1)
                    return false;

                payload = nxAuthToken.Substring(0, i);
                sign = nxAuthToken.Substring(i + 1);

                var payloadTokens = payload.Split(';');
                if (payloadTokens.Length != 2)
                    return false;

                name = payloadTokens[0];
                roles = payloadTokens[1].Split(',');

                return true;
            }

            private static string Join(string payload, string sign) => payload + ";" + sign;

            private static string SignWithMD5(string input)
            {
                using MD5 md5 = MD5.Create();

                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        public static IPrincipal GetPrincipal(string authToken)
        {
            AuthToken.ExtractNameAndRoles(authToken, out var name, out var roles);

            return new ExamplePrincipal(authToken, name, roles);
        }

        private readonly string _secritKey;

        public ExampleScabraSecurityHandler(string secritKey)
        {
            _secritKey = secritKey;
        }

        object IScabraSecurityHandler.DecodeSecret(byte[] bytes)
        {
            if (bytes == null)
                return null;

            return Encoding.ASCII.GetString(bytes);
        }

        byte[] IScabraSecurityHandler.EncodeSecret(object secret)
        {
            if (secret == null || secret is not string authToken)
                return null;

            return Encoding.ASCII.GetBytes(authToken);
        }

        object IScabraSecurityHandler.GetSecret()
        {
            if (Thread.CurrentPrincipal == null)
                return null;

            if (Thread.CurrentPrincipal is not ExamplePrincipal principal)
                return null;

            return principal.AuthToken;
        }

        void IScabraSecurityHandler.TakeSecurityMeasures(object secret)
        {
            if (secret == null)
            {
                // This is the only case when we will
                // work in the unauthenticated context.

                return;
            }

            if (secret is not string authToken)
                throw new NotSupportedException();

            var verified = AuthToken.Verify(authToken, _secritKey, out string name, out string[] roles);
            if (!verified)
                throw new SecurityException("Authentication failed.");

            Thread.CurrentPrincipal = new ExamplePrincipal(authToken, name, roles);
        }

        public string CreateAuthToken(string name, params string[] roles)
        {
            return AuthToken.Create(name, roles, _secritKey);
        }
    }
}
