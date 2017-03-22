using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using Newtonsoft.Json;

using BackupAirways.GestionSynchros;
using WebServer;

namespace BackupAirways.Gui
{
	public class WebGui
	{
		private readonly 	GestionnaireSynchros 	_gestionnaireSynchros;
		private 			Server 					_webServer;
		private				Thread					_threadWebServer;
		
		public				Thread					ThreadWebServer { get { return _threadWebServer; } }
		
		
		public WebGui(GestionnaireSynchros gestionnaireSynchros) {
			_gestionnaireSynchros = gestionnaireSynchros;
			
			#if DEBUG
			_webServer = new Server(C.PREFIXE);
			#else
			_webServer = new Server();
			#endif
						
			_webServer.AddAssembly(Assembly.GetExecutingAssembly(), "Gui.Web");
			
			_webServer.ajouteAction(CJS.ACTION__LISTE_DOSSIERS, 		this.getDossiers);
			_webServer.ajouteAction(CJS.ACTION__C_JS,			 		this.getConstantes);
			_webServer.ajouteAction(CJS.ACTION__CHEMINS_DRIVES, 		this.getDrivesPaths);
			_webServer.ajouteAction(CJS.ACTION__SET_DOSSIER_TAMPON,		this.setDossierTampon);
			_webServer.ajouteAction(CJS.ACTION__LISTE_SAUVEGARDES,		this.getSauvegardes);
			_webServer.ajouteAction(CJS.ACTION__ETAT_INITIALISATION,	this.etatInitialisation);
			_webServer.ajouteAction(CJS.ACTION__GET_NOM_MACHINE,		this.getNomMachine);
			_webServer.ajouteAction(CJS.ACTION__CHANGE_NOM_CLIENT,		this.changeNomClient);
			_webServer.ajouteAction(CJS.ACTION__GET_LISTE_CLIENTS,		this.getListeClients);
			_webServer.ajouteAction(CJS.ACTION__NOUVELLE_SAUVEGARDE,	this.nouvelleSynchro);
			_webServer.ajouteAction(CJS.ACTION__JOINDRE_SAUVEGARDE,		this.joindreSauvegarde);

			_threadWebServer = _webServer.start();
			
			File.WriteAllText(C.FICHIER_PREFIXE_WEB, _webServer.Prefixes[0]);
		}
		
		#region Actions Serveurs
		
		/// <summary>
		/// Transmet les constantes définies dans la classe CJS
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>Reponse WebServer des constantes au format json</returns>
		public WebReponse getConstantes (Dictionary<string, string> parametres)
		{
			var constantes 	= typeof(CJS).GetFields();
			var retours 	= new List<string>();
			
			
			foreach (FieldInfo constante in constantes)
			{
				retours.Add(constante.Name + " : \"" + constante.GetValue(null) + "\"");
			}
			
			return new WebReponse(Mime.js, "CJS = { " + String.Join(" , ", retours) + " };");
		}
		
		
		/// <summary>
		/// Transmet le nom de la machine
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>{ CJS.PARAM__NOM_MACHINE : nom de la machine }</returns>
		public WebReponse getNomMachine (Dictionary<string, string> parametres)
		{
			return new WebReponse(Mime.json, "{\"" + CJS.PARAM__NOM_MACHINE + "\" : \"" + System.Net.Dns.GetHostEntry("").HostName.Split('.')[0] + "\"}");
		}
		
		
		/// <summary>
		/// Renvoie l'état d'initialisation de l'application (nom et dossier tampon défini)
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public WebReponse etatInitialisation(Dictionary<string, string> parametres)
		{
			return new WebReponse(Mime.json, "{\"" + CJS.PARAM__EST_INITIALISE + "\" : " + (_gestionnaireSynchros.Initialise ? "true" : "false") + "}");
		}
		
		
		/// <summary>
		/// Renvoie la liste des clients découverts
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public WebReponse getListeClients(Dictionary<string, string> parametres)
		{
			return new WebReponse(Mime.json, JsonConvert.SerializeObject(_gestionnaireSynchros.Clients));
		}
		
		
		/// <summary>
		/// Renvoie les listes des sauvegardes
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>{ maitres : [sauvegardes dont le client est maitre], esclaves : [sauvegardes dont le client est esclave], inutilisees : [sauvegardes disponibles] }</returns>
		public WebReponse getSauvegardes (Dictionary<string, string> parametres)
		{	
			return new WebReponse(
				Mime.json,
				"{ \"maitres\" : " + JsonConvert.SerializeObject(_gestionnaireSynchros.SynchrosMaitre) + 
				", \"esclaves\" : " + JsonConvert.SerializeObject(_gestionnaireSynchros.SynchrosEsclave) +
				", \"inutilisees\" : " + JsonConvert.SerializeObject(_gestionnaireSynchros.SynchrosNonUtilisees) + 				
				"}");
		}
		
		
		/// <summary>
		/// Configure le dossier tampon qui sera utilisé par l'application
		/// </summary>
		/// <param name="parametres">CJS.PARAM__DOSSIER_TAMPON : dossier tampon</param>
		/// <returns></returns>
		public WebReponse setDossierTampon (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER_TAMPON))
			{
				_gestionnaireSynchros.setDossierTampon(parametres[CJS.PARAM__DOSSIER_TAMPON]);
				return new WebReponse(Mime.json, "{\"Message\" : \"ok\"}");
			}
			
