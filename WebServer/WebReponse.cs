using System;
using System.Text;

namespace WebServer
{
	/// <summary>
	/// Réponse du serveur Web
	/// </summary>
	public class WebReponse
	{
		public Mime MimeType;
		public byte[] Message;
		
		/// <summary>
		/// WebReponse Json avec une seule propriété
		/// </summary>
		/// <param name="prop">Nom de la propriété</param>
		/// <param name="valeur">Valeur de la propriété</param>
		/// <returns>json du type {"prop" : "valeur"}</returns>
		public static WebReponse OnePropJson (string prop, string valeur) {
			return new WebReponse(Mime.json, "{\"" + prop + "\" : \"" + valeur.Replace("\\", "\\\\") + "\"}");
		}
		
		/// <summary>
		/// WebReponse standard depuis un tableau d'octets
		/// </summary>
		/// <param name="mimeType">Type mime de la réponse</param>
		/// <param name="message">Tableau d'octets représentant le contenu de la réponse</param>
		public WebReponse(Mime mimeType, byte[] message) {
			Message = message;
			MimeType = mimeType;
		}
		
		/// <summary>
		/// WebReponse standard depuis une chaîne de caractères
		/// </summary>
		/// <param name="mimeType">Type mime de la réponse</param>
		/// <param name="message">Tableau d'octets représentant le contenu de la réponse</param>
		public WebReponse(Mime mimeType, string message) {
			Message = Encoding.UTF8.GetBytes(message);
			MimeType = mimeType;
		}
		
				
		/// <summary>
		/// WebReponse Json depuis une chaîne de caractères
		/// </summary>
		/// <param name="mimeType">Type mime de la réponse</param>
		/// <param name="message">Tableau d'octets représentant le contenu de la réponse</param>
		public WebReponse(string message) {
			Message = Encoding.UTF8.GetBytes(message);
			MimeType = Mime.json;
		}

	}
}
