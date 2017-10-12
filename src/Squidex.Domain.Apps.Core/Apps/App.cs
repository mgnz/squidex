// ==========================================================================
//  App.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class App : ImmutableDomainObject
    {
        private readonly string name;
        private AppClients clients = AppClients.Empty;
        private AppContributors contributors = AppContributors.Empty;
        private LanguagesConfig languages = LanguagesConfig.Create(Language.EN);
        private RefToken planOwner;
        private string planId;

        public string Name
        {
            get { return name; }
        }

        public string PlanId
        {
            get { return planId; }
        }

        public RefToken PlanOwner
        {
            get { return planOwner; }
        }

        public LanguagesConfig Languages
        {
            get { return languages; }
        }

        public PartitionResolver PartitionResolver
        {
            get { return languages.ToResolver(); }
        }

        public IReadOnlyDictionary<string, AppClient> Clients
        {
            get { return clients.Clients; }
        }

        public IReadOnlyDictionary<string, AppContributorPermission> Contributors
        {
            get { return contributors.Contributors; }
        }

        private App(Guid id, string name, Instant now, RefToken actor)
            : base(id, now, actor)
        {
            this.name = name;
        }

        public static App Create(Guid id, string name, Instant now, RefToken actor)
        {
            Guard.NotEmpty(id, nameof(id));

            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException("Cannot create a new app", error);
            }

            return new App(id, name, now, actor);
        }

        public App UpdateLanguages(Instant now, RefToken actor, Func<LanguagesConfig, LanguagesConfig> updater)
        {
            return Update<App>(now, actor, clone =>
            {
                clone.languages = updater?.Invoke(languages) ?? clone.languages;
            });
        }

        public App UpdateContributors(Instant now, RefToken actor, Func<AppContributors, AppContributors> updater)
        {
            return Update<App>(now, actor, clone =>
            {
                clone.contributors = updater?.Invoke(contributors) ?? clone.contributors;
            });
        }

        public App UpdateClients(Instant now, RefToken actor, Func<AppClients, AppClients> updater)
        {
            return Update<App>(now, actor, clone =>
            {
                clone.clients = updater?.Invoke(clients) ?? clone.clients;
            });
        }

        public App ChangePlan(Instant now, RefToken actor, string newPlanId)
        {
            ThrowIfOtherUser(newPlanId, actor);

            return Update<App>(now, actor, clone =>
            {
                clone.planId = newPlanId;
                clone.planOwner = actor;
            });
        }

        private void ThrowIfOtherUser(string newPlanId, RefToken actor)
        {
            if (!string.IsNullOrWhiteSpace(newPlanId) && planOwner != null && !planOwner.Equals(actor))
            {
                throw new ValidationException("Plan can only be changed from current user.");
            }

            if (string.Equals(newPlanId, planId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("App has already this plan.");
            }
        }
    }
}
