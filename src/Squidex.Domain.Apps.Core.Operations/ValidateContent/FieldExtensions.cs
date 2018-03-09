﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public static class FieldExtensions
    {
        public static void AddError(this ConcurrentBag<ValidationError> errors, string message, Field field, IFieldPartitionItem partitionItem = null)
        {
            AddError(errors, message, !string.IsNullOrWhiteSpace(field.RawProperties.Label) ? field.RawProperties.Label : field.Name, field.Name, partitionItem);
        }

        public static void AddError(this ConcurrentBag<ValidationError> errors, string message, string fieldName, IFieldPartitionItem partitionItem = null)
        {
            AddError(errors, message, fieldName, fieldName, partitionItem);
        }

        public static void AddError(this ConcurrentBag<ValidationError> errors, string message, string displayName, string fieldName, IFieldPartitionItem partitionItem = null)
        {
            if (partitionItem != null && partitionItem != InvariantPartitioning.Instance.Master)
            {
                displayName += $" ({partitionItem.Key})";
            }

            errors.Add(new ValidationError(message.Replace("<FIELD>", displayName), fieldName));
        }

        public static async Task ValidateAsync(this Field field, JToken value, ValidationContext context, Action<string> addError)
        {
            try
            {
                var typedValue = value.IsNull() ? null : JsonValueConverter.ConvertValue(field, value);

                foreach (var validator in ValidatorsFactory.CreateValidators(field))
                {
                    await validator.ValidateAsync(typedValue, context, addError);
                }
            }
            catch
            {
                addError("<FIELD> is not a valid value.");
            }
        }
    }
}
