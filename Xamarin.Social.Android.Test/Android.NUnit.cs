using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Reflection;

namespace Android.NUnit
{
	[Activity (Label = "TestRunner", MainLauncher = true)]			
	public class TestRunner : ListActivity
	{
		public static TestRunner Shared { get; private set; }

		List<Assembly> assemblies = new List<Assembly> ();

		public void Add (Assembly assembly)
		{
			assemblies.Add (assembly);
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Shared = this;

			Add (Assembly.GetExecutingAssembly ());

			ListAdapter = new FixtureAdapter (
				this,
				from a in assemblies
				from t in a.GetTypes ()
				where t.GetCustomAttributes (typeof (global::NUnit.Framework.TestFixtureAttribute), true).Length > 0
				orderby t.FullName
				select t);
		}

		class FixtureAdapter : BaseAdapter
		{
			TestRunner activity;
			List<Type> fixtures;

			public FixtureAdapter (TestRunner activity, IEnumerable<Type> fixtures)
			{
				this.activity = activity;
				this.fixtures = fixtures.ToList ();
			}

			public override Java.Lang.Object GetItem (int position)
			{
				return null;
			}

			public override long GetItemId (int position)
			{
				return 0;
			}

			public override View GetView (int position, View convertView, ViewGroup parent)
			{
				var tv = convertView as TextView;

				if (tv == null) {
					tv = new TextView (activity);
				}

				var f = fixtures[position];
				tv.Text = f.Name;

				return tv;
			}

			public override int Count {
				get {
					return fixtures.Count;
				}
			}
		}
	}
}

namespace NUnit.Framework
{
	public class TestFixtureAttribute : Attribute
	{
	}

	public class TestAttribute : Attribute
	{
	}

	public class Assert
	{
		public void IsTrue (bool condition)
		{
		}
	}

	public class AssertionException : Exception
	{
		public AssertionException (string message)
			: base (message)
		{
		}
	}
}

