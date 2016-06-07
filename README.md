# MvvmCrossBindingInflatePerformanceTest
Performance test for MvvmCross BindingInflate on Android

This repository is related to issue [#1342](https://github.com/MvvmCross/MvvmCross/issues/1342) on MvvmCross.

This test uses a modified local version of MvvmCross printing the time used by the BindingInflate.

It consist on only one activity that is opened and closed automatically 20 times.

The SetContentView method of MvxActivity has been modified.

```csharp
public override void SetContentView(int layoutResId)
{
	View view;
	var sw = Stopwatch.StartNew();
	view = this.BindingInflate(layoutResId, null);
	sw.Stop();
	Mvx.TaggedTrace(MvxTraceLevel.Diagnostic, "BindingInflate", "ElapsedTicks {0}, ElapsedMilliseconds {1}", sw.ElapsedTicks, sw.ElapsedMilliseconds);
	this.SetContentView(view);
}
```

I created a simple version of the MvvmCross/Platform/Platform/ReflectionExtensions.cs source file using neither Linq nor Yield

```csharp
namespace MvvmCross.Platform
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public static class ReflectionExtensions
	{
		public static List<Type> GetTypes(this Assembly assembly)
		{
			var list = new List<Type>();
			foreach (var type in assembly.DefinedTypes)
				list.Add(type.AsType());
			return list;
		}

		public static EventInfo GetEvent(this Type type, string name)
		{
			return type.GetRuntimeEvent(name);
		}

		public static IEnumerable<Type> GetInterfaces(this Type type)
		{
			return type.GetTypeInfo().ImplementedInterfaces;
		}

		public static bool IsAssignableFrom(this Type type, Type otherType)
		{
			return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
		}

		public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
		{
			return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
		}

		public static List<ConstructorInfo> GetConstructors(this Type type)
		{
			var ctors = type.GetTypeInfo().DeclaredConstructors;

			List<ConstructorInfo> list = new List<ConstructorInfo>();
			foreach (var ctor in ctors)
				if (ctor.IsPublic)
					list.Add(ctor);
			return list;
		}

		public static bool IsInstanceOfType(this Type type, object obj)
		{
			var objectType = obj.GetType();
			return type.IsAssignableFrom(obj.GetType()) || objectType.IsMarshalByRefObject();
		}

		private static bool IsMarshalByRefObject(this Type type)
		{
			return type != null && type.FullName == "System.MarshalByRefObject";
		}

		//private static bool IsMarshalByRefObject(this object obj)
		//{
		//	return obj != null && obj.GetType().FullName == "System.MarshalByRefObject";
		//}

		public static MethodInfo GetAddMethod(this EventInfo eventInfo, bool nonPublic = false)
		{
			if (eventInfo.AddMethod == null || (!nonPublic && !eventInfo.AddMethod.IsPublic))
			{
				return null;
			}

			return eventInfo.AddMethod;
		}

		public static MethodInfo GetRemoveMethod(this EventInfo eventInfo, bool nonPublic = false)
		{
			if (eventInfo.RemoveMethod == null || (!nonPublic && !eventInfo.RemoveMethod.IsPublic))
			{
				return null;
			}

			return eventInfo.RemoveMethod;
		}

		public static MethodInfo GetGetMethod(this PropertyInfo property, bool nonPublic = false)
		{
			if (property.GetMethod == null || (!nonPublic && !property.GetMethod.IsPublic))
			{
				return null;
			}

			return property.GetMethod;
		}

		public static MethodInfo GetSetMethod(this PropertyInfo property, bool nonPublic = false)
		{
			if (property.SetMethod == null || (!nonPublic && !property.SetMethod.IsPublic))
			{
				return null;
			}

			return property.SetMethod;
		}

		public static List<PropertyInfo> GetProperties(this Type type)
		{
			return GetProperties(type, BindingFlags.FlattenHierarchy | BindingFlags.Public);
		}

		private static bool NullSafeIsPublic(this MethodInfo info)
		{
			if (info == null)
				return false;
			return info.IsPublic;
		}

		private static bool NullSafeIsStatic(this MethodInfo info)
		{
			if (info == null)
				return false;
			return info.IsStatic;
		}

		public static List<PropertyInfo> GetProperties(this Type type, BindingFlags flags)
		{
			var properties = type.GetTypeInfo().DeclaredProperties;
			if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy)
			{
				properties = type.GetRuntimeProperties();
			}

			List<PropertyInfo> list = new List<PropertyInfo>();
			foreach (var property in properties)
			{
				var getMethod = property.GetMethod;
				var setMethod = property.SetMethod;
				if (getMethod == null && setMethod == null) continue;

				var publicTest = (flags & BindingFlags.Public) != BindingFlags.Public ||
					getMethod.NullSafeIsPublic() || setMethod.NullSafeIsPublic();

				var instanceTest = (flags & BindingFlags.Instance) != BindingFlags.Instance ||
					!getMethod.NullSafeIsStatic() || !setMethod.NullSafeIsStatic();

				var staticTest = (flags & BindingFlags.Static) != BindingFlags.Static ||
					getMethod.NullSafeIsStatic() || setMethod.NullSafeIsStatic();

				if (publicTest && instanceTest && staticTest)
				{
					list.Add(property);
				}
			}

			return list;
		}

		public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
		{
			var properties = GetProperties(type, flags);

			return properties.Find(x => x.Name == name);
		}

		public static PropertyInfo GetProperty(this Type type, string name)
		{
			var properties = GetProperties(type, BindingFlags.Public | BindingFlags.FlattenHierarchy);

			return properties.Find(x => x.Name == name);
		}

		public static List<MethodInfo> GetMethods(this Type type)
		{
			return GetMethods(type, BindingFlags.FlattenHierarchy | BindingFlags.Public);
		}

		public static List<MethodInfo> GetMethods(this Type type, BindingFlags flags)
		{
			var methods = type.GetTypeInfo().DeclaredMethods;
			if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy)
			{
				methods = type.GetRuntimeMethods();
			}

			List<MethodInfo> list = new List<MethodInfo>();
			foreach (var method in methods)
			{
				var publicTest = (flags & BindingFlags.Public) != BindingFlags.Public || method.IsPublic;
				var instanceTest = (flags & BindingFlags.Instance) != BindingFlags.Instance || !method.IsStatic;
				var staticTest = (flags & BindingFlags.Static) != BindingFlags.Static || method.IsStatic;

				if (publicTest && instanceTest && staticTest)
					list.Add(method);
			}
			return list;
		}

		public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
		{
			var methods = GetMethods(type, flags);

			return methods.Find(x => x.Name == name);
		}

		public static MethodInfo GetMethod(this Type type, string name)
		{
			var methods = GetMethods(type, BindingFlags.Public | BindingFlags.FlattenHierarchy);
			return methods.Find(x => x.Name == name);
		}

		public static List<ConstructorInfo> GetConstructors(this Type type, BindingFlags flags)
		{
			var ctors = type.GetTypeInfo().DeclaredConstructors;

			List<ConstructorInfo> list = new List<ConstructorInfo>();
			foreach (var ctor in ctors)
			{
				if (ctor.IsPublic)
				{
					var publicTest = (flags & BindingFlags.Public) != BindingFlags.Public || ctor.IsPublic;
					var instanceTest = (flags & BindingFlags.Instance) != BindingFlags.Instance || !ctor.IsStatic;
					var staticTest = (flags & BindingFlags.Static) != BindingFlags.Static || ctor.IsStatic;

					if (publicTest && instanceTest && staticTest)
						list.Add(ctor);
				}
			}

			return list;
		}

		public static List<FieldInfo> GetFields(this Type type)
		{
			return GetFields(type, BindingFlags.Public | BindingFlags.FlattenHierarchy);
		}

		public static List<FieldInfo> GetFields(this Type type, BindingFlags flags)
		{
			var fields = type.GetTypeInfo().DeclaredFields;
			if ((flags & BindingFlags.FlattenHierarchy) == BindingFlags.FlattenHierarchy)
			{
				fields = type.GetRuntimeFields();
			}

			List<FieldInfo> list = new List<FieldInfo>();
			foreach (var field in fields)
			{
				var publicTest = (flags & BindingFlags.Public) != BindingFlags.Public || field.IsPublic;
				var instanceTest = (flags & BindingFlags.Instance) != BindingFlags.Instance || !field.IsStatic;
				var staticTest = (flags & BindingFlags.Static) != BindingFlags.Static || field.IsStatic;

				if (publicTest && instanceTest && staticTest)
					list.Add(field);
			}
			return list;
		}

		public static FieldInfo GetField(this Type type, string name, BindingFlags flags)
		{
			var fields = GetFields(type, flags);

			return fields.Find(x => x.Name == name);
		}

		public static FieldInfo GetField(this Type type, string name)
		{
			var fields = GetFields(type, BindingFlags.Public | BindingFlags.FlattenHierarchy);

			return fields.Find(x => x.Name == name);
		}

		public static Type[] GetGenericArguments(this Type type)
		{
			return type.GenericTypeArguments;
		}
	}
}
```
# Test results

The test has been done on a real device (Xiaomi 4c).

| Test    | No Linq Nor Yield | Using Yield | Using Linq |
|:-------:|:----------:|:-----------:|:-----------------:|
|Test|1336|1321|1394|
|Test|1283|1237|1311|
|Test|1332|1301|1390|
|Test|1331|1350|1424|
|Test|1412|1378|1513|
|Test|1448|1445|1608|
|Test|1575|1515|1565|
|Test|1557|1507|1695|
|Test|1576|1547|1656|
|Test|1732|1730|1655|
|Test|1634|1823|1726|
|Test|1664|1652|1788|
|Test|1751|1705|1788|
|Test|1749|1762|1820|
|Test|1791|1772|1888|
|Test|2623|2383|2659|
|Test|1793|1935|2020|
|Test|1860|1900|2069|
|Test|2188|1886|1966|
|Test|1924|2092|2005|
|Test|1986|2162|2179|
|    |    |
|*Average*|1692,619048|1685,857143|1767,571429|

Testing on a real device a measuring time using MvxStopWatch shows a slight improvement on the inflation time for a huge linear layout without using linq.
In this particular test. the difference in performance between the method using yield return and the other one using lists isn't relevant a the winner is the one with yield.
Some further performance testing is needed here since most of the time I've better performance without yield return than in this particular thest.
We can expect an even more improvement with nested layouts.