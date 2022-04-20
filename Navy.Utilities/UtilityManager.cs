using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

//using Models.Common;

namespace Navy.Utilities
{
	public class UtilityManager
    {
        const string thisClassName = "UtilityManager";


        /// <summary>
        /// Default constructor for UtilityManager
        /// </summary>
        public UtilityManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        #region === Resource Manager Methods ===
        /// <summary>
        /// Handle converting the format of the resource string (to not use periods, etc,)
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="resKey"></param>
        /// <returns></returns>
        public static string ChangeResourceString( ResourceManager rm, string resKey )
        {

            string resultString = "";
            try
            {
                resultString = Regex.Replace( resKey, "\\.", "_" );
                resultString = Regex.Replace( resultString, "\\*", "_" );
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch ( ArgumentException ex )
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // Syntax error in the regular expression
                resultString = resKey;
            }

            return rm.GetString( resultString );
        }

        /// <summary>
        /// Retrieves a string from the resource file
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="resKey">Resourse key</param>
        /// <returns>Related resource string</returns>
        public static string GetResourceValue( ResourceManager rm, string resKey )
        {

            string keyName = "";
            try
            {
                keyName = Regex.Replace( resKey, "\\.", "_" );
                keyName = Regex.Replace( keyName, "\\*", "_" );
            }
            catch ( ArgumentException ex )
            {
                // Syntax error in the regular expression
                keyName = resKey;
            }

            return rm.GetString( keyName );
        }


        /// <summary>
        /// Gets the value of a resource string from applicable resource file. Returns blanks if not found
        /// </summary>
        /// <param name="rm">ResourseManager</param>
        /// <param name="resKey">Key name in resource file</param>
        /// <param name="defaultValue">Value to use if resource is not found</param>
        /// <returns>String from resource file or default value</returns>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public static string GetResourceValue( ResourceManager rm, string resKey, string defaultValue )
        {
            string resource = "";
            string keyName = "";

            try
            {
                keyName = Regex.Replace( resKey, "\\.", "_" );
                keyName = Regex.Replace( keyName, "\\*", "_" );

                resource = rm.GetString( keyName );
                if ( resource.Length < 1 )
                    resource = defaultValue;
            }
            catch
            {
                resource = defaultValue;
            }

            return resource;
        } //

        #endregion

        #region === Application Keys Methods ===

        /// <summary>
        /// Gets the value of an application key from web.config. Returns blanks if not found
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public static string GetAppKeyValue( string keyName )
        {

            return GetAppKeyValue( keyName, "" );
        } //

        /// <summary>
        /// Gets the value of an application key from web.config. Returns the default value if not found
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public static string GetAppKeyValue( string keyName, string defaultValue )
        {
            string appValue = "";

            try
            {
                appValue = System.Configuration.ConfigurationManager.AppSettings[ keyName ];
                if ( appValue == null )
                    appValue = defaultValue;
            }
            catch
            {
                appValue = defaultValue;
				if ( HasMessageBeenPreviouslySent( keyName ) == false )
					LoggingHelper.LogError( string.Format( "@@@@ Error on appKey: {0},  using default of: {1}", keyName, defaultValue ) );
            }

            return appValue;
        } //
        public static int GetAppKeyValue( string keyName, int defaultValue )
        {
            int appValue = -1;

            try
            {
                appValue = Int32.Parse( System.Configuration.ConfigurationManager.AppSettings[ keyName ] );

                // If we get here, then number is an integer, otherwise we will use the default
            }
            catch
            {
                appValue = defaultValue;
				if ( HasMessageBeenPreviouslySent( keyName ) == false )
					LoggingHelper.LogError( string.Format( "@@@@ Error on appKey: {0},  using default of: {1}", keyName, defaultValue ) );
            }

            return appValue;
        } //
        public static bool GetAppKeyValue( string keyName, bool defaultValue, bool reportMissingKey = true )
        {
            bool appValue = false;

            try
            {
                appValue = bool.Parse( System.Configuration.ConfigurationManager.AppSettings[ keyName ] );
            }
            catch (Exception ex)
            {
                appValue = defaultValue;
				if ( reportMissingKey && HasMessageBeenPreviouslySent( keyName ) == false )
					 LoggingHelper.LogError( string.Format( "@@@@ Error on appKey: {0},  using default of: {1}", keyName, defaultValue ) );
            }

            return appValue;
        } //

		public static bool HasMessageBeenPreviouslySent( string keyName )
		{

			string key = "appkey_" + keyName;
			//check cache for keyName
			if ( HttpRuntime.Cache[ key ] != null )
			{
				return true;
			}
			else
			{
				//not really much to store
				HttpRuntime.Cache.Insert( key, keyName );
			}

			return false;
		}
		#endregion

