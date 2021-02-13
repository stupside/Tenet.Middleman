# Tenet.Middleman

This project is exclusivly made for the auth service [TENET.OOO](https://app.tenet.ooo).

This project acts as a middleman between the default auth and the clients. 
It's delivered as is and will allow you to stream content to the client by using the library delivered when you registered to the service.
If you want to use docker, I provided a sample, which is configured to work on an alpine by default.

You can obviously tweak the solution to your needs.

When you registered on the website, and you have created a new product you will might want to configure the middleman.

First start by adding a new configuration file for your product. 
## Create a test.json:
```json
  {
    "Pid": your_product_id,
    "Name": name_of_the_configuration,
    "Expiry": 5000,
    "Secret": a_super_secret,
    "Bytes": the_stream
  }
```

## Register the test.json in your appsettings by adding the path the the Streaming:Drivers
```
  "Streaming": {
    "Drivers": [
      "test.json"
    ]
  }
```

## Configure the client to work in per with the middleman:
```cpp
  std::string key;
  std::cout << "Key : ";
  std::cin >> key;

	tenet::Configuration config = tenet::Configuration()
		.with_endpoints(AUTH)
		.with_hardware(HARDWARE);

	tenet::Auth auth(PRODUCT_CODE, config);

	features::Authenticate response = auth.authenticate(key, 10);
	if (!response.succeed())
	{
		std::cout << response.message() << std::endl;
		return 0;
	}

	if (!auth.is_authenticated())
		return 0;

	features::Stream stream = auth.stream(response);
	if (!stream.succeed())
	{
		std::cout << stream.message() << std::endl;
	  return 0;
	}

	if (!stream.valid())
		return 0;

	decrypted_stream = stream.decrypt(STREAM_SECRET)
```
