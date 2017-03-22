using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace WebServer
{
	/// <summary>
	/// Classe statiques de fonctions utiles
	/// </summary>
	static internal class U
	{
		
		/// <summary>
		/// Lit tous les octets d'un fichier
		/// </summary>
		/// <param name="reader">Un reader pointant sur le fichier à lire</param>
		/// <returns>Tableau d'octets représentant le contenu du fichier </returns>
		public static byte[] ReadAllBytes(BinaryReader reader) {
			const int bufferSize = 4096;
			using (var ms = new MemoryStream()) {
				var buffer = new byte[bufferSize];
				int count;
				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) {
					ms.Write(buffer, 0, count);
				}
				
				return ms.ToArray();
			}
		}
		
		/// <summary>
		/// Lit les paramètres intégrés à une requête HTTP
		/// </summary>
		/// <param name="ctx">Le contexte HTTP</param>
		/// <returns>Dictionnaire clés -> valeur</returns>
		public static Dictionary<string, string> lireParametres (HttpListenerContext ctx) {
			
			var 				retour 	= new Dictionary<string, string>();
			string 				post 	= "";
			NameValueCollection prms;
			
            if(ctx.Request.HttpMethod == "POST") {
                using(System.IO.StreamReader reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding)) {
                    post = reader.ReadToEnd();
                }
            }
                        
            prms = System.Web.HttpUtility.ParseQueryString(post);
            
            foreach (var key in prms.AllKeys) {
            	retour.Add(key, prms.Get(key));
            }			
            
			return retour;
		}
		
		/// <summary>
		/// Renvoie un port inutilisé au hasard
		/// </summary>
		/// <returns>Le numéro du port.</returns>
		public static int GetRandomUnusedPort() {
		    var listener = new TcpListener(IPAddress.Any, 0);
		    int port;
		    
		    listener.Start();
		    port = ((IPEndPoint)listener.LocalEndpoint).Port;
		    listener.Stop();
		    
		    return port;
		}
		
	
	}
}
