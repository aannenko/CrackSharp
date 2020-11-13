## CrackSharp.Api
Use code in this directory to build a .NET Core WebAPI application, capable of bruteforcing DES hashes produced by [crypt(3)](https://www.man7.org/linux/man-pages/man3/crypt.3.html), using a specified charset and expected length of the text behind the hash. It can decrypt multiple hashes in parallel: it starts a new decryption task for each *unique* set of parameters (see remarks below).

The application also allows to calculate crypt(3)-like DES hashes from arbitrary text and, optionally, salt.

### Docker Deployment
1. Clone this repository and `cd` to its root
2. Create a Docker image: `docker build -t crack-sharp .`
3. Use one of the following commands:
    - `docker run -it --rm -p 5000:5000 --name crack-sharp crack-sharp` - run a container and attach to its console, container's files will be removed once it is stopped (useful for testing/debugging the app)
    - `docker run -d -p 5000:5000 --name crack-sharp crack-sharp` - run a container in detached mode
4. Test the app by opening `<container_address>/api/v1/des/encrypt?text=test` in a browser.

### Usage
1. Run `dotnet run -c Release` to start the application as a web-service.
2. To **decrypt** a hash `50.jPgLzVirkc`, open the following address in your web browser, substituting `<address>` with your web-service's address:
`<address>/api/v1/des/decrypt?hash=50.jPgLzVirkc`. Output: `hi` - that's the text behind the hash `50.jPgLzVirkc`.
3. To **encrypt** a word `LOL`, open the following address in your web browser, substituting `<address>` with your web-service's address:
`<address>/api/v1/des/encrypt?text=LOL`. Output: hash that corresponds to `LOL` with its first two characters being an auto-generated salt.

#### Available parameters
Decryption
- `hash=<some_des_hash_here>` - the service will attempt to find a combination of characters behind the given hash.
- `maxTextLength=<your_number_here>` (optional) - the service will check all character combinations (words) starting from 1 char-long and up to the provided word length before giving up. Defalut value is `8` which is also a maximum, see remarks below.
- `chars=abcXYZ` (optional) - the service will only build combinations from these characters. Default value is `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`.

Encryption
- `text=<text_to_encrypt>` - the service will encrypt first 8 characters of the specified text (see remarks below) and return encryption result. If salt is not specified by the user, it is generated automatically.
- `salt=<encryption_salt>` (optional) - salt allows for predictable encryption results. The first two characters of a hash is its salt.

### Remarks
1. Primary goal of each decryption request to this web service is to decrypt the specified hash. It means that the parameters `maxTextLength` and `chars` will be ignored if the service already knows a decrypted value of the hash. Also if multiple requests to decrypt the same hash are made at the same time but with different values for `chars` and/or `maxTextLength`, multiple decryption tasks will start. If any of these tasks decrypts the hash, all these tasks will immediately return decrypted value even if it's not composable from the specified `chars` or is longer than `maxTextLength`.

2. Encryption requests end up with encryption results being cached so that decryption requests could use them in the future. For example, a request `/api/v1/des/encrypt?text=tungstenite&salt=a1` will return `a1dosrPtorvEw`, and a subsequent request `/api/v1/des/decrypt?hash=a1dosrPtorvEw` will instantly return `tungsten` (crypt(3) only encrypts first 8 characters of the text). If the encryption result would not have been there for the decryption request to use, decryption process would start and take some time to decrypt the hash.
