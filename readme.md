# ![dosiero-logo](./src/Dosiero/wwwroot/favicon.svg)  Dosiero

A minimalist  *stateless* self-hosted *no-js* pay-to-access file server with Monero payments, designed to be easily extensible.

<p align="center">
<img src="./images/home.png" width="140px" />
<img src="./images/browse.png" width="140px" />
<img src="./images/pay.png" width="140px" />
<img src="./images/download.png" width="140px" />
</p>

## Building

Download one of the pre-built binaries from the releases section, or clone the repository and run the following command:

```sh
dotnet publish ./src/Dosiero --output build --self-contained
```

Then in the `build` folder there will be a `dosiero` executable.

## Running

All configuration is passed in to the program via command line arguments. It is suggested that you create a shell script
to invoke the program with the desired arguments to avoid typing them out each time.

Powershell:

```ps1
.\dosiero.exe `
>> /provider fs ~/Documents/store/files `
>> /price %.txt 0.01 `
>> /price %.png 0.05 `
>> /readme %.% "~/Documents/store/docs/%1.html" `
>> /payment monero https://127.0.0.1:28089 --username user --password pass
```

Bash:
    
```sh
./doserio \
  /provider fs ~/store/files \
  /price %.txt 0.01 \
  /price %.png 0.05 \
  /readme %.% ~/store/docs/%1.html \
  /readme %.txt ~/store/docs/%1.txt.html \
  /payment monero https://127.0.0.1:28089 --username user --password pass
```

You can get help by passing `--help` to the end of any command.