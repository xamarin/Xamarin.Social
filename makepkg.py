#!/usr/bin/env python

import subprocess
import sys

XPKG = sys.argv[1]
OUTPUT = sys.argv[2]

#
# BUILD
#
def mdbuild (solution_path):
	xbuild_args = [
		"/Applications/MonoDevelop.app/Contents/MacOS/mdtool",
		"build",
		"--configuration:Release",
		solution_path,
	]
	err = subprocess.call (xbuild_args)
	if err:
		raise Exception ("MDTOOL RETURNED %s" % err)


mdbuild ("src/Xamarin.Social.iOS/Xamarin.Social.iOS.sln")
mdbuild ("src/Xamarin.Social.Android/Xamarin.Social.Android.sln")

#
# PACKAGE
#
xpkg_args = [
	"mono",
	XPKG,
	"create",
	OUTPUT,
	"--details", "addon/Details.md",
	"--license", "addon/License.md",
	"--getting-started", "addon/GettingStarted.md",
	"--publisher", "Xamarin",
	"--publisher-url", "http://xamarin.com",
	"--summary", "Flexible social features for your app.",
	"--icon", "icons/Xamarin.Social_512x512.png",
	"--library", "ios:src/Xamarin.Social.iOS/bin/Release/Xamarin.Social.iOS.dll",
	"--library", "android:src/Xamarin.Social.Android/bin/Release/Xamarin.Social.Android.dll",
	"--sample", "Xamarin.Social Sample (Android). Xamarin.Social Sample (Android).:samples/Xamarin.Social.Sample.Android/Xamarin.Social.Sample.Android.sln",
	"--sample", "Xamarin.Social Sample (iOS). Xamarin.Social Sample (iOS).:samples/Xamarin.Social.Sample.iOS/Xamarin.Social.Sample.iOS.sln",
]
subprocess.call (xpkg_args)
