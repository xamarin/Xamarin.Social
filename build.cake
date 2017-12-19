/*
#########################################################################################
Installing

	Windows - powershell
		
        Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        .\build.ps1

	Windows - cmd.exe prompt	
	
        powershell ^
			Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        powershell ^
			.\build.ps1
	
	Mac OSX 

        rm -fr tools/; mkdir ./tools/ ; \
        cp cake.packages.config ./tools/packages.config ; \
        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/osx ; \
        chmod +x ./build.sh ;
        ./build.sh

	Linux

        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/linux
        chmod +x ./build.sh && ./build.sh

Running Cake to Build Xamarin.Auth targets

	Windows

		tools\Cake\Cake.exe --verbosity=diagnostic --target=libs
		tools\Cake\Cake.exe --verbosity=diagnostic --target=nuget
		tools\Cake\Cake.exe --verbosity=diagnostic --target=samples

		tools\Cake\Cake.exe -experimental --verbosity=diagnostic --target=libs
		tools\Cake\Cake.exe -experimental --verbosity=diagnostic --target=nuget
		tools\Cake\Cake.exe -experimental --verbosity=diagnostic --target=samples
		
	Mac OSX 
	
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=libs
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=nuget
		
NuGet Publish patterns

		BEFORE PASTING:
		NOTE: ** / 
		** /output/Xamarin.Auth.1.5.0-alpha-12.nupkg,
		** /output/Xamarin.Auth.XamarinForms.1.5.0-alpha-12.nupkg,
		** /output/Xamarin.Auth.Extensions.1.5.0-alpha-12.nupkg
		

#########################################################################################
*/	
#addin nuget:?package=Cake.Xamarin
#addin nuget:?package=Cake.Xamarin.Build
#addin nuget:?package=Cake.FileHelpers
#tool nuget:?package=vswhere


var TARGET = Argument ("t", Argument ("target", "Default"));

Task ("externals-cake-build")
	.Does 
	(
		() => 
		{
			// Xamarin.Auth preparation
			CopyDirectory("./tools/", "./externals/Xamarin.Auth/tools/");
			
			CakeExecuteScript
						(
							"./externals/Xamarin.Auth/build.cake", 
							new CakeSettings
							{ 
								Arguments = new Dictionary<string, string>()
								{
									{"target", "libs"},
									{"verbosity", "diagnostic"},
								}
							}
						);
		}
	);

Task ("nuget-restore")
	.Does 
	(
		() => 
		{
			FilePathCollection solutions = null;
			
			solutions = GetFiles("./externals/Xamarin.Auth/source/**/*.sln");
			foreach (FilePath source_solution in solutions)
			{
				NuGetRestore
					(
						source_solution, 
						new NuGetRestoreSettings 
						{ 
							Verbosity = NuGetVerbosity.Detailed,
						}
					); 
			}

			solutions = GetFiles("./source/**/*.sln");
			foreach (FilePath source_solution in solutions)
			{
				NuGetRestore
					(
						source_solution, 
						new NuGetRestoreSettings 
						{ 
							Verbosity = NuGetVerbosity.Detailed,
						}
					); 
			}
			
			solutions = GetFiles("./samples/**/*.sln");
			foreach (FilePath source_solution in solutions)
			{
				NuGetRestore
					(
						source_solution, 
						new NuGetRestoreSettings 
						{ 
							Verbosity = NuGetVerbosity.Detailed,
						}
					); 
			}
			
			return;
		}
	);

var buildSpec = new BuildSpec () 
{
	Libs = new [] 
	{
		new DefaultSolutionBuilder 
		{
			SolutionPath = "./source/Xamarin.Social.sln",
			OutputFiles = new [] 
			{ 
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.Social.Portable/bin/Release/Xamarin.Social.dll",
					ToDirectory = "./output/pcl/"
				},
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.Social.XamarinAndroid/bin/Release/Xamarin.Social.dll",
					ToDirectory = "./output/android/",
				},
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.Social.XamarinIOS/bin/Release/Xamarin.Social.dll",
					ToDirectory = "./output/ios-unified/",
				},
			}
		},	
		new DefaultSolutionBuilder 
		{
			SolutionPath = "./source/Xamarin.Social-Xamarin.Studio-MacOSX.sln",
			OutputFiles = new [] 
			{ 
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.Social.Portable/bin/Release/Xamarin.Social.dll" 
				},
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.Social.XamarinAndroid/bin/Release/Xamarin.Social.dll" 
				},
				new OutputFileCopy 
				{ 
					FromFile = "./source/Xamarin.Social.XamarinIOS/bin/Release/Xamarin.Social.dll" 
				},
			}
		},	
	},

	// Samples = new [] 
	// {
	// 	new DefaultSolutionBuilder 
	// 	{ 
	// 		SolutionPath = "samples/Stripe.UIExamples/Stripe.UIExamples.sln",  
	// 		Configuration = "Release" 
	// 	},
	// },

	NuGets = new [] 
	{
		new NuGetInfo 
		{ 
			NuSpec = "./nuget/Xamarin.Social.nuspec", 
		},
	},
};

Task ("externals")
	.IsDependentOn ("externals-base")
	//.WithCriteria (!FileExists ("./externals/stripe-android.aar"))
	.Does 
	(
		() =>
		{
			EnsureDirectoryExists ("./externals");
			
			// DownloadFile (ANDROIDURL, "./externals/stripe-android.aar");
			// DownloadFile (ANDROIDPAYURL, "./externals/stripe-android-pay.aar");
			// 
			// DownloadFile (ANDROIDDOCSURL, "./externals/stripe-android-javadoc.jar");
			// Unzip ("./externals/stripe-android-javadoc.jar", "./externals/stripe-android-javadoc/");
			// 
			// DownloadFile (ANDROIDPAYDOCSURL, "./externals/stripe-android-pay-javadoc.jar");
			// Unzip ("./externals/stripe-android-pay-javadoc.jar", "./externals/stripe-android-pay-javadoc/");
		}
	);

Task ("clean")
	.IsDependentOn ("clean-base")
	.Does 
	(
		() =>
		{
			if (DirectoryExists ("./externals"))
			{
				DeleteDirectory ("./externals", true);
			}
		}
	);

//=================================================================================================
// Put those 2 CI targets at the end of the file after all targets
// If those targets are before 1st RunTarget() call following error occusrs on 
//		*	MacOSX under Mono
//		*	Windows
// 
//	Task 'ci-osx' is dependent on task 'libs' which do not exist.
//
// Xamarin CI - Jenkins job targets
Task ("ci-osx")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    //.IsDependentOn ("samples")
	;
Task ("ci-windows")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    //.IsDependentOn ("samples")
	;	
//=================================================================================================

SetupXamarinBuildTasks (buildSpec, Tasks, Task);

RunTarget("externals-cake-build");
RunTarget (TARGET);
