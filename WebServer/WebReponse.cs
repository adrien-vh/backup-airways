/*
 * Created by SharpDevelop.
 * User: Adrien
 * Date: 05/03/2017
 * Time: 09:07
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;

namespace WebServer
{
	/// <summary>
	/// Description of Reponse.
	/// </summary>
	public class WebReponse
	{
		public Mime MimeType { get; set; }
		public byte[] Message { get; set; }
		
		/// <summary>
		/// WebRepons Json avec une seule propriété
		/// </summary>
		/// <param name="prop">Nom de la propriété</param>
		/// <param name="valeur">Valeur de la propriété</param>
		/// <returns>json du type {"prop" : "valeur"}</returns>
		public static WebReponse OnePropJson (string prop, string valeur)
		{
			return new WebReponse(Mime.json, "{\"" + prop + "\" : \"" + valeur + "\"}");
		}
		
		public WebReponse(Mime mimeType, byte[] message)
		{
			Message = message;
			MimeType = mimeType;
		}
		
		
		public WebReponse(Mime mimeType, string message)
		{
			Message = Encoding.UTF8.GetBytes(message);
			MimeType = mimeType;
		}
		
		public WebReponse()
		{
			Message = new byte[0];
			MimeType = Mime.defaut;
		}
	}
}
