furry-octo-ninja
================

Fiddler Session Replay


Getting Started
================

1. Download; Install fiddler2 [http://www.fiddler2.com/fiddler2/]

2. Download; Install FiddlerCore (v2.4.0.1) [http://www.fiddler2.com/Fiddler/Core/]


Appendex A.FiddlerCore
================

FiddlerCore is currently provided as a .NET class library that can be consumed by any .NET application. 

FiddlerCore is designed for use in special-purpose applications that run with either no user-interface (e.g. test automation), or a UI which is so specialized that a Fiddler Addon would not be a suitable option (e.g. a WPF traffic visualization).

FiddlerCore Wiki: http://fiddler.wikidot.com/fiddlercore


Appendex B.Replaying Fiddler Sessions
================
The following snippet looks for requests for replaceme.txt and returns a previously captured response stored in a session object named SessionIWantToReturn.

	Fiddler.FiddlerApplication.BeforeRequest += delegate(Fiddler.Session oS)
	  {
		if (oS.uriContains("replaceme.txt"))
		{
			oS.utilCreateResponseAndBypassServer();
			oS.responseBodyBytes = SessionIWantToReturn.responseBodyBytes;
			oS.oResponse.headers = (HTTPResponseHeaders) SessionIWantToReturn.oResponse.headers.Clone();
		}
	  };

