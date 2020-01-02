## CrackSharp.Api
Use code in this directory to build a .NET Core WebAPI service, capable of bruteforcing DES hashes using a specified charset and expected length of the text behind the hash. It can decrypt multiple hashes in parallel: it starts a new decryption task for each *unique* set of parameters and then each task periodically checks if the hash has already been decrypted by the other tasks.

The service also allows to calculate DES hashes from arbitrary text and (optionally) salt.

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
1. To **decrypt** a hash `50.jPgLzVirkc`, open the following address in your web browser, substituting `<address>` with your web-service's address:
`<address>/api/v1/des/decrypt?hash=50.jPgLzVirkc`. Output: `hi` - that's the text behind the DES hash `50.jPgLzVirkc`
2. To **encrypt** a word `LOL`, open the following address in your web browser, substituting `<address>` with your web-service's address:
`<address>/api/v1/des/encrypt?text=LOL`. Output: DES hash that corresponds to `LOL` with its first two characters being an auto-generated salt.

#### Available params
*Decryption*
- `hash=<some_des_hash_here>` - the service will attempt to find a combination of characters behind the given DES hash.
- `maxTextLength=<your_number_here>` (optional) - the service will check all character combinations (words) starting from 1 char-long and up to the provided word length before giving up. Defalut value is `8`.
- `chars=abcXYZ` (optional) - the service will only build combinations from these characters. Default value is `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`.

*Encryption*
- `text=<text_to_encrypt>` - the service will encrypt the specified text and return encryption result. If salt is not specified by the user, it is generated automatically.
- `salt=<encryption_salt>` (optional) - salt allows for predictable encryption results. The first two characters of a DES hash is its salt.

### Remarks
1. Primary goal of each decryption request to this web service is to decrypt the specified DES hash. It means that the parameters `maxTextLength` and `chars` will be ignored if the service already knows a decrypted value of the hash. Also if multiple requests to decrypt the same hash are made at the same moment but with different values for `chars` and/or `maxTextLength`, multiple decryption tasks will start; if any of these tasks decrypts the hash, all these tasks will immediately return decrypted value even if it's not composable from the specified `chars` or is longer than `maxTextLength`.

2. If decrypting a hash with an OpenShift-hosted app takes longer than 30 seconds, chances are you will see an error `504 Gateway Time-out The server didn't respond in time.` coming from your OpenShift route. At the time of writing, to fix this I had to add an annotation to the route with a key `haproxy.router.openshift.io/timeout` and value `10m` which increased the timeout to 10 minutes (according to [this](https://docs.openshift.com/container-platform/4.2/networking/routes/route-configuration.html) article).
