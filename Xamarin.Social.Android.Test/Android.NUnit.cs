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
using Android.Graphics;

namespace Android.NUnit
{
	[Activity (Label = "TestRunner", MainLauncher = true)]
	public class TestRunner : ListActivity
	{
		public static TestRunner Shared { get; private set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Shared = this;

			ListAdapter = new FixtureAdapter (
				this,
				from t in Assembly.GetExecutingAssembly ().GetTypes ()
				where t.GetCustomAttributes (typeof (global::NUnit.Framework.TestFixtureAttribute), true).Length > 0
				orderby t.FullName
				select t);

			ListView.ItemClick += (s, e) => {
				var f = ((FixtureAdapter)ListAdapter)[e.Position];
				var i = new Intent (this, typeof(FixtureActivity));
				i.PutExtra ("FixtureFullName", f.FullName);
				StartActivity (i);
			};
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

			public Type this[int position] { get { return fixtures[position]; } }

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
					tv.SetTextSize (Android.Util.ComplexUnitType.Sp, 36);
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

	[Activity (Label = "Fixture", MainLauncher = true)]			
	public class FixtureActivity : ListActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			var fixture = Assembly.GetExecutingAssembly ().GetType (Intent.GetStringExtra ("FixtureFullName"));

			ListAdapter = new TestAdapter (
				this,
				from t in fixture.GetMethods ()
				where t.GetCustomAttributes (typeof (global::NUnit.Framework.TestAttribute), true).Length > 0
				orderby t.Name
				select t);

			ListView.ItemClick += (s, e) => {
				RunTest (((TestAdapter)ListAdapter)[e.Position]);
			};
		}

		void RunTest (MethodInfo test)
		{
			try {
				var o = Activator.CreateInstance (test.DeclaringType);
				test.Invoke (o, null);
			}
			catch (TargetInvocationException ex) {
				ShowError (ex.InnerException);
			}
		}

		void ShowError (Exception ex)
		{
			var b = new AlertDialog.Builder (this);
			b.SetMessage (ex.ToString ());
			b.SetTitle (ex.GetType ().Name);
			var alert = b.Create ();
			alert.Show ();
		}

		class TestAdapter : BaseAdapter
		{
			FixtureActivity activity;
			List<MethodInfo> tests;

			public TestAdapter (FixtureActivity activity, IEnumerable<MethodInfo> tests)
			{
				this.activity = activity;
				this.tests = tests.ToList ();
			}

			public MethodInfo this[int position] { get { return tests[position]; } }

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
					tv.SetTextSize (Android.Util.ComplexUnitType.Sp, 36);
				}

				var t = tests[position];

				tv.Text = t.Name;

				return tv;
			}

			public override int Count {
				get {
					return tests.Count;
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
		public static void IsTrue (bool condition)
		{
			if (!condition) {
				throw new AssertionException ("Expected <true> but was <false>");
			}
		}

		public static void AreEqual (object expected, object value)
		{
			if (!expected.Equals (value)) {
				throw new AssertionException ("Expected <" + expected + "> but was <" + value + ">");
			}
		}

		public static void NotNull (object value)
		{
			if (value == null) {
				throw new AssertionException ("Expected not null but was null");
			}
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

