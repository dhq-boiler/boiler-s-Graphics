import oauth2 as oauth
def tweet(text):
	url = "https://api.twitter.com/1.1/statuses/update.json?status={}".format(text)

	consumer = oauth.Consumer(key='$(APIKey)', secret='$(APIKeySecret)')
	token = oauth.Token(key='$(AccessToken)', secret='$(AccessTokenSecret)')
	client = oauth.Client(consumer, token)
	resp, content = client.request( url, method="POST")

	return content

tweet("https://github.com/dhq-boiler/boiler-s-Graphics/releases/tag/$(tag)")