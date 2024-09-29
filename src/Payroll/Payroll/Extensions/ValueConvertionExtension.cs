using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Extensions
{
    public static class ValueConversionExtension
    {
        //public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
        //{
        //    ValueConverter<T, string> converter = new ValueConverter<T, string>
        //    (
        //        v => JsonConvert.SerializeObject(v),
        //        v => JsonConvert.DeserializeObject<T>(v) ?? new T()
        //    );

        //    ValueComparer<T> comparer = new ValueComparer<T>
        //    (
        //        (l, r) => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(r),
        //        v => v == null ? 0 : JsonConvert.SerializeObject(v).GetHashCode(),
        //        v => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v))
        //    );

        //    propertyBuilder.HasConversion(converter);
        //    propertyBuilder.Metadata.SetValueConverter(converter);
        //    propertyBuilder.Metadata.SetValueComparer(comparer);
        //    propertyBuilder.HasColumnType("jsonb");

        //    return propertyBuilder;
        //}
        //public static PropertyBuilder<T> HasJsonConversionList<T>(this PropertyBuilder<IList<T>> propertyBuilder) where T : class, new()
        //{
        //    ValueConverter<IList<T>, string> converter = new ValueConverter<IList<T>, string>
        //    (
        //        v => JsonConvert.SerializeObject(v),
        //        v => JsonConvert.DeserializeObject<IList<T>>(v) ?? new List<T>()
        //    );

        //    ValueComparer<IList<T>> comparer = new ValueComparer<IList<T>>
        //    (
        //        (l, r) => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(r),
        //        v => v == null ? 0 : JsonConvert.SerializeObject(v).GetHashCode(),
        //        v => JsonConvert.DeserializeObject<IList<T>>(JsonConvert.SerializeObject(v))
        //    );

        //    propertyBuilder.HasConversion(converter);
        //    propertyBuilder.Metadata.SetValueConverter(converter);
        //    propertyBuilder.Metadata.SetValueComparer(comparer);
        //    propertyBuilder.HasColumnType("jsonb");

        //    return propertyBuilder;
        //}
    }
}
