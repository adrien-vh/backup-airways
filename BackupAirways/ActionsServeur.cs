using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Net;

using Newtonsoft.Json;

using WebServer;

namespace Saw
{
	internal static class ActionsServeurs
	{		
		/// <summary>
		/// Transmet les constantes définies dans la classe CJS
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>Reponse WebServer des constantes au format json</returns>
		public static WebReponse getConstantes (Dictionary<string, string> parametres)
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
		public static WebReponse getNomMachine (Dictionary<string, string> parametres)
		{
			return new WebReponse(Mime.json, "{\"" + CJS.PARAM__NOM_MACHINE + "\" : \"" + System.Net.Dns.GetHostEntry("").HostName.Split('.')[0] + "\"}");
		}
		
		
		/// <summary>
		/// Renvoie l'état d'initialisation de l'application (nom et dossier tampon défini)
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public static WebReponse etatInitialisation(Dictionary<string, string> parametres)
		{
			return new WebReponse(Mime.json, "{\"" + CJS.PARAM__EST_INITIALISE + "\" : " + (Baw.Instance.Initialise ? "true" : "false") + "}");
		}
		
		
		/// <summary>
		/// Renvoie la liste des clients découverts
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns></returns>
		public static WebReponse getListeClients(Dictionary<string, string> parametres)
		{
			return new WebReponse(Mime.json, JsonConvert.SerializeObject(Baw.Instance.Clients));
		}
		
		
		/// <summary>
		/// Renvoie les listes des sauvegardes
		/// </summary>
		/// <param name="parametres">Non utilisé</param>
		/// <returns>{ maitres : [sauvegardes dont le client est maitre], esclaves : [sauvegardes dont le client est esclave], inutilisees : [sauvegardes disponibles] }</returns>
		public static WebReponse getSauvegardes (Dictionary<string, string> parametres)
		{	
			return new WebReponse(
				Mime.json,
				"{ \"maitres\" : " + JsonConvert.SerializeObject(Baw.Instance.SynchrosMaitre) + 
				", \"esclaves\" : " + JsonConvert.SerializeObject(Baw.Instance.SynchrosEsclave) +
				", \"inutilisees\" : " + JsonConvert.SerializeObject(Baw.Instance.SynchrosNonUtilisees) + 				
				"}");
		}
		
		
		/// <summary>
		/// Configure le dossier tampon qui sera utilisé par l'application
		/// </summary>
		/// <param name="parametres">CJS.PARAM__DOSSIER_TAMPON : dossier tampon</param>
		/// <returns></returns>
		public static WebReponse setDossierTampon (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER_TAMPON))
			{
				Baw.Instance.setDossierTampon(parametres[CJS.PARAM__DOSSIER_TAMPON]);
				return new WebReponse(Mime.json, "{\"Message\" : \"ok\"}");
			}
			
			return new WebReponse(Mime.json, "{\"Erreur\" : \"erreur\"}");			
		}
		
		
		/// <summary>
		/// Configure le nom du client
		/// </summary>
		/// <param name="parametres">CJS.PARAM__NOM_MACHINE : nom du client</param>
		/// <returns></returns>
		public static WebReponse changeNomClient (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__NOM_MACHINE))
			{				
				Baw.Instance.changeNomClient(parametres[CJS.PARAM__NOM_MACHINE]);
				return new WebReponse(Mime.json, "{\"Message\" : \"ok\"}");
			}
			
			return new WebReponse(Mime.json, "{\"Erreur\" : \"erreur\"}");			
		}
		
		public static WebReponse joindreSauvegarde (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER) && parametres.ContainsKey(CJS.PARAM__NOM_SAUVEGARDE))
			{
				Baw.Instance.joindreSynchro(parametres[CJS.PARAM__DOSSIER], parametres[CJS.PARAM__NOM_SAUVEGARDE]);
			}
			
			return new WebReponse(Mime.json, "{\"Erreur\" : \"erreur\"}");
		}
		
		/// <summary>
		/// Récupère les chemins des cloud installés sur la machine
		/// </summary>
		/// <param name="parametres"></param>
		/// <returns></returns>
		public static WebReponse getDrivesPaths (Dictionary<string, string> parametres)
		{
			var retour 				= new Dictionary<string, string>();
			var cheminGoogleDrive 	= System.Environment.GetEnvironmentVariable("USERPROFILE") + "\\Google Drive";
			
			if (Directory.Exists(cheminGoogleDrive))
			{
				retour.Add(CJS.REP__CHEMIN_GDRIVE, cheminGoogleDrive);
			}
			
			return new WebReponse(Mime.json, JsonConvert.SerializeObject(retour));
		}
		
		public static WebReponse nouvelleSynchro (Dictionary<string, string> parametres)
		{
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER) && parametres.ContainsKey(CJS.PARAM__NOM_SAUVEGARDE))
			{
				Baw.Instance.nouvelleSynchro(parametres[CJS.PARAM__DOSSIER],parametres[CJS.PARAM__NOM_SAUVEGARDE]);
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
		public static WebReponse getDossiers (Dictionary<string, string> parametres)
		{
			string[] 	fichiers;
			DriveInfo[] listeDisques;
			
			var donnees 	= new List<Fichier>();
			
			if (parametres.ContainsKey(CJS.PARAM__DOSSIER))
			{
				// Retourne la liste des disques
				if (parametres[CJS.PARAM__DOSSIER].Trim() == "")
				{
					listeDisques = DriveInfo.GetDrives();
					foreach (DriveInfo disque in listeDisques)
					{
						if (disque.DriveType == DriveType.Fixed || disque.DriveType == DriveType.Network)
						{
							donnees.Add(new Fichier(disque.Name, disque.VolumeLabel));
						}
					}
					return new WebReponse(Mime.json, JsonConvert.SerializeObject(donnees));
				}
				// Retourne la listes des sous-dossiers et fichiers
				else
				{					
					parametres[CJS.PARAM__DOSSIER] = parametres[CJS.PARAM__DOSSIER].WithEndingSlash();
					
					if (Directory.Exists(parametres[CJS.PARAM__DOSSIER]))
					{
						try
						{
							if (!parametres.ContainsKey(CJS.PARAM__FICHIERS_SEUL))
							{
								
								fichiers = Directory.GetDirectories(parametres[CJS.PARAM__DOSSIER]);
								for (var i = 0; i< fichiers.Length; i++)
								{
									if (!U.IsSystem(fichiers[i])) {
										donnees.Add(new Fichier(fichiers[i]));
									}
								}		
								
							}
							if (!parametres.ContainsKey(CJS.PARAM__DOSSIERS_SEUL))
							{
								fichiers = Directory.GetFiles(parametres[CJS.PARAM__DOSSIER]);
								for (var i = 0; i< fichiers.Length; i++)
								{
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
	}
}
