## Sharing

To share an item: create it, create the service, then present the share UI:

```csharp
using Xamarin.Social;
using Xamarin.Social.Services;
...

// 1. Create the service
var facebook = new FacebookService {
	ClientId = "<App ID from http://developers.facebook.com/apps>"
};

// 2. Create an item to share
var item = new Item {
	Text = "This is the best library I've ever used!",
};
item.Links.Add (new Uri ("http://xamarin.com"));

// 3. Present the UI on iOS
var shareViewController = facebook.GetShareUI (item, result => {
	// result lets you know if they went through with it or canceled
	DismissViewController (true, null);
});
PresentViewController (shareViewController, true, null);
```

If you're on Android, then you should present the share UI using an intent:

```csharp
// 3. Present the UI on Android
var shareIntent = facebook.GetShareUI (this, item, result => {
	// congratulate the user for being awesome
});
StartActivityForResult (shareIntent, 42);
```


## Services

Xamarin.Social comes with a variety of services that you can use to share items. Each service requires a certain number of parameters to be filled in by you:

* [App.net](https://alpha.app.net/developer/apps/) `new AppDotNetService { ClientId }`
* [Facebook](http://developers.facebook.com) `new FacebookService { ClientId }`
* [Flickr](http://www.flickr.com/services/api/) `new FlickrService { ConsumerKey, ConsumerSecret }`
* [Pinterest](http://pinterest.com) `new PinterestService ()`
* [Twitter](http://dev.twitter.com)<sup>*</sup> `new TwitterService { ConsumerKey, ConsumerSecret }`
* [Twitter](http://dev.twitter.com)<sup>5</sup> `new Twitter5Service ()`

<sup>*</sup> make sure that your Twitter app has an authentication callback URL registered in Twitter's app settings.

<sup>5</sup> uses native iOS 5 user interfaces and accounts (only available on iOS 5 and newer).





## Share Items

To share some text, links, or images, fill out an `Item` object and call `GetShareUI`. The share UI will allow the user to select the account that they want to use so you don't need to provide one. The UI will also allow the user to edit the item's text before it is posted.

Items have properties for Text, Images, Files, and Links; however, not all services support sharing all those types of media. Use these properties of `Service` objects to find out about such limitations:

* `MaxTextLength`
* `MaxLinks`
* `MaxImages`
* `MaxFiles`

In addition to presenting the Share UI, you can also share items directly using the `ShareItemAsync` method of the service.



## Social APIs

If you want to do more than just share, you can access the API using request objects from the service:

```csharp
var request = facebook.CreateRequest (
	"GET",
	new Uri ("https://graph.facebook.com/me/feed"),
	account);
request.GetResponseAsync ().ContinueWith (response => {
	// parse the JSON in response.GetResponseText ()
});
```

Notice how the service automatically authenticates the request for you. You're welcome.



## Authentication

Xamarin.Social uses the Xamarin.Auth library to fetch and store `Account` objects. 

Each service exposes a `GetAuthenticateUI` method that returns a `Xamarin.Auth.Authenticator` object that you can use to authenticate the user. Doing so will automatically store the authenticated account so that it can be used later.

You can retrieve stored accounts with `GetAccountsAsync`:

```csharp
facebook.GetAccountsAsync ().ContinueWith (accounts => {
	// accounts is an IEnumerable<Account> of saved accounts
});
```