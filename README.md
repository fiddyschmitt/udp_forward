# udpforward

Convert between unicast, multicast and broadcast packets.

## Download

Portable executables for Windows, Linux and Mac can be found over in the [releases](https://github.com/fiddyschmitt/udp_forward/releases/latest) section.

## Example 1

This example listens to unicast packets and forwards them to two unicast destinations.

`udpforward.exe --config example1.json`

```
{
  "Forwards": [
    {
      "Bidirectional": true,
      "Listeners": [
        "192.168.56.1:11000"
      ],
      "Senders": [
        {
          "SenderFromAddress": "192.168.56.1:11000",
          "Destinations": [ "192.168.56.120:13000" ]
        },
        {
          "SenderFromAddress": "127.0.0.1:11000",
          "Destinations": [ "127.0.0.1:5000" ]
        }
      ]
    }
  ]
}
```

## Example 2

This example receives multicast packets and forwards them to a unicast destination.

`udpforward.exe --config example2.json`

```
{
	"Forwards": [
		{
			"Bidirectional": false,
			"DedupeWindowMilliseconds": 5000,
			"Listeners": [
				"192.168.1.31:11000"
			],
			"JoinMulticastGroups": [
				"239.0.0.1"
			],
			"Senders": [
				{
					"SenderFromAddress": "192.168.56.1:12000",
					"Destinations": [ "192.168.56.120:13000" ]
				}
			]
		}
	]
}
```
