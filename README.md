# Xamarin.Social

A cross-platform API for accessing social networks.

Version 1 of this API focuses on getting authentication and sharing right. It also includes hooks to make API calls easier.

## Example uses

### Post to Twitter

    Service.Twitter.ShareAsync (new Item ("This is a great example!"))

### Find all the services that images can be uploaded to

    var imageServices = from s in Service.GetServices ()
                        where s.CanShareImages
                        select s;

### Find out if we have OS authentication

    var twitterIsReady = Service.Twitter.OSAccount != null;

### Authenticate a different account if we don't have 

    var pinterest = Service.GetService ("Pinterest");
    var creds = pinterest.GetBlankCredentials ();
    // Force user to fill in creds
    var account = pinterest.AuthenticateAsync (creds).Result;
    pinterest.ShareAsync (new Item ("Is this really as elegant as possible?", account));

(Pinterest support is incomplete/nonexistent/will never happen)