			return new WebReponse(Mime.json, "{\"Erreur\" : \"erreur\"}");			
		}
		
		
		/// <summary>
		/// Configure le nom du client
		/// </summary>
		/// <param name="parametres">CJS.PARAM__NOM_MACHINE : nom du client</param>
		/// <returns></returns>
		public WebReponse changeNomClient (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__NOM_MACHINE)) {				
				_gestionnaireSynchros.changeNomClient(parametres[CJS.PARAM__NOM_MACHINE]);
				return new WebReponse(Mime.json, "{\"Message\" : \"ok\"}");
			}
			
			return new WebReponse(Mime.json, "{\"Erreur\" : \"erreur\"}");			
		}
		
		public WebReponse joindreSauvegarde (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER) && parametres.ContainsKey(CJS.PARAM__NOM_SAUVEGARDE))
			{
				_gestionnaireSynchros.joindreSynchro(parametres[CJS.PARAM__DOSSIER], parametres[CJS.PARAM__NOM_SAUVEGARDE]);
			}
			
			return new WebReponse(Mime.json, "{\"Erreur\" : \"erreur\"}");
		}
		
		/// <summary>
		/// Récupère les chemins des cloud installés sur la machine
		/// </summary>
		/// <param name="parametres"></param>
		/// <returns></returns>
		public WebReponse getDrivesPaths (Dictionary<string, string> parametres)
		{
			var retour 				= new Dictionary<string, string>();
			var cheminGoogleDrive 	= System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Google Drive";
			
			if (Directory.Exists(cheminGoogleDrive))
			{
				retour.Add(CJS.REP__CHEMIN_GDRIVE, cheminGoogleDrive);
			}
			
			return new WebReponse(Mime.json, JsonConvert.SerializeObject(retour));
		}
		
		public WebReponse nouvelleSynchro (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER) && parametres.ContainsKey(CJS.PARAM__NOM_SAUVEGARDE))
			{
				_gestionnaireSynchros.nouvelleSynchro(parametres[CJS.PARAM__DOSSIER],parametres[CJS.PARAM__NOM_SAUVEGARDE]);
				return WebReponse.OnePropJson(CJS.REP__ERREUR, "OK");
			}
			else
			{
				return WebReponse.OnePropJson(CJS.REP__ERREUR, "La demande ne contient pas les paramètres requis.");
			}
		}
		
		/// <summary>
		/// Renvoie les sous dossiers et/ou fichiers d'un dossier
		/// </summary>
		/// <param name="parametres"> - CJS.PARAM__DOSSIER : dossier à parcourir
		///  - CJS.PARAM__FICHIERS_SEUL : renvoie uniquement les fichiers si défini
		///  - CJS.PARAM__DOSSIERS_SEUL : renvoie uniquement les dossiers si défini.</param>
		/// <returns></returns>
		public WebReponse getDossiers (Dictionary<string, string> parametres)
		{
			string[] 	fichiers;
			DriveInfo[] listeDisques;
			
			var donnees 	= new List<Fichier>();
			
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER))
			{
				// Retourne la liste des disques
				if (parametres[CJS.PARAM__DOSSIER].Trim() == "" && !C.IS_LINUX)
				{
					listeDisques = DriveInfo.GetDrives();
					foreach (DriveInfo disque in listeDisques) {
						if (disque.DriveType == DriveType.Fixed || disque.DriveType == DriveType.Network) {
							donnees.Add(new Fichier(disque.Name, disque.VolumeLabel));
						}
					}
					return new WebReponse(Mime.json, JsonConvert.SerializeObject(donnees));
				}
				// Retourne la listes des sous-dossiers et fichiers
				else
				{	
					if (parametres [CJS.PARAM__DOSSIER].Trim () == "" && C.IS_LINUX) {
						parametres [CJS.PARAM__DOSSIER] = "/";
					}

					parametres[CJS.PARAM__DOSSIER] = parametres[CJS.PARAM__DOSSIER].WithEndingSlash();
					
					if (Directory.Exists(parametres[CJS.PARAM__DOSSIER])) {
						try {
							if (!parametres.ContainsKey(CJS.PARAM__FICHIERS_SEUL)) {
								
								fichiers = Directory.GetDirectories(parametres[CJS.PARAM__DOSSIER]);
								for (var i = 0; i< fichiers.Length; i++) {
									if (!U.IsSystem(fichiers[i])) {
										donnees.Add(new Fichier(fichiers[i]));
									}
								}		
							}
							
							if (!parametres.ContainsKey(CJS.PARAM__DOSSIERS_SEUL)) {
								fichiers = Directory.GetFiles(parametres[CJS.PARAM__DOSSIER]);
								for (var i = 0; i< fichiers.Length; i++) {
									if (!U.IsSystem(fichiers[i])) {
										donnees.Add(new Fichier(fichiers[i]));
									}
								}
							}
						}
						catch (Exception e)
						{
							return new WebReponse(Mime.json, "{\"" + CJS.REP__ERREUR + "\":\"" + e.Message.Replace(@"\",@"\\") + "\"}");
						}
										
						return new WebReponse(Mime.json, JsonConvert.SerializeObject(donnees));
					}
					else
					{
						return new WebReponse(Mime.json, "{\"" + CJS.REP__ERREUR + "\" : \"Le disque " + parametres[CJS.PARAM__DOSSIER] + " n'existe pas\"}");
					}
				}
			}
			else
			{
				return new WebReponse(Mime.json, "{\"" + CJS.REP__ERREUR + "\" : \"Le paramètre disque est absent\"}");
			}
		}
		#endregion
	}
}
