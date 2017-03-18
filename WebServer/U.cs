/*
 * Created by SharpDevelop.
 * User: Adrien
 * Date: 04/03/2017
 * Time: 08:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace WebServer
{
	/// <summary>
	/// Description of U.
	/// </summary>
	static internal class U
	{
		public static string ExtensionFichier(String fichier)
		{
			int posPoint = fichier.LastIndexOf('.');
			
			if (posPoint < 0) {
				return null;
			}
			return fichier.Substring(fichier.LastIndexOf('.') + 1);
		}
				
		public static byte[] ReadAllBytes(BinaryReader reader)
		{
			const int bufferSize = 4096;
			using (var ms = new MemoryStream()) {
				byte[] buffer = new byte[bufferSize];
				int count;
				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
					ms.Write(buffer, 0, count);
				return ms.ToArray();
			}
		}
		
		public static byte[] stringToBytes (string texte)
		{
			return Encoding.UTF8.GetBytes(texte);
		}
		
		public static Dictionary<string, string> lireParametres (HttpListenerContext ctx)
		{
			var retour = new Dictionary<string, string>();
			NameValueCollection prms;
						
			string post = "";
			
			
            if(ctx.Request.HttpMethod == "POST")
            {
                using(System.IO.StreamReader reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    post = reader.ReadToEnd();
                }
            }
                        
            prms = System.Web.HttpUtility.ParseQueryString(post);
            foreach (var key in prms.AllKeys)
            {
            	retour.Add(key, prms.Get(key));
            }			
			return retour;
		}
		
		public static int GetRandomUnusedPort()
		{
		    var listener = new TcpListener(IPAddress.Any, 0);
		    int port;
		    listener.Start();
		    port = ((IPEndPoint)listener.LocalEndpoint).Port;
		    listener.Stop();
		    return port;
		}
		
	
	}
}
