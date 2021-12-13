using System;
using System.Collections.Generic;
using System.Linq;

using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Navy.Utilities
{
	public static class JsonHelper
	{
		/// <summary>
		/// Get a JSONResult from an input object. Will return with JsonRequestBehavior set to AllowGet and MaxJsonLength set to Int32.MaxValue.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static JsonResult GetRawJson( object input )
		{
			var result = new JsonResult();
			result.Data = input;
			result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
			result.MaxJsonLength = Int32.MaxValue;
			return result;
		}

		public static JsonResult GetRawJson(string input)
		{
			var result = new JsonResult();
			result.Data = input;
			result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
			result.MaxJsonLength = Int32.MaxValue;
			return result;
		}
		/// <summary>
		/// Get a JSONResult from an input object with wrapper to help with client-side error handling.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <param name="extra"></param>
		/// <returns></returns>
		public static JsonResult GetJsonWithWrapper( object input, bool valid, string status, object extra )
		{
			var data = new
			{
				data = input,
				valid = valid,
				status = status,
				extra = extra
			};
			return GetRawJson( data );
		}
		public static JsonResult GetJsonWithWrapper( object input )
		{
			return GetJsonWithWrapper( input, true, "okay", null );
		}

		/// <summary>
		/// Get list of objects in a JSON-LD graph
		/// </summary>
		/// <param name="json"></param>
		/// <returns>JArray</returns>
		public static JArray GetGraphList( string json )
		{
			if ( string.IsNullOrWhiteSpace( json ) )
				return null; //??

			Dictionary<string, object> dictionary = JsonHelper.JsonToDictionary( json );
			object graph = dictionary[ "@graph" ];
			var glist = JsonConvert.SerializeObject( graph );
			//parse graph in to list of objects
			JArray graphList = JArray.Parse( glist );

			return graphList;
		}

		/// <summary>
		/// Generic handling of Json object - especially for unexpected types
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Dictionary<string, object> JsonToDictionary( string json )
		{
			var result = new Dictionary<string, object>();
			var obj = JObject.Parse( json );
			foreach ( var property in obj )
			{
				result.Add( property.Key, JsonToObject( property.Value ) );
			}
			return result;
		}
		public static object JsonToObject( JToken token )
		{
			switch ( token.Type )
			{
				case JTokenType.Object:
				{
					return token.Children<JProperty>().ToDictionary( property => property.Name, property => JsonToObject( property.Value ) );
				}
				case JTokenType.Array:
				{
					var result = new List<object>();
					foreach ( var obj in token )
					{
						result.Add( JsonToObject( obj ) );
					}
					return result;
				}
				default:
				{
					return ( ( JValue )token ).Value;
				}
			}
		}

		public static JsonSerializerSettings GetJsonSettings()
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				Formatting = Formatting.Indented
			};

			return settings;
		}
	}
}