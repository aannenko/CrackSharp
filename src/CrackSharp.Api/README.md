## CrackSharp.Api
Use code in this directory to build a .NET WebAPI service, capable of bruteforcing DES hashes produced by [crypt(3)](https://www.man7.org/linux/man-pages/man3/crypt.3.html), using a specified charset and expected length of the text behind the hash. It can decrypt multiple hashes in parallel: it starts a new decryption task for each *unique* set of parameters (see remarks below).

The service also allows to calculate crypt(3)-like DES hashes from arbitrary text and, optionally, salt.

### Usage
Examples in PowerShell 7 or bash:
```powershell
# Build and run the app
dotnet run -c Release

# Attempt to decrypt 50.jPgLzVirkc using a default charset
curl -kL 'http://localhost:5000/api/v1/des/decrypt?hash=50.jPgLzVirkc' # output: hi

# Attempt to decrypt 50.jPgLzVirkc using a charset 'efghij', give up after trying 'jjj'
curl -kL 'http://localhost:5000/api/v1/des/decrypt?hash=50.jPgLzVirkc&chars=efghij&maxTextLength=3' # output: hi

# Encrypt 'LOL' using random salt
curl -kL 'http://localhost:5000/api/v1/des/encrypt?text=LOL' # output (something like): FAzlTwVAZ1NZ2

# Encrypt 'LOL' using salt '50'
curl -kL 'http://localhost:5000/api/v1/des/encrypt?text=LOL&salt=50' # output: 50cI2vYkF0YU2
```

#### Parameters
Decryption
- `hash=<some_des_hash_here>` (required) - the service will attempt to find a combination of characters behind the given hash.
- `maxTextLength=<your_number_here>` (optional) - the service will check all character combinations (words) starting from 1 char-long and up to the provided word length before giving up. Defalut value is `8` which is also a maximum, see remarks below.
- `chars=abcXYZ` (optional) - the service will only build combinations from these characters. Default value is `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`.

Encryption
- `text=<text_to_encrypt>` (required) - the service will encrypt first 8 characters of the specified text (see remarks below) and return encryption result. If salt is not specified by the user, it is generated automatically.
- `salt=<encryption_salt>` (optional) - salt allows for predictable encryption results. The first two characters of a hash is its salt.

### Docker

#### Build image locally and run
```powershell
git clone https://github.com/aannenko/CrackSharp.git
cd CrackSharp
docker build -t crack-sharp .

# run a container and attach to its console, container's files will be removed once it is stopped (useful for testing/debugging)
docker run -it --rm -p 5000:5000 -e ASPNETCORE_URLS=http://+:5000 --name crack-sharp crack-sharp

# -- OR --

# run a container in detached mode
docker run -d -p 5000:5000 -e ASPNETCORE_URLS=http://+:5000 --name crack-sharp crack-sharp
```

#### Run a pre-made image
```powershell
docker run -it --rm -p 5000:5000 -e ASPNETCORE_URLS=http://+:5000 ghcr.io/aannenko/cracksharp:master
```
For supported platforms, see "platforms" in [docker-publish.yml](https://github.com/aannenko/CrackSharp/blob/master/.github/workflows/docker-publish.yml).

#### Test
Open `<container_address>/api/v1/des/encrypt?text=test` in a browser to test encryption.

#### Swagger
```powershell
docker run -it --rm -p 5000:5000 -e DOTNET_ENVIRONMENT=Development -e ASPNETCORE_URLS=http://+:5000 ghcr.io/aannenko/cracksharp:master
```
Open `<container_address>/swagger` in a browser to access swagger.

### Remarks
1. Two or more decryption requests with the same trio of parameters `hash`, `maxTextLength` and `chars`, including omitted parameters with their default values, will start only one decryption task. When the decryption task is complete, all these requests will return the decrypted value or `404` if the hash could not be decrypted. Any request may be canceled during the decryption process - this will not cancel the task unless all the requests are canceled and no one is waiting for the task's completion, in which case the task is canceled.

2. Primary goal of each decryption request to this web service is to decrypt the specified hash. It means that the parameters `maxTextLength` and `chars` will be ignored if the service already knows a decrypted value of the hash. Also, multiple requests can be sent, looking to decrypt the same hash but with different `chars` and/or `maxTextLength`. If any of these requests decrypts the hash, the rest of the requests will immediately return the decrypted value even if their own `chars` or `maxTextLength` would make them return `404` individually.

3. Encryption requests put unencrypted text and its encrypted value (hash) into cache for the decryption requests to use. For example, encryption request `/api/v1/des/encrypt?text=tungstenite&salt=a1` will return `a1dosrPtorvEw`, and a subsequent decryption request `/api/v1/des/decrypt?hash=a1dosrPtorvEw` will instantly return `tungsten` (crypt(3) only encrypts first 8 characters of the text) because the hash and its encrypted value were already in the cache.
