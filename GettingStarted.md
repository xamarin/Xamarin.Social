# Getting Started

Xamarin.Social helps you post status updates and other media to social
networks, and to access social APIs in general via authenticated
requests.


## 1. Create and configure the service

Access social networks by creating `Service` objects:

```csharp
var facebook = new FacebookService {
	ClientId = "<App ID from http://developers.facebook.com/apps>"
};
```

Xamarin.Social comes with a variety of services:

<table>
	<thead><tr><th>Service</th><th>Class</th><th>Parameters</th><th>Developer Info</th></thead>
	<tbody>
		<tr>
			<td>App.net</td>
			<td><code>AppDotNetService</code></td>
			<td><code>ClientId</code>, <code>RedirectUrl</code></td>
			<td><a href="https://alpha.app.net/developer/apps/">App.net apps</a></td>
		</tr>
		<tr>
			<td>Facebook</td>
			<td><code>FacebookService</code></td>
			<td><code>ClientId</code></td>
			<td><a href="http://developers.facebook.com">Facebook Developers</a></td>
		</tr>
		<tr>
			<td>Flickr</td>
			<td><code>FlickrService</code></td>
			<td><code>ConsumerKey</code>, <code>ConsumerSecret</code></td>
			<td><a href="http://www.flickr.com/services/api/">Flickr Services</a></td>
		</tr>
		<tr>
			<td>Twitter</td>
			<td><code>TwitterService</code></td>
			<td><code>ConsumerKey</code>, <code>ConsumerSecret</code></td>
			<td><a href="http://dev.twitter.com">Twitter Developers</a></td>
		</tr>
		<tr>
			<td>Twitter</td>
			<td><code>Twitter5Service</code>*</td>
			<td>none</td>
			<td><a href="http://dev.twitter.com">Twitter Developers</a></td>
		</tr>
	</tbody>
</table>

\* uses iOS 5-specific UI and account settings.


## 2. Authenticate the user

You need an `Account` object to communicate with a `Service`.  Your
`Service` object's `GetAuthenticateUI` method creates the UI that you
will present to authenticate the user, and gives you access to an
`Account` object once the user has authenticated:

```csharp
var authenticateViewController = facebook.GetAuthenticateUI (account => {
	// This is called after the user has authenticated,
	// or they chose to cancel (account will be null in that case).
	DismissViewController (true);
});
PresentViewController (authenticateViewController, true, null);
```

On Android, `GetAuthenticateUI` returns an `Intent`:

```csharp
var authenticateIntent = facebook.GetAuthenticateUI (this, account => {
	// do something wonderful with the account
});
StartActivityForResult (authenticateIntent, 42);
```

Accounts are automatically saved for you using the secure `SecKeyChain`
on iOS and `KeyStore` on Android.

You can retrieve saved accounts with `GetAccountsAsync`:

```csharp
facebook.GetAccountsAsync ().ContinueWith (accounts => {
	// accounts is an IEnumerable<Account> of saved accounts
});
```

## 3. Share a little something

To share some text, links, or images, fill out an `Item` object and call
`GetShareUI`:

```csharp
var item = new Item {
	Text = "This is the best library I've ever used!",
};
item.Images.Add (imageOfACat);
item.Links.Add (new Uri ("http://xamarin.com"));

var shareViewController = facebook.GetShareUI (item, result => {
	// result lets you know if they went through with it or canceled
	DismissViewController (true);
});
PresentViewController (shareViewController, true, null);
```

On Android, `GetShareUI` will give you an intent:

```csharp
var shareIntent = facebook.GetShareUI (this, item, result => {
	// congratulate the user for being awesome
});
StartActivityForResult (shareIntent, 42);
```

The share UI will allow the user to select the account that they want to
use so you don't need to provide one. The UI will also allow the user to
edit the item's text before it is posted.

Not all services are able to share images, and some (crazy) services
limit the amount of text you can post. Use these properties of the
service to find out about such limitations:

* `MaxTextLength`
* `MaxLinks`
* `MaxImages`
* `MaxFiles`


## 4. Call the API

If you want to do more than just share, you can access the API using
request objects from the service:

```csharp
var request = facebook.CreateRequest (
	"GET",
	new Uri ("https://graph.facebook.com/me/feed"),
	account);
request.GetResponseAsync ().ContinueWith (response => {
	// parse the JSON in response.GetResponseText ()
});
```

Notice how the service automatically authenticates the request for you.
You're welcome.


## 5. Make your own Service

If you want to access a service not covered by this API, fear not, it's
extensible! It's very easy to create your own services. Check out
[Details](Details.md) for details.

