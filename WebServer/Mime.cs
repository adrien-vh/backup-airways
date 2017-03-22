/*
 * Created by SharpDevelop.
 * User: Adrien
 * Date: 04/03/2017
 * Time: 08:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace WebServer
{
	/// <summary>
	/// Types Mime
	/// </summary>
	public class Mime
	{
		private Mime(string type) { Type = type; }
		
		private static readonly Dictionary<string, string> _mimeTypes = new Dictionary<string, string> {
			{"html", "text/html; charset=utf-8" },
			{"css", "text/css; charset=utf-8" },
			{"ico", "image/x-icon" },
			{"jpg", "image/jpeg" },
			{"jpeg", "image/jpeg" },
			{"bmp", "image/bmp" },
			{"png", "image/png" },
			{"js", "text/javascript; charset=utf-8" },
			{"map", "application/json; charset=utf-8" },
			{"json", "application/json; charset=utf-8" },
			{"txt", "text/plain; charset=utf-8" },
			{"*", "application/octet-stream" },
		};
		
		public string Type { get; set; }
		
		public static implicit operator string(Mime m) {
            return m.Type;
        }
		
		public static Mime html 	{ get { return new Mime(_mimeTypes["html"]); } }
		public static Mime css 		{ get { return new Mime(_mimeTypes["css"]); } }
		public static Mime ico 		{ get { return new Mime(_mimeTypes["ico"]); } }
		public static Mime jpg 		{ get { return new Mime(_mimeTypes["jpg"]); } }
		public static Mime bmp 		{ get { return new Mime(_mimeTypes["bmp"]); } }
		public static Mime png 		{ get { return new Mime(_mimeTypes["png"]); } }
		public static Mime js 		{ get { return new Mime(_mimeTypes["js"]); } }
		public static Mime map 		{ get { return new Mime(_mimeTypes["map"]); } }
		public static Mime json 	{ get { return new Mime(_mimeTypes["json"]); } }
		public static Mime txt 		{ get { return new Mime(_mimeTypes["txt"]); } }
		public static Mime defaut 	{ get { return new Mime(_mimeTypes["*"]); } }

		/// <summary>
		/// Récupère le type mime en fonction de l'extension du fichier
		/// </summary>
		/// <param name="nomFichier">Le chemin du fichier concerné</param>
		/// <returns>Le type mime du fichier</returns>
		public static Mime mimeFromNomFichier (string nomFichier) {
			String extension = Path.GetExtension(nomFichier).Substring(1).ToLower();
			
			return _mimeTypes.ContainsKey(extension) ? new Mime(_mimeTypes[extension]) : new Mime(_mimeTypes["*"]);
			
		}
	}
}
