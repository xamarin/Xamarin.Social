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


mdbuild ("src/Xamarin.Auth.iOS/Xamarin.Auth.iOS.sln")
mdbuild ("src/Xamarin.Auth.Android/Xamarin.Auth.Android.sln")

#
# PACKAGE
#
xpkg_args = [
	"mono",
	XPKG,
	"create",
	OUTPUT,
	"--details", "Details.md",
	"--license", "License.md",
	"--getting-started", "GettingStarted.md",
	"--publisher", "Xamarin",
	"--publisher-url", "http://xamarin.com",
	"--summary", "Authenticate against services like Facebook and Microsoft Live with this unified general purpose library.",
	"--icon", "icons/Xamarin.Auth_512x512.png",
	"--library", "ios:src/Xamarin.Auth.iOS/bin/Release/Xamarin.Auth.iOS.dll",
	"--library", "android:src/Xamarin.Auth.Android/bin/Release/Xamarin.Auth.Android.dll",
	"--sample", "Xamarin.Auth Sample (Android). Xamarin.Auth Sample (Android).:samples/Xamarin.Auth.Sample.Android/Xamarin.Auth.Sample.Android.sln",
	"--sample", "Xamarin.Auth Sample (iOS). Xamarin.Auth Sample (iOS).:samples/Xamarin.Auth.Sample.iOS/Xamarin.Auth.Sample.iOS.sln",
]
subprocess.call (xpkg_args)
