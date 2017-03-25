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
		private readonly	WebReponse				_reponseOk 				= WebReponse.OnePropJson(CJS.REP__MESSAGE, 	CJS.VAL__OK);
		private readonly	WebReponse				_reponseErreurParams 	= WebReponse.OnePropJson(CJS.REP__ERREUR, 	"Ereur de paramètres");
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
			
			_webServer.ajouteAction(CJS.ACTION__LISTE_DOSSIERS, 			this.getDossiers);
			_webServer.ajouteAction(CJS.ACTION__C_JS,			 			this.getConstantes);
			_webServer.ajouteAction(CJS.ACTION__CHEMINS_DRIVES, 			this.getDrivesPaths);
			_webServer.ajouteAction(CJS.ACTION__SET_DOSSIER_TAMPON,			this.setDossierTampon);
			_webServer.ajouteAction(CJS.ACTION__LISTE_SAUVEGARDES,			this.getSauvegardes);
			_webServer.ajouteAction(CJS.ACTION__ETAT_INITIALISATION,		this.etatInitialisation);
			_webServer.ajouteAction(CJS.ACTION__GET_NOM_MACHINE,			this.getNomMachine);
			_webServer.ajouteAction(CJS.ACTION__CHANGE_NOM_CLIENT,			this.changeNomClient);
			_webServer.ajouteAction(CJS.ACTION__GET_LISTE_CLIENTS,			this.getListeClients);
			_webServer.ajouteAction(CJS.ACTION__NOUVELLE_SAUVEGARDE,		this.nouvelleSynchro);
			_webServer.ajouteAction(CJS.ACTION__JOINDRE_SAUVEGARDE,			this.joindreSynchro);
			_webServer.ajouteAction(CJS.ACTION__SUPPRIME_SYNCHRO,			this.supprimeSynchro);
			_webServer.ajouteAction(CJS.ACTION__SUPPRIME_CLIENT_SYNCHRO,	this.supprimeClientSynchro);
			_webServer.ajouteAction(CJS.ACTION__CREATION_DOSSIER,			this.creerDossier);


			_threadWebServer = _webServer.start();
			
			File.WriteAllText(C.FICHIER_PREFIXE_WEB, _webServer.Prefixes[0]);
		}
		
		#region Actions Serveurs
		
		/// <summary>
		/// Transmet les constantes définies dans la classe CJS
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>Reponse WebServer des constantes au format json</returns>
		public WebReponse getConstantes (Dictionary<string, string> parametres) {
			var constantes 	= typeof(CJS).GetFields();
			var retours 	= new List<string>();
			
			
			foreach (FieldInfo constante in constantes) {
				retours.Add(constante.Name + " : \"" + constante.GetValue(null) + "\"");
			}
			
			return new WebReponse(Mime.js, "CJS = { " + String.Join(" , ", retours) + " };");
		}
		
		
		
		
		/// <summary>
		/// Transmet le nom de la machine
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>{ CJS.PARAM__NOM_MACHINE : nom de la machine }</returns>
		public WebReponse getNomMachine (Dictionary<string, string> parametres)	{
			return WebReponse.OnePropJson(CJS.PARAM__NOM_MACHINE, System.Net.Dns.GetHostEntry("").HostName.Split('.')[0]);
		}
		
		
		
		
		
		/// <summary>
		/// Renvoie l'état d'initialisation de l'application (nom et dossier tampon défini)
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public WebReponse etatInitialisation(Dictionary<string, string> parametres)	{
			return new WebReponse("{\"" + CJS.PARAM__EST_INITIALISE + "\" : " + (_gestionnaireSynchros.Initialise ? "true" : "false") + "}");
		}
		
		
		
		
		
		/// <summary>
		/// Renvoie la liste des clients découverts
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public WebReponse getListeClients(Dictionary<string, string> parametres) {
			return new WebReponse(JsonConvert.SerializeObject(_gestionnaireSynchros.Clients));
		}
		
		
		/// <summary>
		/// Renvoie les listes des sauvegardes
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>{ maitres : [sauvegardes dont le client est maitre], esclaves : [sauvegardes dont le client est esclave], inutilisees : [sauvegardes disponibles] }</returns>
		public WebReponse getSauvegardes (Dictionary<string, string> parametres) {	
			Dictionary<string, Object> retour = new Dictionary<string, object>();
			
			retour.Add(CJS.PARAM__NOM_MACHINE,			_gestionnaireSynchros.Conf.NomClient);
			retour.Add(CJS.PARAM__SYNCHROS_MAITRES, 	_gestionnaireSynchros.SynchrosMaitre);
			retour.Add(CJS.PARAM__SYNCHROS_ESCLAVES, 	_gestionnaireSynchros.SynchrosEsclave);
			retour.Add(CJS.PARAM__SYNCHROS_INUTILISEES, _gestionnaireSynchros.SynchrosInutilisees);
			
			return new WebReponse(JsonConvert.SerializeObject(retour));
		}
		
		
		
		
		/// <summary>
		/// Configure le dossier tampon qui sera utilisé par l'application
		/// </summary>
		/// <param name="parametres">CJS.PARAM__DOSSIER_TAMPON : dossier tampon</param>
		/// <returns></returns>
		public WebReponse setDossierTampon (Dictionary<string, string> parametres) {
			
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER_TAMPON)) {
				_gestionnaireSynchros.setDossierTampon(parametres[CJS.PARAM__DOSSIER_TAMPON]);
				return _reponseOk;
			}
			
			return _reponseErreurParams;			
		}
		
		
		/// <summary>
		/// Configure le nom du client
		/// </summary>
		/// <param name="parametres">CJS.PARAM__NOM_MACHINE : nom du client</param>
		/// <returns></returns>
		public WebReponse changeNomClient (Dictionary<string, string> parametres) {
			if (parametres.ContainsKey(CJS.PARAM__NOM_MACHINE)) {				
				_gestionnaireSynchros.changeNomClient(parametres[CJS.PARAM__NOM_MACHINE]);
				return _reponseOk;
			}
			
			return _reponseErreurParams;			
		}
		
		
		
		
		/// <summary>
		/// Configure une récupération locale d'une synchro existante
		/// </summary>
		/// <param name="parametres"> - CJS.PARAM__DOSSIER : dossier qui hébergera la synchro
		/// - CJS.PARAM__NOM_SYNCHRO : Nom de la synchro à rejoindre</param>
		/// <returns></returns>
		public WebReponse joindreSynchro (Dictionary<string, string> parametres) {
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER) && parametres.ContainsKey(CJS.PARAM__NOM_SYNCHRO)) {
				_gestionnaireSynchros.joindreSynchro(parametres[CJS.PARAM__DOSSIER], parametres[CJS.PARAM__NOM_SYNCHRO]);
				return _reponseOk;
			}
			
			return _reponseErreurParams;
		}
		
		
		
		
		/// <summary>
		/// Récupère les chemins des cloud installés sur la machine
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public WebReponse getDrivesPaths (Dictionary<string, string> parametres) {
			var retour 				= new Dictionary<string, string>();
			var cheminGoogleDrive 	= System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Google Drive";
			
			if (Directory.Exists(cheminGoogleDrive)) {
				retour.Add(CJS.REP__CHEMIN_GDRIVE, cheminGoogleDrive);
			}
			
			return new WebReponse(JsonConvert.SerializeObject(retour));
		}
		
		
		
		/// <summary>
		/// Création d'une nouvelle synchro depuis un dossier local
		/// </summary>
		/// <param name="parametres"> - CJS.PARAM__DOSSIER : le dossier à synchroniser
		/// - CJS.PARAM__NOM_SYNCHRO : le nom de la synchro</param>
		/// <returns></returns>
		public WebReponse nouvelleSynchro (Dictionary<string, string> parametres) {
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER) && parametres.ContainsKey(CJS.PARAM__NOM_SYNCHRO)) {
				_gestionnaireSynchros.nouvelleSynchro(parametres[CJS.PARAM__DOSSIER],parametres[CJS.PARAM__NOM_SYNCHRO]);
				return _reponseOk;
			}
			
			return _reponseErreurParams;
		}
		
		
		
		/// <summary>
		/// Suppression d'une synchro existante
		/// </summary>
		/// <param name="parametres">CJS.PARAM__NOM_SYNCHRO : nom de la synchro à supprimer</param>
		/// <returns></returns>
		public WebReponse supprimeSynchro (Dictionary<string, string> parametres) {
			
			if (parametres.ContainsKey(CJS.PARAM__NOM_SYNCHRO)) {
				_gestionnaireSynchros.supprimeSynchro(parametres[CJS.PARAM__NOM_SYNCHRO]);
				return _reponseOk;
			}
			
			return _reponseErreurParams;
		}
		
		
		/// <summary>
		/// Suppression d'un client d'une synchro
		/// </summary>
		/// <param name="parametres"> - CJS.PARAM__NOM_MACHINE : nom du client à désynchroniser
		/// - CJS.PARAM__NOM_SYNCHRO : le nom de la synchro</param>
		/// <returns></returns>
		public WebReponse supprimeClientSynchro (Dictionary<string, string> parametres) {
			
			if (parametres.ContainsKey(CJS.PARAM__NOM_SYNCHRO) && parametres.ContainsKey(CJS.PARAM__NOM_MACHINE)) {
				_gestionnaireSynchros.supprimeClientSynchro(parametres[CJS.PARAM__NOM_SYNCHRO], parametres[CJS.PARAM__NOM_MACHINE]);
				return _reponseOk;
			} 
			
			return _reponseErreurParams;
		}
		
		
		/// <summary>
		/// Création d'un dossier en local
		/// </summary>
		/// <param name="parametres">CJS.PARAM__DOSSIER : chemin du dossier à créer</param>
		/// <returns></returns>
		public WebReponse creerDossier (Dictionary<string, string> parametres) {
			
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER)) {
				Directory.CreateDirectory(parametres[CJS.PARAM__DOSSIER]);
				return _reponseOk;
			}
			
			return _reponseErreurParams;
		}
		
		
		
		
		/// <summary>
		/// Renvoie les sous dossiers et/ou fichiers d'un dossier
		/// </summary>
		/// <param name="parametres"> - CJS.PARAM__DOSSIER : dossier à parcourir ("" si racine)
		///  - CJS.PARAM__FICHIERS_SEUL : renvoie uniquement les fichiers si défini
		///  - CJS.PARAM__DOSSIERS_SEUL : renvoie uniquement les dossiers si défini.</param>
		/// <returns></returns>
		public WebReponse getDossiers (Dictionary<string, string> parametres) {
			string[] 	fichiers;
			DriveInfo[] listeDisques;
			
			var donnees 	= new List<Fichier>();
			
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER)) {
				
				// Retourne la liste des disques
				if (parametres[CJS.PARAM__DOSSIER].Trim() == "" && !C.IS_LINUX) {
					
					listeDisques = DriveInfo.GetDrives();
					foreach (DriveInfo disque in listeDisques) {
						if (disque.DriveType == DriveType.Fixed || disque.DriveType == DriveType.Network) {
							donnees.Add(new Fichier(disque.Name, disque.VolumeLabel));
						}
					}
					return new WebReponse(Mime.json, JsonConvert.SerializeObject(donnees));
					
				// retourne la liste des sous-dossiers / fichiers
				} else {	
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
						catch (Exception e)	{
							return WebReponse.OnePropJson(CJS.REP__ERREUR,  e.Message.Replace(@"\",@"\\"));
						}
										
						return new WebReponse(JsonConvert.SerializeObject(donnees));
					} else {
						return WebReponse.OnePropJson(CJS.REP__ERREUR,  "Le disque " + parametres[CJS.PARAM__DOSSIER] + " n'existe pas");
					}
				}
			} else {
				return _reponseErreurParams;
			}
		}
		#endregion
	}
}
