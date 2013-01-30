
Xamarin.Social enables you to post text and other media to social networks and access their API using authenticated requests.

With Xamarin.Social, you can easily:

1. Share text and images to social networks
2. Access social network APIs using authenticated requests
3. Automatically and securely store user credentials

Xamarin.Social works with these social networks:

* [App.net](http://alpha.app.net)
* [Facebook](http://facebook.com)
* [Flickr](http://www.flickr.com)
* [Pinterest](http://pinterest.com)
* [Twitter](http://twitter.com)

An example for sharing a link with Facebook on iOS:

```csharp
using Xamarin.Social;
using Xamarin.Social.Services;
...

public override void ViewDidAppear (bool animated)
{
	base.ViewDidAppear (animated);

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
}
```

*Some screenshots assembled with [PlaceIt](http://placeit.breezi.com/).*