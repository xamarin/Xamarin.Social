/*
	http://cakebuild.net/docs
*/
#addin "Cake.Xamarin"
#addin "Cake.XCode"

#load "./common.cake"

var TARGET = Argument ("t", Argument ("target", "Default"));

BuildSpec buildSpec = new BuildSpec 
{
	Libs = new ISolutionBuilder [] 
	{
		// trigger Xamarin.Auth compilation to restore nuget packages
		new DefaultSolutionBuilder 
		{
			//SolutionPath = WIN_SLN_PATH,
			SolutionPath = "./externals/Xamarin.Auth/source/Xamarin.Auth-Library-MacOSX-Xamarin.Studio.sln",
			BuildsOn = BuildPlatforms.Mac | BuildPlatforms.Windows,
		},
		new DefaultSolutionBuilder 
		{
			SolutionPath = "./source/references01projects/Xamarin.Social-Xamarin.Studio-MacOSX.sln",
			Targets = new [] 
			{ 
				"Xamarin_Social_Portable",
			},
			BuildsOn = BuildPlatforms.Mac | BuildPlatforms.Windows,
			OutputFiles = new [] 
			{
				new OutputFileCopy 
				{
					FromFile = "./source/references01projects/Xamarin.Social.Portable/bin/Release/Xamarin.Social.dll",
					ToDirectory= "./output/pcl/"
				},
			}
		},
		new DefaultSolutionBuilder 
		{
			SolutionPath = "./source/references01projects/Xamarin.Social-Xamarin.Studio-MacOSX.sln",
			Targets = new [] { "Xamarin_Social_XamarinAndroid" },
			OutputFiles = new [] 
			{
				new OutputFileCopy
				{
					FromFile = "./source/references01projects/Xamarin.Social.XamarinAndroid/bin/Release/Xamarin.Social.dll",
					ToDirectory= "./output/android/"
				},
			}
		},
		new IOSSolutionBuilder
		{
			// everything on MacOSX with Xamarin.Studio
			SolutionPath = "./source/references01projects/Xamarin.Social-Xamarin.Studio-MacOSX.sln",
			OutputFiles = new []
			{
				new OutputFileCopy
				{
					FromFile = "./source/references01projects/Xamarin.Social.XamarinAndroid/bin/Release/Xamarin.Social.dll",
					ToDirectory= "./output/android/"
				},
				new OutputFileCopy
				{
					FromFile = "./source/references01projects/Xamarin.Social.XamarinIOS/bin/Release/Xamarin.Social.dll",
					ToDirectory= "./output/ios-unified/"
				},
			}
		},
		/*
		new IOSSolutionBuilder
		{
			// everything on MacOSX with Xamarin.Studio
			SolutionPath = "./source/references02nuget/Xamarin.Social.sln",
		},
		new DefaultSolutionBuilder
		{
			// everything on MacOSX with Xamarin.Studio
			SolutionPath = "./source/ReferencesNuget/Xamarin.Social.sln",
		},
		*/
	},
	Samples = new ISolutionBuilder []
	{
		new IOSSolutionBuilder 
		{
			SolutionPath = @"./samples/Traditional.Standard/references01projects/old-for-backward-compatiblity/Xamarin.Social.Samples.sln"
		},
		new DefaultSolutionBuilder 
		{
			SolutionPath = @"./samples/Traditional.Standard/references01projects/old-for-backward-compatiblity/Xamarin.Social.Samples.sln"
		},
	},
	NuGets = new [] 
	{
		new NuGetInfo 
		{ 
			NuSpec = "./nuget/Xamarin.Social.nuspec"
		},
	},	
};

//DefineDefaultTasks ();
SetupXamarinBuildTasks (buildSpec, Tasks, Task);

Task ("ci-osx")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget");

RunTarget (TARGET);