		#region === Security related Methods ===

		/// <summary>
		/// Encrypt the text using MD5 crypto service
		/// This is used for one way encryption of a user password - it can't be decrypted
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string Encrypt( string data )
        {
            byte[] byDataToHash = ( new UnicodeEncoding() ).GetBytes( data );
            byte[] bytHashValue = new MD5CryptoServiceProvider().ComputeHash( byDataToHash );
            return BitConverter.ToString( bytHashValue );
        }

        /// <summary>
        /// Encrypt the text using the provided password (key) and CBC CipherMode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encrypt_CBC( string text, string password )
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;

            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes( password );

            byte[] keyBytes = new byte[ 16 ];

            int len = pwdBytes.Length;

            if ( len > keyBytes.Length ) len = keyBytes.Length;

            System.Array.Copy( pwdBytes, keyBytes, len );

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;

            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

            byte[] plainText = Encoding.UTF8.GetBytes( text );

            byte[] cipherBytes = transform.TransformFinalBlock( plainText, 0, plainText.Length );

            return Convert.ToBase64String( cipherBytes );

        }

        /// <summary>
        /// Decrypt the text using the provided password (key) and CBC CipherMode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Decrypt_CBC( string text, string password )
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;

            byte[] encryptedData = Convert.FromBase64String( text );

            byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes( password );

            byte[] keyBytes = new byte[ 16 ];

            int len = pwdBytes.Length;

            if ( len > keyBytes.Length ) len = keyBytes.Length;

            System.Array.Copy( pwdBytes, keyBytes, len );

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;

            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();

            byte[] plainText = transform.TransformFinalBlock( encryptedData, 0, encryptedData.Length );

