/*
 * Created by SharpDevelop.
 * User: vanhyftea
 * Date: 03/03/2017
 * Time: 13:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Net;
using System.Threading;
using System.Text; 
using System.Collections.Generic;

namespace WebServer
{
	/// <summary>
	/// Serveur web
	/// </summary>
	public class Server : IDisposable
	{
		#region Propriétés
		/// <summary>
		/// Propriétés volatiles (potentiellement accédées par plusieurs Threads)
		/// </summary>
		private volatile HttpListener 	_listener;	// Listener HTTP
		private volatile bool 			_shouldRun;	// Indique si le serveur devrait tourner
		
		/// <summary>
		/// Propriétés privées
		/// </summary>
		private bool 															_isDisposed;				// Indique si l'instance a été "disposée"
		private Dictionary<string, Assembly> 									_assemblies;				// Assemblies proposant des ressources
		private List<string> 													_ressourcesDisponibles;		// Ressources disponibles
		private Dictionary<string, Func<Dictionary<string, string>, WebReponse>> 	_actionsDefinies;			// Actions pouvant être réalisées par le serveur
		
		/// <summary>
		/// Propriétés en lecture seule
		/// </summary>
		private string[]	_prefixes;
		public string[] 	Prefixes { get { return _prefixes; } }		// Préfixes sur lesquels écoute le serveur
		#endregion
		
		#region Constructeurs
		
		/// <param name="prefixe">Prefixe (eg : http://localhost:8000/)</param>
		public Server(string prefixe)
		{
			_prefixes = new string[] { prefixe};
			init();
		}
		
		/// <param name="port">Port d'écoute</param>
		/// <param name="localOnly">true : on écoute seulement sur l'adresse localhost, false : on écoute sur toutes les adresses</param>
		public Server(int port, bool localOnly)
		{
			initPrefixes(port, localOnly);
			init();
		}
		
		/// <param name="port">Port d'écoute</param>
		public Server(int port)
		{
			initPrefixes(port, true);
			init();
		}
		
		/// <summary>
		/// Le serveur écoutera sur le premier port disponible
		/// </summary>
		public Server()
		{
			initPrefixes(U.GetRandomUnusedPort(), true);
			init();
		}
			
				
		/// <summary>
		/// Initialisation des préfixes
		/// </summary>
		/// <param name="port">Port d'écoute</param>
		/// <param name="localOnly">true : on écoute seulement sur l'adresse localhost, false : on écoute sur toutes les adresses</param>
		private void initPrefixes(int port, bool localOnly)
		{
			if (localOnly) {
				_prefixes = new string[] { "http://localhost:" + port.ToString() + "/" };
				
			} else {
				_prefixes = new string[] { "http://*:" + port.ToString() + "/" };
			}
			
		}
		
		/// <summary>
		/// Initialisation du listener http
		/// </summary>
		private void init()
		{
			_isDisposed 			= false;
			_shouldRun 				= false;
			_actionsDefinies 		= new Dictionary<string, Func<Dictionary<string, string>, WebReponse>>();
			_assemblies 			= new Dictionary<string, Assembly>();
			_ressourcesDisponibles 	= new List<string>();
			
			_listener = new HttpListener();
			

			foreach (string prefixe in _prefixes) {
				_listener.Prefixes.Add(prefixe);
			}
			
			
		}
		
		#endregion
		
		#region Méthodes publiques
		
		/// <summary>
		/// Ajoute une assembly dans les ressources disponibles
		/// </summary>
		/// <param name="assembly">Assembly à ajouter</param>
		/// <param name="chemin">Chemin contenant les ressources</param>
		public void AddAssembly(Assembly assembly, string chemin)
		{
			var resourcesNames = assembly.GetManifestResourceNames();
			var prefixe = assembly.FullName.Substring(0, assembly.FullName.IndexOf(',')) + "." + chemin;
			
			_assemblies.Add(prefixe, assembly);
			
			foreach (var resource in resourcesNames) {
				if (resource.StartsWith(prefixe))
			    {
					_ressourcesDisponibles.Add(resource);
			    }
			}
		}
		
		/// <summary>
		/// Ajoute une action appelée par prefixe/nomAction
		/// </summary>
		/// <param name="nomAction">nom de l'action</param>
		/// <param name="action">fonction appelée devant retournée un objet de type Reponse</param>
		public void ajouteAction (string nomAction, Func<Dictionary<string, string>, WebReponse> action)
		{
			_actionsDefinies.Add(nomAction.ToLower(), action);
		}
		
		/// <summary>
		/// Démarrage du serveur
		/// </summary>
		public Thread start()
		{
			Thread threadServeur = new Thread(demarreServeur);
			threadServeur.Start();
			return threadServeur;
		}
		
		/// <summary>
		/// Arrêt du serveur
		/// </summary>
		public void stop()
		{
			_shouldRun = false;
			_listener.Stop();
		}
		
		#endregion
		
		#region Méthodes utiles
		
		/// <summary>
		/// Test si une action existe
		/// </summary>
		/// <param name="action">nom de l'action à tester</param>
		/// <returns>True si l'action est définie, False sinon</returns>
		private bool actionExiste(string action)
		{
			return _actionsDefinies.ContainsKey(action);
		}
		
		/// <summary>
		/// Lecture du contenu d'une ressource
		/// </summary>
		/// <param name="resource">chemin de la ressource (sans le préfixe)</param>
		/// <returns>Le contenu de la ressource si elle existe, null sinon</returns>
		private byte[] ContenuResource(string resource)
		{
			foreach(KeyValuePair<string, Assembly> assembly in _assemblies)
			{
				if (_ressourcesDisponibles.Contains(assembly.Key + "." + resource))
			    {
			    	Stream streamResource;
					BinaryReader readerResource;
					
					streamResource = assembly.Value.GetManifestResourceStream(assembly.Key + "." + resource);
					readerResource = new BinaryReader(streamResource);
					
					return U.ReadAllBytes(readerResource);
			    }
			}


			return null;
		}
		
		#endregion
		
		/// <summary>
		/// Démarrage du serveur appelé dans un Thread à part
		/// </summary>
		private void demarreServeur()
		{
			try {
				_listener.Start();
				_shouldRun = true;
			} catch (HttpListenerException ex) {
				Logger.Log(ex.Message, global::Logger.LogLevel.ERROR);
				_shouldRun = false;
			}

			if (_shouldRun) {
				Logger.Log("Serveur démarré sur : " + String.Join(",", _prefixes));
			}
			
			while (_listener.IsListening) {
				try
				{
					HttpListenerContext request = _listener.GetContext();
					ThreadPool.QueueUserWorkItem(ProcessRequest, request); 
				}
				catch (Exception e)
				{
					Logger.Log(e.Message, global::Logger.LogLevel.ERROR);
				}
			}
			Logger.Log("Le serveur est arrêté");
		}
		
		/// <summary>
		/// Traitement d'une requête
		/// </summary>
		/// <param name="listenerContext">Contexte du listener</param>
		private void ProcessRequest(object listenerContext)
		{ 
			Dictionary<string, string> 	parametres;
			WebReponse 					reponse;
			
			var 	context 		= (HttpListenerContext)listenerContext;
			var 	cheminDemande 	= context.Request.Url.LocalPath.Substring(1).Replace('/','.');
			var 	type 			= "";
			var 	nomAction 		= context.Request.Url.LocalPath.Substring(1).Split('/')[0].ToLower();
			int 	codeStatus 		= (int)HttpStatusCode.OK;
			
			if (cheminDemande == "") { cheminDemande = "index.html"; }
			
			byte[] 	contenuResource = ContenuResource(cheminDemande);
			
			if (contenuResource != null)
			{
				type = "ressource";
				reponse = new WebReponse(Mime.mimeFromNomFichier(cheminDemande), contenuResource);	
			}
			else if (nomAction == "__stop")
			{
				stop();
				return;
			}
			else if (actionExiste(nomAction))
			{
				type = "action";
				parametres = U.lireParametres(context);
				reponse = _actionsDefinies[nomAction](parametres);
				//Logger.Information(reponse.Message);
			}
			else
			{
				type = "erreur";
				codeStatus = (int)HttpStatusCode.NotFound;
				reponse = new WebReponse(Mime.json, U.stringToBytes("{message : \"Message vide\"}"));				
			}
			
			//Logger.Information("Méthode : " + ctx.Request.HttpMethod);
			Logger.Log(context.Request.RemoteEndPoint.ToString() + " - " + context.Request.IsLocal.ToString() + " - " + context.Request.HttpMethod + " - " + type + " - " + context.Request.Url.LocalPath);
			
			context.Response.StatusCode = codeStatus;
			context.Response.AddHeader("Content-Type", reponse.MimeType);
			context.Response.AddHeader("Access-Control-Allow-Origin", "*");
			context.Response.ContentLength64 = reponse.Message.Length; 
	        context.Response.OutputStream.Write(reponse.Message, 0, reponse.Message.Length);
	        context.Response.OutputStream.Close();
			
		}
		

		
		public void Dispose()
		{
			if (!_isDisposed) {
				_isDisposed = true;
				_shouldRun = false;

				try {
					if (_listener != null) {
						_listener.Stop();
					}
				} catch (Exception ex) {
					Logger.Log(ex.Message, global::Logger.LogLevel.ERROR);
				}
				Logger.Log("Web Server disposed");
			}
		}
	}
}
