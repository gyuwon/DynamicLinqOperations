﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicLinq
{
    public static class DynamicLinqOperations
    {
        private static MethodInfo orderByTemplate = typeof(Enumerable)
            .GetMember("OrderBy")
            .OfType<MethodInfo>()
            .Single(m => m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

        private static MethodInfo whereTemplate = typeof(Enumerable)
            .GetMember("Where")
            .OfType<MethodInfo>()
            .Where(m => m.IsGenericMethodDefinition && m.GetParameters().Length == 2)
            .Single(m => m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);

        private static Delegate GetGetter<T>(PropertyInfo prop)
        {
            var funcType = typeof(Func<,>).MakeGenericType(typeof(T), prop.PropertyType);
            return Delegate.CreateDelegate(funcType, prop.GetGetMethod());
        }
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> s, string propertyName)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop == null)
                throw new InvalidOperationException("Property Not Found");
            var operation = orderByTemplate.MakeGenericMethod(typeof(T), prop.PropertyType);
            var keySelector = GetGetter<T>(prop);
            return (IEnumerable<T>)operation.Invoke(null, new object[] { s, keySelector });
        }

        public static IEnumerable<TSource> Where<TSource, TProperty>(this IEnumerable<TSource> s, string propertyName, Func<TProperty, bool> predicate)
        {
            var prop = typeof(TSource).GetProperty(propertyName);
            if (prop == null)
                throw new InvalidOperationException("Property Not Found");
            var operation = whereTemplate.MakeGenericMethod(typeof(TSource));
            var selector = (Func<TSource, TProperty>)GetGetter<TSource>(prop);
            Func<TSource, bool> p = e => predicate(selector(e));
            return (IEnumerable<TSource>)operation.Invoke(null, new object[] { s, p });
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> s, string propertyName, Func<dynamic, bool> predicate)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop == null)
                throw new InvalidOperationException("Property Not Found");
            var operation = whereTemplate.MakeGenericMethod(typeof(T));
            var selector = GetGetter<T>(prop);
            Func<T, bool> p = e => predicate(selector.DynamicInvoke(e));
            return (IEnumerable<T>)operation.Invoke(null, new object[] { s, p });
        }
    }
}