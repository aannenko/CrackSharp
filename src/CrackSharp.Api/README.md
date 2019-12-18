## CrackSharp.Api
Use code in this directory to build a .NET Core WebAPI service, capable of bruteforcing DES hashes using a specified charset and expected length of the word behind the hash. It can decrypt multiple hashes in parallel: it starts a new decryption task for each *unique* set of parameters and then each task periodically checks if the hash has already been decrypted by the other tasks.

From this repository OpenShift can create a container with a running web service inside using its S2I (Source-To-Image) tool.

### OpenShift S2I Deployment
0. *Prerequisite*: you should have a project in OpenShift, if you don't - create one
1. Go to OpenShift web console's Developer Catalog, select .NET Core item there
2. Select your project, then `dotnet:3.1`, give your new app a name in lower case and specify [this repo's address](https://github.com/aannenko/CrackSharp.git) as a source, tick the `Create route` checkbox and click `Create`
3. Wait for the app to deploy
4. Find your new route in OpenShift web console's Networking -> Routes menu, copy its address and test the app by opening `<route_address>/api/v1/des` in a browser.

### Docker Deployment
1. Clone this repository and `cd` to its root
2. `docker build -t crack-sharp .` - create a Docker image
3. Use one of the following commands:
- `docker run -it --rm -p 8080:80 --name crack-sharp crack-sharp` - this command runs a container with its console output redirected to your terminal and will remove container's files once it is stopped (useful for testing/debugging the app)
- `docker run -d -p 8080:80 --name crack-sharp crack-sharp` - this command launches a container and returns immediately (fire-and-forget).
4. Test the app by opening `<container_address>/api/v1/des` in a browser.

### Usage
1. Execute the following command in PowerShell substituting `<address>` with the appropriate value:
``` PowerShell
Invoke-WebRequest -Uri "<address>/api/v1/des?hash=50.jPgLzVirkc" -UseBasicParsing -TimeoutSec 30
```
2. *Output*: you will almost immediately see `hi` - that's the word behind the DES hash `50.jPgLzVirkc`

#### Available params
- `hash=<some_des_hash_here>` - the service will attempt to find a combination of characters behind the given DES hash.
- `maxWordLength=<your_number_here>` (optional) - the service will check all character combinations (words) starting from 1 char-long and up to the provided word length before giving up. Defalut value for `maxWordLength` is 8.
- `chars=abcXYZ` (optional) - the service will only build combinations from these characters. Default value for `chars` is `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`.

### Remarks
1. Primary goal of each request to this web service is to decrypt the specified DES hash. It means that the parameters `maxWordLength` and `chars` will be ignored if the service already knows a decrypted value of the hash. Also if two requests to decrypt the same hash are made at the same moment but with different values for `chars` and/or `maxWordLength`, two separate decryption tasks will start and each task will periodically check if the hash is already decrypted (and cached) by the other task - in which case it will immediately return decrypted value even if it's not composable from the specified `chars` or is longer than `maxWordLength`.

2. If decrypting a hash with an OpenShift-hosted app takes longer than 30 seconds, chances are you will see an error `504 Gateway Time-out The server didn't respond in time.` coming from your OpenShift route. At the time of writing, to fix this I had to add an annotation to the route with a key `haproxy.router.openshift.io/timeout` and value `10m` which increased the timeout to 10 minutes (according to [this](https://docs.openshift.com/container-platform/4.2/networking/routes/route-configuration.html) article).