// ==========================================================================
//  SchemaConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class SchemaConverter : JsonConverter
    {
        private readonly FieldRegistry fieldRegistry;

        public SchemaConverter(FieldRegistry fieldRegistry)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.fieldRegistry = fieldRegistry;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Schema schema)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("version");
                writer.WriteValue("1.0");

                writer.WritePropertyName("value");
                writer.WriteStartObject();

                var model = new JsonSchemaModel
                {
                    Id = schema.Id,
                    Name = schema.Name,
                    Created = schema.Created,
                    CreatedBy = schema.CreatedBy,
                    IsPublished = schema.IsPublished,
                    LastModified = schema.LastModified,
                    LastModifiedBy = schema.LastModifiedBy,
                    Properties = schema.Properties,
                    ScriptQuery = schema.ScriptQuery,
                    ScriptCreate = schema.ScriptCreate,
                    ScriptUpdate = schema.ScriptUpdate,
                    ScriptChange = schema.ScriptChange,
                    ScriptDelete = schema.ScriptDelete,
                };

                model.Fields =
                    schema.Fields.Select(x =>
                        new JsonFieldModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            IsHidden = x.IsHidden,
                            IsLocked = x.IsLocked,
                            IsDisabled = x.IsDisabled,
                            Partitioning = x.Paritioning.Key,
                            Properties = x.RawProperties
                        }).ToList();

                serializer.Serialize(writer, model);

                writer.WriteEndObject();

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            reader.Read();
            reader.Read();

            var version = reader.ReadAsString();

            if (version != "1.0")
            {
                throw new JsonException($"Invalid version, expected 1.0, got {version}");
            }

            reader.Read();
            reader.Read();

            var model = serializer.Deserialize<JsonSchemaModel>(reader);

            var schema = Schema.Create(model.Id, model.Name, model.Created, model.CreatedBy);

            schema =
                schema.Update(
                    model.LastModified,
                    model.LastModifiedBy,
                    model.Properties);

            schema =
                schema.ConfigureScripts(
                    model.LastModified,
                    model.LastModifiedBy,
                    model.ScriptQuery,
                    model.ScriptCreate,
                    model.ScriptUpdate,
                    model.ScriptChange,
                    model.ScriptDelete);

            foreach (var fieldModel in model.Fields)
            {
                var parititonKey = new Partitioning(fieldModel.Partitioning);

                var field = fieldRegistry.CreateField(fieldModel.Id, fieldModel.Name, parititonKey, fieldModel.Properties);

                if (fieldModel.IsDisabled)
                {
                    field = field.Disable();
                }

                if (fieldModel.IsLocked)
                {
                    field = field.Lock();
                }

                if (fieldModel.IsHidden)
                {
                    field = field.Hide();
                }

                schema = schema.AddOrUpdateField(model.LastModified, model.LastModifiedBy, field);
            }

            reader.Read();
            reader.Read();

            return schema;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Schema);
        }
    }
}
