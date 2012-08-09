# Xamarin.Social

A cross-platform API for accessing social networks.

Version 1 of this API focuses on getting authentication and sharing right. It also includes hooks to make API calls easier.

## Example uses

### Post to Twitter

    Service.Twitter.ShareAsync (new Item ("This is a great example!"))

### Get a user's timeline from Twitter

    var ps = new Dictionary<string,string> {
    	{ "screen_name", "theSeanCook" },
    	{ "count", "5" },
    	{ "include_entities", "1" },
    	{ "incode_rts", "1" },
    };
    var req = Service.Twitter.CreateRequest ("GET", new Uri ("http://api.twitter.com/1/statuses/user_timeline.json"), ps);
    var res = req.GetResponseAsync ().Result;
    var timeLineContent = new StreamReader (res.GetResponseStream ()).ReadToEnd ();

### Find all the services that images can be uploaded to

    var imageServices = from s in Service.GetServices ()
                        where s.CanShareImages
                        select s;

### Register a new service

    var pinterest = new PinterestService ();
    Service.RegisterService (pinterest);

### Find out if we have OS authentication

    var twitterIsReady = Service.Twitter.HasSavedAccounts;

### Authenticate and save a new account

    var pinterest = Service.GetService ("Pinterest");
    var account = pinterest.AddAccountAsync ().Result;

