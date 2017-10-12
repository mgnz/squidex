// ==========================================================================
//  Schema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.OData.Edm;
using NJsonSchema;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class Schema : ImmutableDomainObject
    {
        private readonly string name;
        private SchemaProperties properties = new SchemaProperties();
        private ImmutableList<Field> fields = ImmutableList<Field>.Empty;
        private ImmutableDictionary<long, Field> fieldsById;
        private ImmutableDictionary<string, Field> fieldsByName;
        private string scriptQuery;
        private string scriptCreate;
        private string scriptUpdate;
        private string scriptDelete;
        private string scriptChange;
        private bool isPublished;

        public string Name
        {
            get { return name; }
        }

        public string ScriptQuery
        {
            get { return scriptQuery; }
        }

        public string ScriptCreate
        {
            get { return scriptCreate; }
        }

        public string ScriptUpdate
        {
            get { return scriptUpdate; }
        }

        public string ScriptDelete
        {
            get { return scriptDelete; }
        }

        public string ScriptChange
        {
            get { return scriptChange; }
        }

        public bool IsPublished
        {
            get { return isPublished; }
        }

        public ImmutableList<Field> Fields
        {
            get { return fields; }
        }

        public ImmutableDictionary<long, Field> FieldsById
        {
            get { return fieldsById; }
        }

        public ImmutableDictionary<string, Field> FieldsByName
        {
            get { return fieldsByName; }
        }

        public SchemaProperties Properties
        {
            get { return properties; }
        }

        private Schema(Guid id, string name, Instant now, RefToken actor)
            : base(id, now, actor)
        {
            this.name = name;
        }

        public override void OnInit()
        {
            properties.Freeze();

            fieldsById = fields.ToImmutableDictionary(x => x.Id);
            fieldsByName = fields.ToImmutableDictionary(x => x.Name);
        }

        public static Schema Create(Guid id, string name, Instant now, RefToken actor)
        {
            Guard.NotEmpty(id, nameof(id));

            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException("Cannot create a new schema", error);
            }

            return new Schema(id, name, now, actor);
        }

        public Schema UpdateField(Instant now, RefToken actor, long fieldId, FieldProperties newProperties)
        {
            return UpdateField(now, actor, fieldId, field => field.Update(newProperties));
        }

        public Schema LockField(Instant now, RefToken actor, long fieldId)
        {
            return UpdateField(now, actor, fieldId, field => field.Lock());
        }

        public Schema DisableField(Instant now, RefToken actor, long fieldId)
        {
            return UpdateField(now, actor, fieldId, field => field.Disable());
        }

        public Schema EnableField(Instant now, RefToken actor, long fieldId)
        {
            return UpdateField(now, actor, fieldId, field => field.Enable());
        }

        public Schema HideField(Instant now, RefToken actor, long fieldId)
        {
            return UpdateField(now, actor, fieldId, field => field.Hide());
        }

        public Schema ShowField(Instant now, RefToken actor, long fieldId)
        {
            return UpdateField(now, actor, fieldId, field => field.Show());
        }

        public Schema RenameField(Instant now, RefToken actor, long fieldId, string newName)
        {
            return UpdateField(now, actor, fieldId, field => field.Rename(newName));
        }

        public Schema DeleteField(Instant now, RefToken actor, long fieldId)
        {
            if (fieldsById.TryGetValue(fieldId, out var field) && field.IsLocked)
            {
                throw new DomainException($"Field {fieldId} is locked.");
            }

            return Update<Schema>(now, actor, clone =>
            {
                clone.fields = fields.Where(x => x.Id != fieldId).ToImmutableList();
            });
        }

        public Schema Update(Instant now, RefToken actor, SchemaProperties newProperties)
        {
            Guard.NotNull(newProperties, nameof(newProperties));

            return Update<Schema>(now, actor, clone =>
            {
                clone.properties = newProperties;
            });
        }

        public Schema Publish(Instant now, RefToken actor)
        {
            if (isPublished)
            {
                throw new DomainException("Schema is already published");
            }

            return Update<Schema>(now, actor, clone =>
            {
                clone.isPublished = true;
            });
        }

        public Schema Unpublish(Instant now, RefToken actor)
        {
            if (!isPublished)
            {
                throw new DomainException("Schema is not published");
            }

            return Update<Schema>(now, actor, clone =>
            {
                clone.isPublished = false;
            });
        }

        public Schema ConfigureScripts(Instant now, RefToken actor, string query, string create, string update, string change, string deleted)
        {
            return Update<Schema>(now, actor, clone =>
            {
                clone.scriptChange = change;
                clone.scriptCreate = create;
                clone.scriptDelete = deleted;
                clone.scriptQuery = query;
                clone.scriptUpdate = update;
            });
        }

        public Schema ReorderFields(Instant now, RefToken actor, List<long> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            if (ids.Count != fields.Count || ids.Any(x => !fieldsById.ContainsKey(x)))
            {
                throw new ArgumentException("Ids must cover all fields.", nameof(ids));
            }

            var newFields = fields.OrderBy(f => ids.IndexOf(f.Id)).ToImmutableList();

            return Update<Schema>(now, actor, clone =>
            {
                clone.fields = newFields;
            });
        }

        public Schema UpdateField(Instant now, RefToken actor, long fieldId, Func<Field, Field> updater)
        {
            Guard.NotNull(updater, nameof(updater));

            if (!fieldsById.TryGetValue(fieldId, out var field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Field));
            }

            var newField = updater(field);

            return AddOrUpdateField(now, actor, newField);
        }

        public Schema AddOrUpdateField(Instant now, RefToken actor, Field field)
        {
            Guard.NotNull(field, nameof(field));

            if (fieldsById.Values.Any(f => f.Name == field.Name && f.Id != field.Id))
            {
                throw new ValidationException($"A field with name '{field.Name}' already exists.");
            }

            ImmutableList<Field> newFields;

            if (fieldsById.ContainsKey(field.Id))
            {
                newFields = fields.Select(f => f.Id == field.Id ? field : f).ToImmutableList();
            }
            else
            {
                newFields = fields.Add(field);
            }

            return Update<Schema>(now, actor, clone =>
            {
                clone.fields = newFields;
            });
        }

        public EdmComplexType BuildEdmType(PartitionResolver partitionResolver, Func<EdmComplexType, EdmComplexType> typeResolver)
        {
            Guard.NotNull(typeResolver, nameof(typeResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = Name.ToPascalCase();

            var edmType = new EdmComplexType("Squidex", schemaName);

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToEdmType(edmType, partitionResolver, schemaName, typeResolver);
            }

            return edmType;
        }

        public JsonSchema4 BuildJsonSchema(PartitionResolver partitionResolver, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = Name.ToPascalCase();

            var schema = new JsonSchema4 { Type = JsonObjectType.Object };

            foreach (var field in fieldsByName.Values.Where(x => !x.IsHidden))
            {
                field.AddToJsonSchema(schema, partitionResolver, schemaName, schemaResolver);
            }

            return schema;
        }
    }
}