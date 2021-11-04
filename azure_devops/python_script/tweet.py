import oath2
def tweet(text):
	url = "https://api.twitter.com/1.1/statuses/update.json?status={}".format(text)

	consumer = oauth2.Consumer(key='$(APIKey)', secret='$(APIKeySecret)')
	token = oauth2.Token(key='$(AccessToken)', secret='$(AccessTokenSecret)')
	client = oauth2.Client(consumer, token)
	resp, content = client.request( url, method="POST")

	return content

tweet("https://github.com/dhq-boiler/boiler-s-Graphics/releases/tag/$(tag)")