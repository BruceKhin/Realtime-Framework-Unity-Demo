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
  internal class HandshakeResponse : HandshakeBase
  {
    #region Private Fields

    private string _code;
    private string _reason;

    #endregion

    #region Private Constructors

    private HandshakeResponse ()
    {
    }

    #endregion

    #region Public Constructors

    public HandshakeResponse (HttpStatusCode code)
    {
      _code = ((int) code).ToString ();
      _reason = code.GetDescription ();

      var headers = Headers;
      headers ["Server"] = "websocket-sharp/1.0";
      if (code == HttpStatusCode.SwitchingProtocols) {
        headers ["Upgrade"] = "websocket";
        headers ["Connection"] = "Upgrade";
      }
    }

    #endregion

    #region Public Properties

    public AuthenticationChallenge AuthChallenge {
      get {
        var auth = Headers ["WWW-Authenticate"];
        return auth != null && auth.Length > 0
               ? AuthenticationChallenge.Parse (auth)
               : null;
      }
    }

    public CookieCollection Cookies {
      get {
        return Headers.GetCookies (true);
      }
    }

    public bool IsUnauthorized {
      get {
        return _code == "401";
      }
    }

    public bool IsWebSocketResponse {
      get {
        var headers = Headers;
        return ProtocolVersion > HttpVersion.Version10 &&
               _code == "101" &&
               headers.Contains ("Upgrade", "websocket") &&
               headers.Contains ("Connection", "Upgrade");
      }
    }

    public string Reason {
      get {
        return _reason;
      }

      private set {
        _reason = value;
      }
    }

    public string StatusCode {
      get {
        return _code;
      }

      private set {
        _code = value;
      }
    }

    #endregion

    #region Public Methods

    public static HandshakeResponse CreateCloseResponse (HttpStatusCode code)
    {
      var res = new HandshakeResponse (code);
      res.Headers ["Connection"] = "close";

      return res;
    }

    public static HandshakeResponse Parse (string [] headerParts)
    {
      var statusLine = headerParts [0].Split (new [] { ' ' }, 3);
      if (statusLine.Length != 3)
        throw new ArgumentException ("Invalid status line: " + headerParts [0]);

      var headers = new WebHeaderCollection ();
      for (int i = 1; i < headerParts.Length; i++)
        headers.SetInternally (headerParts [i], true);

      return new HandshakeResponse {
        Headers = headers,
        ProtocolVersion = new Version (statusLine [0].Substring (5)),
        Reason = statusLine [2],
        StatusCode = statusLine [1]
      };
    }

    public void SetCookies (CookieCollection cookies)
    {
      if (cookies == null || cookies.Count == 0)
        return;

      var headers = Headers;
      foreach (var cookie in cookies.Sorted)
        headers.Add ("Set-Cookie", cookie.ToResponseString ());
    }

    public override string ToString ()
    {
      var output = new StringBuilder (64);
      output.AppendFormat ("HTTP/{0} {1} {2}{3}", ProtocolVersion, _code, _reason, CrLf);

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
