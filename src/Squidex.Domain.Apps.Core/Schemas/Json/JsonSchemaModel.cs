// ==========================================================================
//  JsonSchemaModel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    internal sealed class JsonSchemaModel
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Instant Created { get; set; }

        [JsonProperty]
        public Instant LastModified { get; set; }

        [JsonProperty]
        public RefToken CreatedBy { get; set; }

        [JsonProperty]
        public RefToken LastModifiedBy { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string ScriptQuery { get; set; }

        [JsonProperty]
        public string ScriptCreate { get; set; }

        [JsonProperty]
        public string ScriptUpdate { get; set; }

        [JsonProperty]
        public string ScriptChange { get; set; }

        [JsonProperty]
        public string ScriptDelete { get; set; }

        [JsonProperty]
        public bool IsPublished { get; set; }

        [JsonProperty]
        public SchemaProperties Properties { get; set; }

        [JsonProperty]
        public List<JsonFieldModel> Fields { get; set; }
    }
}