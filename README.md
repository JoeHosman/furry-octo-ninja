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


Appendex B.Recording Fiddler Sessions
================

	http://fiddler.wikidot.com/fiddlercore-autorespond
	
Unlike Fiddler, FiddlerCore does not keep a session list automatically. If you want a session list, simply create an List<Fiddler.Session> and add new sessions to it as they’re received. Note that the multi-threaded nature of FiddlerCore means you need to Invoke to a single thread, use thread-safe data structures, or use a Monitor or other synchronization mechanism (as shown below) to update/iterate a list of sessions in a thread-safe way.

	// Inside your main object, create a list to hold the sessions
	// This generic list type requires your source file includes #using System.Collections.Generic.
		List<Fiddler.Session> oAllSessions = new List<Fiddler.Session>();
	 
	// Inside your attached event handlers, add the session to the list:
		Fiddler.FiddlerApplication.BeforeRequest += delegate(Fiddler.Session oS) {
				Monitor.Enter(oAllSessions);
				oAllSessions.Add(oS);
				Monitor.Exit(oAllSessions);
		};
		
Keep in mind, however, that keeping a session list can quickly balloon out the memory use within your application because a web Session object will not be garbage collected while there's an active reference to it (in the oAllSessions list). You should periodically trim the list to keep it of a reasonable size. Alternatively, if you only care about request URLs or headers, you could keep a List<> of those types rather than storing a reference to the full session object.


Appendex C.Replaying Fiddler Sessions
================

	http://fiddler.wikidot.com/fiddlercore-sessionlist

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

