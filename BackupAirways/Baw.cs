// TODO: Gérer une liste des clients à part pour retrouver les clients liés à aucune sauvegarde
// TODO: -== FAIT ==- Ne mettre à jour le md5 que s'il y eu des modifications
// TODO: Ne générer les demandes que si elle n'existe pas déjà
// TODO: Gérer le déplacement d'un dossier de sauvegarde
// TODO: Ajouter la possibilité de créer un dossier depuis l'interface web

using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

using WebServer;

namespace Saw
{
	public sealed class Baw
	{
		/// <summary>
		/// Gestion singleton
		/// </summary>
		private static Baw instance = new Baw();
		public static Baw Instance { get { return instance; } }
		
		private Server 					_webServer;
		private Thread 					_threadWebServer;
		private Thread					_threadSynchros;
		private List<SynchroMaitre>		_synchrosMaitre 				= new List<SynchroMaitre>();
		private List<SynchroEsclave>	_synchrosEsclave 				= new List<SynchroEsclave>();
		private List<Synchro>			_synchrosNonUtilisees 			= new List<Synchro>();
		private bool					_traitementSynchroInitialise 	= false;
		private bool					_demandeArretSynchro			= false;
		private List<string>			_clients 						= new List<string>();
		
		private bool					_dossierTamponValide 	{ get { return Directory.Exists(C.DOSSIER_TAMPON); } }
		private bool					_nomDefini 				{ get { return C.NOM_CLIENT != ""; } }
		
		public List<SynchroMaitre> 		SynchrosMaitre 			{ get { return _synchrosMaitre; } }
		public List<SynchroEsclave> 	SynchrosEsclave 		{ get { return _synchrosEsclave; } }
		public List<Synchro> 			SynchrosNonUtilisees 	{ get { return _synchrosNonUtilisees; } }
		public bool						Initialise 				{ get { return _dossierTamponValide && _nomDefini; } }
		public List<string>				Clients					{ get { return _clients; } }
		
		
		/// <summary>
		/// Constructeur
		/// </summary>
		private Baw()
		{
			Logger.Log ("Is Linux :" + C.IS_LINUX);

			initWebserver();
			
			if (_dossierTamponValide)
			{				
				if (_nomDefini) {
					initTraitementSynchro();
				}
			}
		}
		
		
		/// <summary>
		/// Changement de nom du client
		/// </summary>
		/// <param name="nom">Nouveau nom</param>
		public void changeNomClient(string nom)
		{
			string fichierClient = Conf.Instance.DossierTampon + "\\." + nom + ".client";
			
			Conf.Instance.NomClient = nom;
			if (!File.Exists(fichierClient))
			{
				File.WriteAllText(fichierClient, string.Empty);
			}
			initTraitementSynchro();
		}
		
		/// <summary>
		/// Choix du dossier tampon
		/// </summary>
		/// <param name="chemin">Chemin du dossier tampon</param>
		public void setDossierTampon(string chemin)
		{
			chemin = chemin.WithoutEndingSlash();
			
			if (chemin.Substring(chemin.Length - 4) != ".saw")
			{
				chemin =  chemin + "\\.saw";
			}
			
			Directory.CreateDirectory(chemin);
			
			Conf.Instance.DossierTampon = chemin;
			
			recupereClients();
		}
		
		/// <summary>
		/// Récupère la liste des clients depuis le dossier tamps
		/// </summary>
		public void recupereClients()
		{
			foreach(string fichier in Directory.GetFiles(C.DOSSIER_TAMPON, "*.client", SearchOption.TopDirectoryOnly))
			{
				ajoutClient(Path.GetFileName(fichier).Split('.')[1]);
			}
		}
		
		/// <summary>
		/// Création d'une nouvelle synchro
		/// </summary>
		/// <param name="dossier">Dossier à synchroniser</param>
		/// <param name="nom">Nom de la synchro</param>
		public void nouvelleSynchro(string dossier, string nom)
		{
			Synchro.InitFolder(nom, C.NOM_CLIENT, dossier);
			_synchrosMaitre.Add(new SynchroMaitre(nom));
		}
		
		/// <summary>
		/// Rattachement à une synchro existante
		/// </summary>
		/// <param name="dossier">Dossier local hébergeant la synchro</param>
		/// <param name="nom">Nom de la synchro à rejoindre</param>
		public void joindreSynchro(string dossier, string nom)
		{
			Synchro.GetSynchros(ref _synchrosMaitre, ref _synchrosEsclave, ref _synchrosNonUtilisees);
			foreach (Synchro s in _synchrosNonUtilisees)
			{
				if (s.Nom == nom)
				{
					s.Rejoindre(dossier);
				}
			}
			Synchro.GetSynchros(ref _synchrosMaitre, ref _synchrosEsclave, ref _synchrosNonUtilisees);
		}
		