            return Encoding.UTF8.GetString( plainText );

        }

        /// <summary>
        /// Encode a passed URL while first checking if already encoded
        /// </summary>
        /// <param name="url">A web Address</param>
        /// <returns>Encoded URL</returns>
        public static string EncodeUrl( string url )
        {
            string encodedUrl = "";

            if ( url.Length > 0 )
            {
                //check if already encoded

                if ( url.ToLower().IndexOf( "%3a" ) > -1
                    //|| url.ToLower().IndexOf( "&amp;" ) > -1
                )
                {
                    encodedUrl = url;
                }
                else
                {
                    encodedUrl = HttpUtility.UrlEncode( url );
                    //fix potential encode errors:
                    encodedUrl = encodedUrl.Replace( "%26amp%3b", "%26" );
                }
            }

            return encodedUrl;
        }

		/// <summary>
		/// Generate an MD5 hash of a string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GenerationMD5String( string input, bool returnAsLowerCase = true )
		{
			// Use input string to calculate MD5 hash
			using ( System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create() )
			{
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes( input );
				byte[] hashBytes = md5.ComputeHash( inputBytes );

				// Convert the byte array to hexadecimal string
				StringBuilder sb = new StringBuilder();
				for ( int i = 0; i < hashBytes.Length; i++ )
				{
					sb.Append( hashBytes[ i ].ToString( "X2" ) );
				}
                if ( returnAsLowerCase )
                    return sb.ToString().ToLower();
                else
                    return sb.ToString();
            }
		}
		public static string CreateCtidFromString( string property)
		{
			//assign default, just in case
			string ctid = "ce-" + Guid.NewGuid().ToString().ToLower();
			if (string.IsNullOrWhiteSpace(property))
			{				
				return ctid;
			}
			string id = GenerationMD5String( property );
			if ( id.Length == 32 )
			{
				ctid = "ce-" + id.Substring( 0, 8 ) + "-" + id.Substring( 8, 4 ) + "-" + id.Substring( 12, 4 ) + "-" + id.Substring( 16, 4 ) + "-" + id.Substring( 20, 12 );
				LoggingHelper.DoTrace( 1, "CreateCtidFromString. Input: " + property + ", CTID: " + ctid );
			} else
			{

			}
		
			return ctid;
		}
		#endregion

		#region === Path related Methods ===
		/// <summary>
		/// FormatAbsoluteUrl an absolute URL - equivalent to Url.Content()
		/// </summary>
		/// <param name="path"></param>
		/// <param name="uriScheme"></param>
		/// <returns></returns>
		public static string FormatAbsoluteUrl( string path, string uriScheme = null )
		{
			try
			{
				//need to handle where called from batch!!!!
				if ( HttpContext.Current == null )
				{
					//try other methods
					return FormatAbsoluteUrl( path, false );
				}

				uriScheme = uriScheme ?? HttpContext.Current.Request.Url.Scheme; //allow overriding http or https

				var environment = System.Configuration.ConfigurationManager.AppSettings[ "environment" ]; //Use port number only on localhost because https redirecting to a port on production screws this up
				string host = environment == "development" ? HttpContext.Current.Request.Url.Authority : HttpContext.Current.Request.Url.Host;

				return uriScheme + "://" +
					( host + "/" + HttpContext.Current.Request.ApplicationPath + path.Replace( "~/", "/" ) )
					.Replace( "///", "/" )
					.Replace( "//", "/" );
			}
			catch ( Exception ex )
			{
				//try other methods
				return FormatAbsoluteUrl( path, false );
			}
		}
		//
		/// <summary>
		/// Format a relative, internal URL as a full URL, with http or https depending on the environment. 
		/// Determines the current host and then calls overloaded method to complete the formatting
		/// </summary>
		/// <param name="relativeUrl">Internal URL, usually beginning with the path after the domain/</param>
		/// <param name="isSecure">If the URL is to be formatted as a secure URL, set this value to true.</param>
		/// <returns>Formatted URL</returns>
		public static string FormatAbsoluteUrl( string relativeUrl, bool isSecure )
        {
            string host = "";
            try
            {
                //14-10-10 mp - change to explicit value from web.config
                host = GetAppKeyValue( "domainName" );
                if ( host == "" )
                {
                    // doing it this way so as to not break anything - HttpContext doesn't exist in a WCF web service
                    // so if this doesn't work we go get the FQDN another way.
                    host = HttpContext.Current.Request.ServerVariables[ "HTTP_HOST" ];
                    //need to handle ports!!
                }
            }
            catch ( Exception ex )
            {
                host = Dns.GetHostEntry( "localhost" ).HostName;
                // Fix up name with www instead of webX
                Regex hostEx = new Regex( @"web.?" );
                Match match = hostEx.Match( host );
                if ( match.Index > -1 )
                {
                    if (match.Value.Length > 0)
                        host = host.Replace( match.Value, "www" );
                }
            }

            return FormatAbsoluteUrl( relativeUrl, host, isSecure );
        }

        /// <summary>
        /// Format a relative, internal URL as a full URL, with http or https depending on the environment.
        /// </summary>
        /// <param name="relativeUrl">Internal URL, usually beginning with /vos_portal/</param>
        /// <param name="host">name of host (e.g. localhost, edit.credentialengine.org, www.credentialengine.org)</param>
        /// <param name="isSecure">If the URL is to be formatted as a secure URL, set this value to true.</param>
        /// <returns>Formatted URL</returns>
        private static string FormatAbsoluteUrl( string relativeUrl, string host, bool isSecure )
        {
            string url = "";
            if ( string.IsNullOrEmpty( relativeUrl ) )
                return "";
            if ( string.IsNullOrEmpty( host ) )
                return "";
			//ensure not already an absolute
			if ( host.ToLower().StartsWith( "http" ) )
			{
				if ( host.EndsWith( "/" ) && relativeUrl.StartsWith( "/" ) )
					relativeUrl = relativeUrl.TrimStart( '/' );
				url = ( host + relativeUrl );
				return url;
			}
			//            if ( relativeUrl.ToLower().StartsWith( "http" ) )
			return relativeUrl;
        }

        /// <summary>
        /// Gets the last section (subchannel) of passed url
        /// Note downside is we are working with physical url which may not be meaningfull (esp for business main channels)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlLastSection( string url )
        {
            //
            string section = "";
            string[] dirArray = url.Split( '/' );

            for ( int i = dirArray.Length ; i > 0 ; i-- )
            {
                if ( dirArray[ i - 1 ].Trim().Length > 0 && dirArray[ i - 1 ].IndexOf( ".htm" ) == -1 )
                {
                    section = dirArray[ i - 1 ].Trim();
                    break;
                }
            }
            return section;
        }//

        /// <summary>
        /// Insert soft breaks into long URLs or email addresses in text string
        /// </summary>
        /// <param name="anchorText">Text to insert soft breaks into</param>
        /// <returns></returns>
        public static string InsertSoftbreaks( string anchorText )
        {
            string skippingSoftBreaks = GetAppKeyValue( "skippingSoftBreaks", "no" );

            StringBuilder newText = new StringBuilder( 255 );
            const string softbreak = "<span class=\"softbreak\"> </span>";
            // First make sure that the text doesn't already contain softbreak
            int sbPos = anchorText.ToLower().IndexOf( "softbreak" );
            if ( sbPos >= 0 )
                return anchorText;

            if ( skippingSoftBreaks.Equals( "yes" ) )
                return anchorText;

            //skip if an img exists
            int imgPos = anchorText.ToLower().IndexOf( "<img" );
            if ( imgPos >= 0 )
                return anchorText;
            imgPos = anchorText.ToLower().IndexOf( "<asp:img" );
            if ( imgPos >= 0 )
                return anchorText;

            //check for large anchor text - could be indicative of missing/misplaced ending tags, 
            //which could cause a problem
            if ( anchorText.Length > 200 )
                return anchorText;

            // We're going to look for http, img, /, and @
            //MP - should also try to handle https!
            int httpPos = anchorText.IndexOf( "http://" );
            int atPos = anchorText.IndexOf( "@" );
            int slashPos = anchorText.IndexOf( "/" );

            if ( ( httpPos >= 0 ) && ( atPos == -1 ) )
            {
                // We have http but not @
                if ( ( httpPos >= imgPos ) && ( imgPos >= 0 ) )
                {
                    // The http may be inside an img tag, do nothing
                    return anchorText;
                }
            }
            if ( ( httpPos == -1 ) && ( atPos >= 0 ) )
            {
                // We have @ but not http
                if ( atPos >= imgPos )
                {
                    // the @ may be inside an img tag, do nothing
                    return anchorText;
                }
            }

            if ( ( httpPos >= 0 ) && ( atPos >= 0 ) )
            {
                //We have both @ and http
                if ( httpPos < atPos )
                {
                    if ( imgPos < httpPos )
                    {
                        return anchorText;
                    }
                }
                if ( atPos < httpPos )
                {
                    if ( imgPos < atPos )
                    {
                        return anchorText;
                    }
                }
            }

            if ( ( httpPos == -1 ) && ( atPos == -1 ) )
            {
                // we have neither @ nor http
                return anchorText;
            }

            // First we look to see if we have an http link, and handle it.
            if ( httpPos >= 0 )
            {
                string imgTagToEnd = "";
                string priorToImgTag = "";
                if ( imgPos >= 0 )
                {
                    priorToImgTag = anchorText.Substring( 0, imgPos );
                    imgTagToEnd = anchorText.Substring( imgPos, anchorText.Length - imgPos );
                }
                else
                {
                    priorToImgTag = anchorText;
                }
                httpPos += 7;  // 7 = length of string "http://"
                newText.Append( priorToImgTag.Substring( 0, httpPos ) );
                newText.Append( softbreak );
                priorToImgTag = priorToImgTag.Substring( httpPos, priorToImgTag.Length - ( httpPos ) );
                slashPos = priorToImgTag.IndexOf( "/" );
                while ( slashPos > -1 )
                {
                    slashPos++;
                    newText.Append( priorToImgTag.Substring( 0, slashPos ) );
                    newText.Append( softbreak );
                    priorToImgTag = priorToImgTag.Substring( slashPos, priorToImgTag.Length - slashPos );
                    slashPos = priorToImgTag.IndexOf( "/" );
                }
                if ( newText.ToString() == "http://" + softbreak )
                {
                    newText.Append( priorToImgTag );
                }
                priorToImgTag = newText.ToString();
                newText.Remove( 0, newText.ToString().Length );
                int dotPos = priorToImgTag.IndexOf( "." );
                while ( dotPos > -1 )
                {
                    dotPos++;
                    newText.Append( priorToImgTag.Substring( 0, dotPos ) );
                    newText.Append( softbreak );
                    priorToImgTag = priorToImgTag.Substring( dotPos, priorToImgTag.Length - dotPos );
                    dotPos = priorToImgTag.IndexOf( "." );
                }
                newText.Append( priorToImgTag );
                newText.Append( imgTagToEnd );
            }
            else
            {
                // Now we want to know if we're looking at an email address
                if ( atPos >= 0 )
                {
                    string imgTagToEnd = "";
                    string priorToImgTag = "";
                    if ( imgPos >= 0 )
                    {
                        priorToImgTag = anchorText.Substring( 0, imgPos );
                        imgTagToEnd = anchorText.Substring( imgPos, anchorText.Length - imgPos );
                    }
                    else
                    {
                        priorToImgTag = anchorText;
                    }
                    // Insert softbreak after the '@' sign.
                    atPos++;
                    newText.Append( priorToImgTag.Substring( 0, atPos ) );
                    newText.Append( softbreak );
                    newText.Append( priorToImgTag.Substring( atPos, priorToImgTag.Length - atPos ) );
                    // Now insert softbreak after each dot.
                    priorToImgTag = newText.ToString();
                    newText.Remove( 0, newText.ToString().Length );
                    int dotPos = priorToImgTag.IndexOf( "." );
                    while ( dotPos > -1 )
                    {
                        dotPos++;
                        newText.Append( priorToImgTag.Substring( 0, dotPos ) );
                        newText.Append( softbreak );
                        priorToImgTag = priorToImgTag.Substring( dotPos, priorToImgTag.Length - dotPos );
                        dotPos = priorToImgTag.IndexOf( "." );
                    }
                    newText.Append( priorToImgTag );
                    newText.Append( imgTagToEnd );
                }
                else
                {
                    newText.Append( anchorText );
                }
            }
            return newText.ToString();
        }//
        /// <summary>
        /// Insert landing page - used to record a link to an external site before actual transfer
        /// </summary>
        /// <param name="insideTag">Destination URL</param>
        /// <returns></returns>

 
        #endregion

        /// <summary>
        /// Format a title (such as for a library) to be url friendly
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string UrlFriendlyTitle( string title )
        {
            if ( title == null || title.Trim().Length == 0 )
                return "";

            title = title.Trim();

            string encodedTitle = title.Replace( " - ", "-" );
            encodedTitle = encodedTitle.Replace( " ", "_" );
            //encodedTitle = encodedTitle.Replace( ".", "-" );
            encodedTitle = encodedTitle.Replace( "'", "" );
            encodedTitle = encodedTitle.Replace( "&", "-" );
            encodedTitle = encodedTitle.Replace( "#", "" );
            encodedTitle = encodedTitle.Replace( "$", "S" );
            encodedTitle = encodedTitle.Replace( "%", "percent" );
            encodedTitle = encodedTitle.Replace( "^", "" );
            encodedTitle = encodedTitle.Replace( "*", "" );
            encodedTitle = encodedTitle.Replace( "+", "_" );
            encodedTitle = encodedTitle.Replace( "~", "_" );
            encodedTitle = encodedTitle.Replace( "`", "_" );
            encodedTitle = encodedTitle.Replace( ":", "-" );
            encodedTitle = encodedTitle.Replace( ";", "" );
            encodedTitle = encodedTitle.Replace( "?", "" );
            encodedTitle = encodedTitle.Replace( "\"", "_" );
            encodedTitle = encodedTitle.Replace( "\\", "_" );
            encodedTitle = encodedTitle.Replace( "<", "_" );
            encodedTitle = encodedTitle.Replace( ">", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );

            if ( encodedTitle.EndsWith( "." ) )
                encodedTitle = encodedTitle.Substring( 0, encodedTitle.Length - 1 );

            return encodedTitle;
        } //

		#region string helpers
        
		/// <summary>
		/// extract value of a particular named parameter from passed string (Assumes and equal sign is used)
		/// ex: for string:		
		///			string searchString = "key1=value1;key2=value2;key3=value3;";
		/// To retrieve the value for key2 use:
		///			value = ExtractNameValue( searchString, "key2", ";");
		/// </summary>
		/// <param name="sourceString">String to search</param>
		/// <param name="name">Name of "parameter" in string</param>
		/// <param name="endDelimiter">End Delimeter. A character used to indicate the end of value in the string (often a semi-colon)</param>
		/// <returns>The value associated with the passed name</returns>
		public static string ExtractNameValue( string sourceString, string name, string endDelimiter )
		{
			string assignDelimiter = "=";

			return ExtractNameValue( sourceString, name, assignDelimiter, endDelimiter );
		}//

		/// <summary>
		/// extract value of a particular named parameter from passed string. The assign delimiter
		/// ex: for string:		
		///			string radioButtonId = "Radio_q_4_c_15_";
		/// To retrieve the value for question # use:
		///			qNbr = ExtractNameValue( radioButtonId, "q", "_", "_");
		/// To retrieve the value for choiceId use:
		///			choiceId = ExtractNameValue( radioButtonId, "c", "_", "_");
		/// </summary>
		/// <param name="sourceString">String to search</param>
		/// <param name="name">Name of "parameter" in string</param>
		/// <param name="assignDelimiter">Assigned delimiter. Typically an equal sign (=), but could be any defining character</param>
		/// <param name="endDelimiter">End Delimeter. A character used to indicate the end of value in the string (often a semi-colon)</param>
		/// <returns></returns>
		public static string ExtractNameValue( string sourceString, string name, string assignDelimiter, string endDelimiter )
		{
			int pos = sourceString.IndexOf( name + assignDelimiter );

			if ( pos == -1 )
				return "";

			string value = sourceString.Substring( pos + name.Length + 1 );
			int pos2 = value.IndexOf( endDelimiter );
			if ( pos2 > -1 )
				value = value.Substring( 0, pos2 );

			return value;
		}//

		#endregion
		#region === Miscellaneous helper methods: defaults, IsDatatype, etc. ===
		/// <summary>
		/// Returns passed string as an integer, if is an integer and not null/empty. 
		/// Otherwise returns the passed default value
		/// </summary>
		/// <param name="stringToTest"></param>
		/// <param name="defaultValue"></param>
		/// <returns>The string parameter as an int or the default value if the parameter is not a vlid integer</returns>
		public static int AssignWithDefault( string stringToTest, int defaultValue )
        {
            int newVal;

            try
            {
                if ( stringToTest.Length > 0 && IsInteger( stringToTest ) )
                {
                    newVal = Int32.Parse( stringToTest );
                }
                else
                {
                    newVal = defaultValue;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //

        /// <summary>
        /// Checks passed string, if not nullthen returns the passed string. 
        ///	Otherwise returns the passed default value
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <param name="defaultValue"></param> 
        /// <returns>int</returns>
        public static string AssignWithDefault( string stringToTest, string defaultValue )
        {
            string newVal;

            try
            {
                if ( stringToTest == null )
                {
                    newVal = defaultValue;
                }
                else
                {
                    newVal = stringToTest;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //

        public static bool Assign( bool? value, bool defaultValue )
        {
            bool newVal;

            try
            {
                if ( value != null )
                {
                    newVal = ( bool ) value;
                }
                else
                {
                    newVal = defaultValue;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //
        public static int Assign(int? value, int defaultValue)
        {
            int newVal;
            try
            {
                if (value != null)
                {
                    newVal = (int)value;
                }
                else
                {
                    newVal = defaultValue;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //

        /// <summary>
        /// CurrencyToDecimal - handle assignment of a string containing formatted currency to a decimal
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal CurrencyToDecimal( string strValue, decimal defaultValue )
        {

            decimal decimalAmt = 0;

            try
            {
                if ( strValue == "" )
                {
                    decimalAmt = defaultValue;
                }
                else
                {
                    //remove leading $
                    string amount = strValue.Replace( "$", "" );
                    decimalAmt = decimal.Parse( amount );
                }
            }
            catch
            {

                decimalAmt = defaultValue;
            }

            return decimalAmt;

        } //
        /// <summary>
        /// IsInteger - test if passed string is an integer
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        public static bool IsInteger( string stringToTest )
        {
            int newVal;
            bool result = false;
            try
            {
                newVal = Int32.Parse( stringToTest );

                // If we get here, then number is an integer
                result = true;
            }
            catch
            {

                result = false;
            }
            return result;

        }
		public static int MapIntegerOrDefault( string stringToTest, int defaultValue = 0 )
		{
			var isValid = true;
			var value = MapInteger( stringToTest, ref isValid );
			return isValid ? value : defaultValue;
		}
        public static int MapInteger( string stringToTest, ref bool isValid )
        {
            int newVal = 0;
            isValid = false;
            try
            {
                isValid = Int32.TryParse( stringToTest, NumberStyles.Any,
                    NumberFormatInfo.InvariantInfo, out newVal );
            }
            catch
            {

                isValid = false;
            }
            return newVal;

        }
        /// <summary>
        /// IsNumeric - test if passed string is numeric
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        public static bool IsNumeric( string stringToTest )
        {
            double newVal;
            bool result = false;
            try
            {
                result = double.TryParse( stringToTest, NumberStyles.Any,
                    NumberFormatInfo.InvariantInfo, out newVal );
            }
            catch
            {

                result = false;
            }
            return result;

		}
		public static decimal MapDecimalOrDefault( string stringToTest, decimal defaultValue = 0 )
		{
			var isValid = true;
			var value = MapDecimal( stringToTest, ref isValid );
			return isValid ? value : defaultValue;
		}
		public static decimal MapDecimal( string stringToTest, ref bool isValid )
        {
            decimal newVal =0;
            isValid = false;
            try
            {
                isValid = decimal.TryParse( stringToTest, NumberStyles.Any,
                    NumberFormatInfo.InvariantInfo, out newVal );
            }
            catch
            {

                isValid = false;
            }
            return newVal;

        }

        /// <summary>
        /// IsDate - test if passed string is a valid date
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        public static bool IsDate( string stringToTest, ref DateTime outputDate )
        {

            //DateTime newDate;
            bool result = false;
            try
            {
                outputDate = System.DateTime.Parse( stringToTest );
                result = true;
            }
            catch
            {

                result = false;
            }
            return result;

        } //end

        public static bool IsValidDate( string stringToTest )
        {

            //DateTime newDate;
            bool result = false;
            try
            {
                DateTime outputDate = System.DateTime.Parse( stringToTest );
                result = true;
            }
            catch
            {
                result = false;
            }
            return result;

        } //end

        #endregion


        #region === Html handling/cleaning methods ===
        /// <summary>
        /// Check if text contains html tags
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static bool HasHtmlTags( string description )
        {
            if ( string.IsNullOrWhiteSpace( description ) )
                return false;

            var htmlTags = new Regex( @"<[^>]*>" ).Match( description );
            return htmlTags.Success;
        }
        /// <summary>
        /// Strip tags from user input.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// 
        public static String CleanText( String text )
        {
            bool scriptTagsFound = false;

            return CleanText( text, false, ref scriptTagsFound );
        }

        /// <summary>
        /// Remove all HTML and script tags
        /// </summary>
        /// <param name="text"></param>
        /// <param name="scriptTagsFound">True if any script tags found. This suggests need for a deliberate action/rebuke!</param>
        /// <returns></returns>
        public static string CleanText( string text, ref bool scriptTagsFound )
        {
            return CleanText( text, false, ref scriptTagsFound );
        }

        /// <summary>
        /// Remove all HTML and script tags
        /// </summary>
        /// <param name="text"></param>
        /// <param name="allowingHtmlPosts">If true, only check for script tags, and return inpout as is.</param>
        /// <param name="scriptTagsFound">True if any script tags found. This suggests need for a deliberate action/rebuke!</param>
        /// <returns></returns>
        public static string CleanText( string text, bool allowingHtmlPosts, ref bool scriptTagsFound )
        {
            if ( string.IsNullOrEmpty( text.Trim() ) )
                return string.Empty;

            string output = string.Empty;
            try
            {
                //don't know when the many individual check were removed, but the latter and regex are not handling stuff
                //		==> In BaseFactory
                //text = text.Replace( "-Ã‚Â­", "-" );
                ////Ã¢â‚¬â€œ
                ////this doesn't work
                //  text = text.Replace( " Ã¢â‚¬â€œ ­", " - " );
                ////so doing parts
                //if ( text.IndexOf( "Ã¢â" ) > -1 )
                //{
                //	text = text.Replace( "Ã¢â", "" );
                //	text = text.Replace( "¬", " - " );
                //	text = text.Replace( "â€œ", "" );
                //}
                if ( allowingHtmlPosts == false && HasHtmlTags( text ) )
                {
                    output = CleanHtml( text, ref scriptTagsFound );
                }
                else
                {
                    output = text;
                }
                //always reject if any script tags
                if ( output.ToLower().IndexOf( "<script" ) > -1
                    || output.ToLower().IndexOf( "javascript" ) > -1 )
                {
                    scriptTagsFound = true;
                    output = "";
                }
                //one last??
                //output = FilterText( output );
            }
            catch ( Exception ex )
            {
                //??
                output = "";
            }

            return output;
        }

        /// <summary>
        /// Remove all HTML and script tags
        /// </summary>
        /// <param name="text"></param>
        /// <param name="scriptTagsFound">True if any script tags found. This suggests need for a deliberate action/rebuke!</param>
        /// <returns></returns>
        public static string CleanHtml( string text, ref bool scriptTagsFound )
        {
            if ( string.IsNullOrEmpty( text.Trim() ) )
                return "";

            string output = "";
            try
            {

                //first attempt replacing some tags with a reasonable option
                text = ReplaceTagParts( text );
                //found cases of empty paragraph tags, so remove - doesn't buy much when skipping 
                text = Regex.Replace( text, @"<p>\s*</p>", "" ).Trim();
                string newline = "\n";
                //didn'like this
                //text = _emptyPRegex( text, string.Empty );
                //TODO - consider changing ending tags to a new line!
                //TODO - handling multi-level lists 
                //perhaps check for line breaks already being present?
                if ( text.IndexOf( newline ) == -1 )
                {
                    text = text.Replace( "</p>", Environment.NewLine );

                    //in the case of COOL there are link breaks in the Html, may not be the case for other sources
                    //text = text.Replace( "</li>", Environment.NewLine );

                    text = text.Replace( "</ul>", Environment.NewLine );
                    text = text.Replace( "</ol>", Environment.NewLine );
                    //may not be necessary
                    //text = text.Replace( "</table>", Environment.NewLine );
                }

                text = text.Replace( "<br/>", Environment.NewLine );
                text = text.Replace( "<br>", Environment.NewLine );
                text = text.Replace( "<li>", "- " );

                string rxPattern = "<(?>\"[^\"]*\"|'[^']*'|[^'\">])*>";
                Regex rx = new Regex( rxPattern );
                output = rx.Replace( text, String.Empty );

            }
            catch ( Exception ex )
            {
                //??
                output = text;
            }

            return output;
        }

        /// <summary>
        /// Custom handling of specific tags. 
        /// Currently handling acronym and anchor
        /// </summary>
        /// <returns></returns>
        public static string ReplaceTagParts( string input )
        {
            string output = "";
            if ( string.IsNullOrWhiteSpace( input ) )
                return "";

            output = ReplaceTagContentFromHtml( input, "acronym", "title", "{0} ({1})" );

            output = ReplaceTagContentFromHtml( output, "a", "href", "{1} (see: {0})" );

            return output;
        }
        /// <summary>
        /// Replace content of html tag like anchor with a desired format before stripping html.
        /// A recursive call to this method is performed at the end to handle all tags of specified type
        /// </summary>
        /// <param name="input"></param>
        /// <param name="htmlTag"></param>
        /// <param name="property">ex: href. TBD: input may include "=", although usually assumed. Will need to assume some delimiter like "/'</param>
        /// <param name="formatTemplate">Provide template to format output</param>
        /// <returns></returns>
        public static string ReplaceTagContentFromHtml( string input, string htmlTag, string property, string formatTemplate )
        {
            //actually may be a replace of the extracted pair of the html tag section
            //also may need to loop thru the full input document
            string output = "";
            if ( string.IsNullOrWhiteSpace( input ) )
                return "";
            string beginingTag = "<" + htmlTag + " "; //add space to prevent confusion for say <acronym
            string endingTag = string.Format( "</{0}>", htmlTag );
            int tagStart = input.IndexOf( beginingTag );        //ex. <a 
            if ( tagStart == -1 )
                return input;
            //
            int tagEnd = input.IndexOf( endingTag );
            if ( tagEnd == -1 ) //inconsistent, skip
                return input;
            //=======================================================
            string part1 = "";
            if ( tagStart > 0 )
                part1 = input.Substring( 0, tagStart );
            string part2 = input.Substring( tagStart, ( tagEnd - tagStart ) + endingTag.Length ); //confirm
            string part3 = input.Substring( tagEnd + endingTag.Length ); //confirm
            string insideText = "";
            string outsideText = "";
            //get start, will need to strip equals, and starting " or '
            //int insideStart = input.IndexOf( property, tagStart );	//ex. href=
            ////actually should be " or ' in case multiple interior properties like target, alt, etc
            //int insideTagEnd = input.IndexOf( ">", insideStart );
            //int insideTextEnd = input.IndexOf( "\"", insideStart );
            //if ( insideTextEnd == -1)
            //	insideTextEnd = input.IndexOf( "'", insideStart );

            //try general regex stuff
            ExtractTagParts( input, htmlTag, property, ref insideText, ref outsideText );

            //replace
            //caller must out the {0} and {1} in the desidered order
            output = part1
                    + string.Format( formatTemplate, insideText, outsideText )
                    + part3;

            //if more of the tag exists, call recursively
            //this could be an issue if a different format template would be preferred in a different context?
            if ( output.IndexOf( beginingTag ) > -1 )
            {
                output = ReplaceTagContentFromHtml( output, htmlTag, property, formatTemplate );
            }

            return output;
        }

        /// <summary>
        /// Extract tag parts and format for non-Html display
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tag"></param>
        /// <param name="property"></param>
        /// <param name="part1"></param>
        /// <param name="part2"></param>
        /// <returns></returns>
        private static bool ExtractTagParts( string input, string tag, string property, ref string part1, ref string part2 )
        {
            //example input
            //var html = "<a href=\"http://forum.tibia.com/forum/?action=board&boardid=476\">Amera</a><br><font class=\"ff_info\">This board is for general discussions related to the game world Amera.</font>";
            //var f1 = "<a\\b[^>]*?\\bhref=\"([^\"]*?)\"[^>]*?>(.*?)<\\/a>";
            var f2 = string.Format( "<{0}\\b[^>]*?\\b{1}=\"([^\"]*?)\"[^>]*?>(.*?)<\\/{0}>", tag, property );
            var match = Regex.Match( input, f2, RegexOptions.IgnoreCase );
            if ( match.Success )
            {
                part1 = match.Groups[1].ToString(); //not sure what would be returned
                part2 = match.Groups[2].ToString();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// This can be much faster than using a Regex method
        /// </summary>
        public static string StripTagsCharArray( string source )
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for ( int i = 0; i < source.Length; i++ )
            {
                char let = source[i];
                if ( let == '<' )
                {
                    inside = true;
                    continue;
                }
                if ( let == '>' )
                {
                    inside = false;
                    continue;
                }
                if ( !inside )
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string( array, 0, arrayIndex );
        }
        #endregion


    }
}
