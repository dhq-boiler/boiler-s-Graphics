from twitter import Twitter, OAuth

access_token = $(AccessToken)
access_token_secret = $(AccessTokenSecret)
api_key = $(APIKey)
api_secret = $(APIKeySecret)

t = Twitter(auth = OAuth(access_token, access_token_secret, api_key, api_secret))

text = 'https://github.com/dhq-boiler/boiler-s-Graphics/releases/tag/$(tag)'
statusUpdate = t.statuses.update(status=text)