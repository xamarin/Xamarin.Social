


# Features

* Post with Compose GUI
* Post without GUI
* Use native API
* Credential/account storage
* Offline storage until can work

Item types:

* Text
* Link
* Image
* File
* Video?


# Services

Share OS Support:

<table>
    <tr><th>Service/OS</th><th>&lt;iOS4</th><th>iOS5</th><th>iOS6</th><th>Android</th><th>WP7</th></tr>
    <tr><th>Facebook</th><td></td><td></td><td>X</td><td></td><td></td></tr>
    <tr><th>Twitter</th><td></td><td>X</td><td>X</td><td></td><td></td></tr>
    <tr><th>Google+</th><td></td><td></td><td></td><td></td><td></td></tr>
</table>


## Google+

* [API Reference](https://developers.google.com/+/api/latest/)


## API

### Share text

    Service.Facebook.ShareAsync (new Item ("Hello, World."));

# Existing APIs

## ShareKit

* [SHKItem](http://getsharekit.com/docs/#item)

### Share text with selector

    SHKItem *item = [SHKItem text:@"Hello, World."];
    SHKActionSheet *actionSheet = [SHKActionSheet actionSheetForItem:item];
    [actionSheet showFromToolbar:navigationController.toolbar];

## iOS

* [SLRequest](https://developer.apple.com/library/prerelease/ios/#documentation/Social/Reference/SLRequest_Class/Reference/Reference.html#//apple_ref/doc/uid/TP40012234)
* [SLComposeViewController](https://developer.apple.com/library/prerelease/ios/#documentation/NetworkingInternet/Reference/SLComposeViewController_Class/Reference/Reference.html#//apple_ref/doc/uid/TP40012205)
* [ACAccount](https://developer.apple.com/library/ios/#documentation/Accounts/Reference/ACAccountClassRef/Reference/Reference.html)

### Share text

    var vc = SLComposeViewController.ComposeViewControllerForServiceType (SLServiceType.Facebook);
    vc.InitialText = "Hello, World.";
    vc.ShowModal ();

## WinRT

* [Windows.Security.Authentication.Web](http://msdn.microsoft.com/en-us/library/windows/apps/windows.security.authentication.web.aspx)

