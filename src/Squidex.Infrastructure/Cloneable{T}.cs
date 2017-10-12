// ==========================================================================
//  Cloneable_T.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public abstract class Cloneable<T> : Cloneable where T : Cloneable
    {
        protected Cloneable()
        {
            OnInit();
        }

        protected T Clone(Action<T> updater)
        {
            return Clone<T>(updater);
        }

        protected TResult Clone<TResult>(Action<TResult> updater) where TResult : Cloneable
        {
            var clone = (TResult)MemberwiseClone();

            updater(clone);

            clone.OnInit();

            return clone;
        }
    }
}
