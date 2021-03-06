﻿using System;

using Norma.Eta.Extensions;

namespace Norma.Eta.Models
{
#pragma warning disable CS0659 // 型は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません

    public class EnumWrap<T> where T : struct
#pragma warning restore CS0659 // 型は Object.Equals(object o) をオーバーライドしますが、Object.GetHashCode() をオーバーライドしません
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Display { get; }

        public T EnumValue { get; }

        public EnumWrap(T value)
        {
            if (!typeof(T).IsEnum)
                throw new NotSupportedException($"{nameof(EnumWrap<T>)} does not support {value.GetType()}");
            Display = (value as Enum)?.ToLocaleString();
            EnumValue = value;
        }

        #region Overrides of Object

#pragma warning disable 659

        public override bool Equals(object obj)
#pragma warning restore 659
        {
            var value = obj as EnumWrap<T>;
            return EnumValue.Equals(value?.EnumValue);
        }

        #endregion
    }
}