		/// <summary>
		/// Ajoute un client à la liste
		/// </summary>
		/// <param name="client">Nom du client</param>
		/// <returns>False si le client existe déjà dans la liste</returns>
		public bool ajoutClient(string client)
		{
			if (!_clients.Contains(client) && client != "")
			{
				_clients.Add(client);
				return true;
			}
			return false;
		}
		
		
		/// <summary>
		/// Supprime un client de la liste
		/// </summary>
		/// <param name="client">Nom du client</param>
		/// <returns>False si le client n'était pas dans la liste</returns>
		public bool supprimeClient(string client)
		{
			if (_clients.Contains(client))
			{
				_clients.Remove(client);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Lancement du traitement des synchros
		/// </summary>
		private void initTraitementSynchro()
		{
			if (!_traitementSynchroInitialise)
			{
				Logger.Information("Récupération des infos de synchro");
				
				_threadSynchros					= new Thread(boucleTraitementSynchros);
				_threadSynchros.Start();
				
				_traitementSynchroInitialise 	= true;
			}
		}
		
		/// <summary>
		/// Boucle principale de traitement des synchros
		/// </summary>
		private void boucleTraitementSynchros ()
		{
			var sw = new Stopwatch();
			
			while (!_demandeArretSynchro)
			{
				sw.Restart();
				Synchro.GetSynchros(ref _synchrosMaitre, ref _synchrosEsclave, ref _synchrosNonUtilisees);
				
				traiteSynchrosMaitres();
				traiteSynchrosEsclaves();
				Logger.Log("Fin du traitement des synchros : " + sw.ElapsedMilliseconds + "ms");
				sw.Stop();
				Thread.Sleep(C.INTERVALLE_SYNCHRO_MINUTES * 60 * 1000);
			}
		}
		
		/// <summary>
		/// Initialisation du serveur Web
		/// </summary>
		private void initWebserver()
		{
			_webServer = new Server(C.PREFIXE);
			
			_webServer.AddAssembly(Assembly.GetExecutingAssembly(), "Web");
			
			_webServer.ajouteAction(CJS.ACTION__LISTE_DOSSIERS, 		ActionsServeurs.getDossiers);
			_webServer.ajouteAction(CJS.ACTION__C_JS,			 		ActionsServeurs.getConstantes);
			_webServer.ajouteAction(CJS.ACTION__CHEMINS_DRIVES, 		ActionsServeurs.getDrivesPaths);
			_webServer.ajouteAction(CJS.ACTION__SET_DOSSIER_TAMPON,		ActionsServeurs.setDossierTampon);
			_webServer.ajouteAction(CJS.ACTION__LISTE_SAUVEGARDES,		ActionsServeurs.getSauvegardes);
			_webServer.ajouteAction(CJS.ACTION__ETAT_INITIALISATION,	ActionsServeurs.etatInitialisation);
			_webServer.ajouteAction(CJS.ACTION__GET_NOM_MACHINE,		ActionsServeurs.getNomMachine);
			_webServer.ajouteAction(CJS.ACTION__CHANGE_NOM_CLIENT,		ActionsServeurs.changeNomClient);
			_webServer.ajouteAction(CJS.ACTION__GET_LISTE_CLIENTS,		ActionsServeurs.getListeClients);
			_webServer.ajouteAction(CJS.ACTION__NOUVELLE_SAUVEGARDE,	ActionsServeurs.nouvelleSynchro);
			_webServer.ajouteAction(CJS.ACTION__JOINDRE_SAUVEGARDE,		ActionsServeurs.joindreSauvegarde);

			_threadWebServer = _webServer.start();
		}
		
		/// <summary>
		/// Traitement des synchros maitres
		/// </summary>
		private void traiteSynchrosMaitres()
		{
			long tailleDossier 	= U.tailleDossier(C.DOSSIER_TAMPON);
			long tailleMax		= C.TAILLE_MAX_TAMPON * 1024 * 1024;
			
			foreach(SynchroMaitre s in _synchrosMaitre)
			{
				if (s.Valide)
				{
					s.GenListeFichiersDbEtFichier();
					
					foreach (Reponse reponse in s.ReponsesExistantes())
					{
						if (!s.FichierExiste(reponse))
						{
							Logger.Log("Suppression de la réponse " + reponse.Fichier + " car le fichier correspondant n'existe plus");
							s.SupprimeTransaction(reponse);
						}
					}
					
					foreach (Demande demande in s.GetDemandes())
					{
						if (tailleDossier > tailleMax) break;
						
						if (!s.FichierExiste(demande))
						{
							Logger.Log("Suppression de la demande " + demande.Fichier + " car le fichier correspondant n'existe plus");
							s.SupprimeTransaction(demande);
						}
						else if (s.ReponseExiste(demande) == null)
						{
							Logger.Log("Fourniture du fichier " + demande.Md5f.Chemin);
							tailleDossier += s.FourniReponse(demande);
						}
					}	
				}
			}
		}
		
		/// <summary>
		/// Traitement des synchros esclaves
		/// </summary>
		private void traiteSynchrosEsclaves()
		{
			DeltaMd5 		delta;
			List<Demande> 	demandes;
			int 			demandesFaites;
			
			foreach(SynchroEsclave s in _synchrosEsclave)
			{
				if (s.Valide)
				{
					s.GenListeFichiersDbEtFichier();
					
					delta 			= s.DeltaFichiersAvecMaitre();
					demandes 		= s.GenDemandes(delta.Md5Ajoutes, 2 * C.MAX_DEMANDES_SIMULTANEES);
					demandesFaites 	= 0;
					
					List<Md5Fichier> md5sSupprimes = delta.Md5Supprimes;
					foreach (Md5Fichier md5Supprime in md5sSupprimes)
					{
						Logger.Log("Suppression du fichier " + md5Supprime.Chemin + " car le fichier correspondant n'existe plus");
						s.SupprimeFichier(md5Supprime);
					}
					
					foreach (Demande demande in demandes)
					{
						if (s.ReponseExiste(demande) != null)
						{
							Logger.Log("Récupération du fichier " + demande.FichierReponse + " (" + demande.Md5f.Chemin + ")");
							s.RecupereReponse(demande);
						}
						else
						{
							if (demandesFaites < C.MAX_DEMANDES_SIMULTANEES)
							{
								if (!s.FichierTransactionExiste(demande))
								{
									Logger.Log("Demande du fichier " + demande.Md5f.Chemin);
									s.FaireDemande(demande);
								}
								demandesFaites++;
							}
						}
					}		
				}
			}
		}
		
	}
}
