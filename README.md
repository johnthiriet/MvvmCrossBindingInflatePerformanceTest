# MvvmCrossBindingInflatePerformanceTest
Performance test for MvvmCross BindingInflate on Android

This repository is related to issue [#1342](https://github.com/MvvmCross/MvvmCross/issues/1342) on MvvmCross.

This test uses a modified local version of MvvmCross printing the time used by the BindingInflate.

It consist on only onw activity that is opened and closed 14 times.

The SetContentView method of MvxActivity has been modified.
{% highlight C# %}
public override void SetContentView(int layoutResId)
{
    View view;
    var dt = DateTime.Now;
    view = this.BindingInflate(layoutResId, null);
    var dt2 = DateTime.Now;

    Mvx.Trace(MvxTraceLevel.Diagnostic, "Binding Inflate Duration {0}", (dt2 - dt).TotalMilliseconds);
    this.SetContentView(view);
}
{% endhighlight %}

I created a simple version of the MvvmCross/Platform/Platform/ReflectionExtensions.cs source file using neither Linq nor Yield

{% highlight C# %}
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
{% endhighlight %}

This version probably needs a little bit more testing and tweaking.

Here are the first performance results.


| Test    | Using Linq | Using Yield | No Linq Nor Yield |
|:-------:|:----------:|:-----------:|:-----------------:|
| Test    | 468,544    | 487,692     | 491,53            |
| Test    | 771,512    | 978,187     | 1066,50           |
| Test    | 840,576    | 1031,439    | 1172,74           |
| Test    | 939,418    | 877,451     | 863,29            |
| Test    | 775,485    | 1123,052    | 950,56            |
| Test    | 1011,873   | 1087,925    | 1001,25           |
| Test    | 1144,127   | 1282,016    | 847,25            |
| Test    | 983,441    | 1082,459    | 1015,64           |
| Test    | 794,55     | 859,534     | 851,29            |
| Test    | 930,924    | 998,959     | 973,28            |
| Test    | 969,917    | 1026,068    | 967,62            |
| Test    | 1026,292   | 976,693     | 760,00            |
| Test    | 869,074    | 822,47      | 1066,36           |
| Test    | 1051,532   | 962,377     | 995,43            |
| Test    | 1008,592   | 1221,89     | 1119,32           |
| *Total* | *13585,86* | *14818,21*  | *14142,05*        |
|*Average*| *905,72*   | *987,88*    | *942,80*          |

#Results

On this simple project the test tends to show that Linq may be a little faster than other method in Debug mode on a simulator.
Some more test on real apps are necessary to confirm that because it goes against all known fact that Linq tends to be slower.
