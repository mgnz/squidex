// ==========================================================================
//  AppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppClients : Cloneable<AppClients>
    {
        public static readonly AppClients Empty = new AppClients();

        private ImmutableDictionary<string, AppClient> clients = ImmutableDictionary<string, AppClient>.Empty;

        public IReadOnlyDictionary<string, AppClient> Clients
        {
            get { return clients; }
        }

        private AppClients()
        {
        }

        public AppClients Add(string clientId, string secret)
        {
            ThrowIfFound(clientId, () => "Cannot add client");

            return Clone(clone =>
            {
                clone.clients = clients.SetItem(clientId, new AppClient(secret, clientId, AppClientPermission.Editor));
            });
        }

        public AppClients Rename(string clientId, string name)
        {
            ThrowIfNotFound(clientId);

            return Clone(clone =>
            {
                clone.clients = clients.SetItem(clientId, clients[clientId].Rename(name, () => "Cannot rename client"));
            });
        }

        public AppClients Update(string clientId, AppClientPermission permission)
        {
            ThrowIfNotFound(clientId);

            return Clone(clone =>
            {
                clone.clients = clients.SetItem(clientId, clients[clientId].Update(permission, () => "Cannot update client"));
            });
        }

        public AppClients Revoke(string clientId)
        {
            ThrowIfNotFound(clientId);

            return Clone(clone =>
            {
                clone.clients = clients.Remove(clientId);
            });
        }

        private void ThrowIfNotFound(string clientId)
        {
            if (!clients.ContainsKey(clientId))
            {
                throw new DomainObjectNotFoundException(clientId, "Contributors", typeof(App));
            }
        }

        private void ThrowIfFound(string clientId, Func<string> message)
        {
            if (clients.ContainsKey(clientId))
            {
                var error = new ValidationError("Client id is alreay part of the app", "Id");

                throw new ValidationException(message(), error);
            }
        }
    }
}
