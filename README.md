# Sample Code for Mailgun on ASP.NET Core

This is a sample code for use in conjunction with my tutorial blog, [Using Mailgun in ASP.NET Core on Linux Mint 17](http://www.cat-in-the-box.com/MadHatterHouse/vale/2016/using-mailgun-in-asp-net-core-on-linux-mint-17/).

## Building
Install the latest [.NET Core](https://www.microsoft.com/net/core).

Clone directory:

	> git clone https://github.com/ValleyWulf/NETCore-MailGun.git
	> cd NETCore-MailGun
	> dotnet restore

## Set Up
Before running the code, you would need to supply it with a valid configuration.

To do so, create a directory called `Config`:

	> mkdir Config

And create a file called `MailGunEmailSettings.json` under the `Config` directory.

The `MailGunEmailSettings.json` would contain the settings for Mailgun configuration:

	{

 	  "SMTPSettings": 
 	  {
   	    "SmtpHost": "smtp.mailgun.org",
   	    "SmtpPort": "587",
   	    "SmtpLogin": "<Mailgun's Default SMTP Login>",
   	    "SmtpPassword": "<Mailgun's Default Password>",
   	    "SenderName": "<Sender Name>",
   	    "From": "<someone@somewhere.com>"
 	  },

 	  "RestAPISettings": 
 	  {
   	    "ApiKey": "<Mailgun's API Key>",
   	    "BaseUri": "<Mailgun's API Base URI>",
	    "RequestUri": "messages",
   	    "From": "<Non-Reply <someone@somewhere.com>>"
 	  }

	}
	
By default, the web application uses the configuration for api.  You can change this in `appsettings.json`, under the `EmailProvider` section:

	"EmailProvider": {
	    "Provider": "MailGun",
	    "Description": "Mail service that sends out registration confirmation and account recovery emails.",
	    "ConfigFile": "Config/MailGunEmailSettings.json",
	    "ConnectionType": "api"
	},


## Runing the app
To trun the app, simply do:

	> dotnet run

And bring up a browser and go to URL `localhost:5000` or whatever default URL your web application is set up to run on.

## License
This sample code is licensed under the [MIT License](https://github.com/ValleyWulf/NETCore-MailGun/blob/master/License.txt).

