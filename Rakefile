require "rake/clean"

CLEAN.include "*.xam"
CLEAN.include ".xpkg"

file ".xpkg/xamarin-component.exe" do
	puts "* Downloading xpkg..."
	mkdir "xpkg"
	sh "curl -L https://components.xamarin.com/submit/xpkg > xpkg.zip"
	sh "unzip -q -o xpkg.zip -d .xpkg"
	sh "rm xpkg.zip"
end

task :default => ".xpkg/xamarin-component.exe" do
	line = <<-END
	mono .xpkg/xamarin-component.exe package
		END
	puts "* Creating Xamarin.Social Component"
	puts line.strip.gsub "\t\t", "\\\n    "
	sh line, :verbose => false
	puts "* Created Xamarin.Social Component"
end

