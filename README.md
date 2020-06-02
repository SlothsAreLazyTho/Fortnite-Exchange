## Exchange
Get exchange token from your epic games account to authenticate within certain services.

* [This Nils](https://github.com/ThisNils), Thanks for helping.


## Getting started.
Run the jar file via a run.bat file.
It will open a screen for logging in to epic games. After doing the login procedure you will get a link look like this.

```
{
"redirectUrl": "https://accounts.epicgames.com/fnauth?code=************************",
"sid": null
}
```

Copy the code parameter and paste it into the console. Finally it will give you the exchange token. After the first time giving the authentication code. It will generate an device file. This will give you the exchange code immediately


## Built with

[Newtonsoft Json](https://www.newtonsoft.com/json) - Converting json to classes.
