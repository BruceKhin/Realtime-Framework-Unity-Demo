// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
#if (!UNITY_WEBPLAYER && (UNITY_PRO_LICENSE ||  UNITY_EDITOR ||  !(UNITY_IOS || UNITY_ANDROID)))
using System;
using System.Text;
using Realtime.Messaging.WebsocketsSharp.Net;

namespace Realtime.Messaging.WebsocketsSharp
{
  internal class HandshakeRequest : HandshakeBase
  {
    #region Private Fields

    private string _method;
    private string _uri;
    private bool   _websocketRequest;
    private bool   _websocketRequestWasSet;

    #endregion

    #region Private Constructors

    private HandshakeRequest ()
    {
    }

    #endregion

    #region Public Constructors

    public HandshakeRequest (string absPathAndQuery)
    {
      _uri = absPathAndQuery;
      _method = "GET";

      var headers = Headers;
      headers ["User-Agent"] = "websocket-sharp/1.0";
      headers ["Upgrade"] = "websocket";
      headers ["Connection"] = "Upgrade";
    }

    #endregion

    #region Public Properties

    public AuthenticationResponse AuthResponse {
      get {
        var auth = Headers ["Authorization"];
        return auth != null && auth.Length > 0
               ? AuthenticationResponse.Parse (auth)
               : null;
      }
    }

    public CookieCollection Cookies {
      get {
        return Headers.GetCookies (false);
      }
    }

    public string HttpMethod {
      get {
        return _method;
      }

      private set {
        _method = value;
      }
    }

    public bool IsWebSocketRequest {
      get {
        if (!_websocketRequestWasSet) {
          var headers = Headers;
          _websocketRequest = _method == "GET" &&
                              ProtocolVersion > HttpVersion.Version10 &&
                              headers.Contains ("Upgrade", "websocket") &&
                              headers.Contains ("Connection", "Upgrade");

          _websocketRequestWasSet = true;
        }

        return _websocketRequest;
      }
    }

    public string RequestUri {
      get {
        return _uri;
      }

      private set {
        _uri = value;
      }
    }

    #endregion

    #region Public Methods

    public static HandshakeRequest Parse (string [] headerParts)
    {
      var requestLine = headerParts [0].Split (new [] { ' ' }, 3);
      if (requestLine.Length != 3)
        throw new ArgumentException ("Invalid request line: " + headerParts [0]);

      var headers = new WebHeaderCollection ();
      for (int i = 1; i < headerParts.Length; i++)
        headers.SetInternally (headerParts [i], false);

      return new HandshakeRequest {
        Headers = headers,
        HttpMethod = requestLine [0],
        ProtocolVersion = new Version (requestLine [2].Substring (5)),
        RequestUri = requestLine [1]
      };
    }

    public void SetCookies (CookieCollection cookies)
    {
      if (cookies == null || cookies.Count == 0)
        return;

      var buff = new StringBuilder (64);
      foreach (var cookie in cookies.Sorted)
        if (!cookie.Expired)
          buff.AppendFormat ("{0}; ", cookie.ToString ());

      var len = buff.Length;
      if (len > 2) {
        buff.Length = len - 2;
        Headers ["Cookie"] = buff.ToString ();
      }
    }

    public override string ToString ()
    {
      var output = new StringBuilder (64);
      output.AppendFormat ("{0} {1} HTTP/{2}{3}", _method, _uri, ProtocolVersion, CrLf);

      var headers = Headers;
      foreach (var key in headers.AllKeys)
        output.AppendFormat ("{0}: {1}{2}", key, headers [key], CrLf);

      output.Append (CrLf);

      var entity = EntityBody;
      if (entity.Length > 0)
        output.Append (entity);

      return output.ToString ();
    }

    #endregion
  }
}

#endif