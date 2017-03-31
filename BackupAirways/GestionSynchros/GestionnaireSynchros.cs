using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Newtonsoft.Json;

using BackupAirways.Synchros;


namespace BackupAirways.GestionSynchros
{
	/// <summary>
	/// Gestion des synchros
	/// </summary>
	public class GestionnaireSynchros
	{		
		private bool					_traitementSynchroInitialise 	= false;
		private Thread					_threadSynchros;
		private bool					_demandeArretSynchro			= false;
		private List<string>			_clients 						= new List<string>();
		
		private Conf					_conf;
		
		private List<SynchroMaitre>		_synchrosMaitre 				= new List<SynchroMaitre>();
		private List<SynchroEsclave>	_synchrosEsclave 				= new List<SynchroEsclave>();
		private List<Synchro>			_synchrosNonUtilisees 			= new List<Synchro>();
		
		private string					_nomClient				{ get { return _conf.NomClient; } }
		private string					_dossierSynchros		{ get { return _conf.DossierTampon; } }
		private bool					_dossierTamponValide 	{ get { return Directory.Exists(_conf.DossierTampon); } }
		private bool					_nomDefini 				{ get { return _conf.NomClient != ""; } }
		
		public bool						Initialise 				{ get { return _dossierTamponValide && _nomDefini; } }
		public List<string>				Clients					{ get { return _clients; } }
		
		public List<SynchroMaitre> 		SynchrosMaitre 			{ get { return _synchrosMaitre; } }
		public List<SynchroEsclave> 	SynchrosEsclave 		{ get { return _synchrosEsclave; } }
		public List<Synchro> 			SynchrosInutilisees 	{ get { return _synchrosNonUtilisees; } }
		public Conf						Conf					{ get { return _conf; } }
		
		/// <summary>
		/// Constructure
		/// </summary>
		/// <param name="conf">Configuration de l'application</param>
		public GestionnaireSynchros(Conf conf) {
			_conf				= conf;
			
			if (_dossierTamponValide) {
				recupereClients();
			}
			
			if (Initialise) {
				DemarreSynchros();
			}
		}
		
		
		/// <summary>
		/// Changement de nom du client
		/// </summary>
		/// <param name="nom">Nouveau nom</param>
		public void changeNomClient(string nom) {
			string fichierClientActuel 	= _dossierSynchros + "\\." + _nomClient + ".client";
			string nouveauFichierClient = _dossierSynchros + "\\." + nom + ".client";
			
			if (File.Exists(fichierClientActuel)) {
				File.Delete(fichierClientActuel);
			}
						
			_conf.NomClient = nom;
			
			if (!File.Exists(nouveauFichierClient)) {
				File.WriteAllText(nouveauFichierClient, string.Empty);
			}
			
			DemarreSynchros();
		}
		
		/// <summary>
		/// Récupère les synchros configurées en analysant le dossier Tampon
		/// </summary>
		private void getSynchros() {
			ConfSynchro	confSynchro;
			string[]	chemins;
			
			_synchrosMaitre.Clear();
			_synchrosEsclave.Clear();
			_synchrosNonUtilisees.Clear();
			
			foreach(string fichier in Directory.GetFiles(_conf.DossierTampon, C.FICHIER_CONF_SYNCHRO, SearchOption.AllDirectories))	{
	
				chemins = fichier.Replace(_conf.DossierTampon + "\\", "").Split('\\');
				
				if (chemins.Length == 2) {
					confSynchro = JsonConvert.DeserializeObject<ConfSynchro>(File.ReadAllText(fichier));
															
					if (confSynchro.Client == _conf.NomClient) {
						_synchrosMaitre.Add(new SynchroMaitre(chemins[0], _conf));
					}
					
					#if DEBUG
					if (File.Exists(_conf.DossierTampon + "\\" + chemins[0] + "\\." + _conf.NomClient + ".client")) {
					#else
					else if (File.Exists(_conf.DossierTampon + "\\" + chemins[0] + "\\." + _conf.NomClient + ".client")) {
					#endif
						_synchrosEsclave.Add(new SynchroEsclave(chemins[0], _conf));
					} else {
						_synchrosNonUtilisees.Add(new Synchro(chemins[0], _conf, TypeSynchro.Inutilisee));
					}
				}
			}
		}	
		
		/// <summary>
		/// Récupère la liste des clients depuis le dossier tampon
		/// </summary>
		private void recupereClients()
		{
			string client;
			
			foreach(string fichier in Directory.GetFiles(_dossierSynchros, "*.client", SearchOption.TopDirectoryOnly)) {
				client = Path.GetFileName(fichier).Split('.')[1];
				
				if (!_clients.Contains(client) && client != "")	{
					_clients.Add(client);
				}
			}
		}
			
		/// <summary>
		/// Supprime un client de la liste
		/// </summary>
		/// <param name="client">Nom du client</param>
		/// <returns>False si le client n'était pas dans la liste</returns>
		private bool supprimeClient(string client) {
			if (_clients.Contains(client)) {
				_clients.Remove(client);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Initie un dossier pour une nouvelle synchro
		/// </summary>
		/// <param name="nom">Nom de la synchro</param>
		/// <param name="maitre">Nom de la machine maître</param>
		/// <param name="dossier">Dossier à synchroniser</param>
		private void InitSynchroFolder(string nom, string maitre, string dossier) {
			var 	confSynchro 			= new ConfSynchro(maitre, dossier);
			string 	dossierTamponSynchro 	= _conf.DossierTampon + "\\" + nom;
			string 	fichierConf				= dossierTamponSynchro + "\\" + C.FICHIER_CONF_SYNCHRO;
			
			Directory.CreateDirectory(dossierTamponSynchro);
			File.WriteAllText(fichierConf, JsonConvert.SerializeObject(confSynchro));
			
		}
		
		
		/// <summary>
		/// Création d'une nouvelle synchro
		/// </summary>
		/// <param name="dossier">Dossier à synchroniser</param>
		/// <param name="nom">Nom de la synchro</param>
		public void nouvelleSynchro(string dossier, string nom) {
			InitSynchroFolder(nom, _nomClient, dossier);
			_synchrosMaitre.Add(new SynchroMaitre(nom, _conf));
		}
		
		/// <summary>
		/// Suppression d'une synchro existante
		/// </summary>
		/// <param name="nom">Nom de la synchro</param>
		public void supprimeSynchro(string nom) {
			try {
				Directory.Delete(_dossierSynchros + "\\" + nom, true);	
			} catch (Exception e) {
				Logger.Log(e.Message, global::Logger.LogLevel.ERROR);
			}
			
			getSynchros();
		}
		
		/// <summary>
		/// Détachement d'un client d'une synchro
		/// </summary>
		/// <param name="nomSynchro">Nom de la synchro</param>
		/// <param name="client">Nom du client</param>
		public void supprimeClientSynchro(string nomSynchro, string client) {
			string fichierMd5 			= _dossierSynchros + "\\" + nomSynchro + "\\" + client + ".md5";
			string fichierConfClient 	= _dossierSynchros + "\\" + nomSynchro + "\\." + client + ".client";
			
			if (File.Exists(fichierMd5)) {
				File.Delete(fichierMd5);
			}
			
			if (File.Exists(fichierConfClient)) {
				File.Delete(fichierConfClient);
			}
			
			getSynchros();
		}
		
		/// <summary>
		/// Rattachement à une synchro existante
		/// </summary>
		/// <param name="dossier">Dossier local hébergeant la synchro</param>
		/// <param name="nom">Nom de la synchro à rejoindre</param>
		public void joindreSynchro(string dossier, string nom) {
			getSynchros();
			foreach (Synchro s in _synchrosNonUtilisees) {
				if (s.Nom == nom) {
					s.Rejoindre(dossier);
				}
			}
			getSynchros();
		}
		
		/// <summary>
		/// Lancement du traitement des synchros
		/// </summary>
		public void DemarreSynchros() {
			if (!_traitementSynchroInitialise && Initialise) {
				Logger.Log("Récupération des infos de synchro");
				
				_threadSynchros					= new Thread(boucleTraitementSynchros);
				_threadSynchros.Start();
				
				_traitementSynchroInitialise 	= true;
			}
		}
		
		/// <summary>
		/// Choix du dossier tampon
		/// </summary>
		/// <param name="chemin">Chemin du dossier tampon</param>
		public void setDossierTampon(string chemin) {
			chemin = chemin.WithoutEndingSlash();
			
			if (chemin.Substring(chemin.Length - 4) != ".saw") {
				chemin =  chemin + "\\.saw";
			}
			
			Directory.CreateDirectory(chemin);
			
			_conf.DossierTampon = chemin;
		}
		
		/// <summary>
		/// Boucle principale de traitement des synchros
		/// </summary>
		private void boucleTraitementSynchros () {
			var sw = new Stopwatch();
			
			while (!_demandeArretSynchro) {
				sw.Restart();

				getSynchros();
				
				traiteSynchrosMaitres();
				traiteSynchrosEsclaves();
				Logger.Log("Fin du traitement des synchros : " + sw.ElapsedMilliseconds + "ms");
				sw.Stop();
				Thread.Sleep(C.INTERVALLE_SYNCHRO_MINUTES * 60 * 1000);
				//Thread.Sleep(2000);

			}
		}
		
		/// <summary>
		/// Traitement des synchros maitres
		/// </summary>
		private void traiteSynchrosMaitres() {
			long tailleDossier 	= U.tailleDossier(_dossierSynchros);
			long tailleMax		= _conf.TailleMaxTampon * 1024 * 1024;
			
			foreach(SynchroMaitre s in _synchrosMaitre) {
				if (s.Valide) {
					s.GenListeFichiers();
					s.SupprimeReponsesSansDemande();
					
					foreach (Demande demande in s.GetDemandes()) {
						
						if (tailleDossier > tailleMax) break;
						
						if (!s.FichierDemandeExiste(demande)) {
							Logger.Log("Suppression de la demande " + demande.FichierDemande + " car le fichier correspondant n'existe plus");
							s.SupprimeDemande(demande);
							
						} else if (demande.FichierReponseExistant(s.DossierTampon) == null) {
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
		private void traiteSynchrosEsclaves() {
			DeltaMd5 		delta;
			List<Demande> 	demandes;
			int 			demandesFaites;
			string			fichierReponse;
			Demande			demandeSuiteFichier;
			
			foreach(SynchroEsclave s in _synchrosEsclave) {
				if (s.Valide) {
					s.GenListeFichiers();
					
					delta 			= s.DeltaFichiersAvecMaitre();
					demandes 		= s.GenDemandes(delta.Md5Ajoutes, 2 * C.MAX_DEMANDES_SIMULTANEES);
					demandesFaites 	= 0;
					
					List<Md5Fichier> md5sSupprimes = delta.Md5Supprimes;
					foreach (Md5Fichier md5Supprime in md5sSupprimes) {
						Logger.Log("Suppression du fichier " + md5Supprime.Chemin + " car le fichier correspondant n'existe plus");
						s.SupprimeFichier(md5Supprime);
					}
					
					s.SupprimeAncienneDemandes(demandes);
					
					foreach (Demande demande in demandes) {
						fichierReponse = demande.FichierReponseExistant(s.DossierTampon);
					
						if (fichierReponse != null) {
							Logger.Log("Récupération du fichier " + fichierReponse + " (" + demande.Md5f.Chemin + ")");
							demandeSuiteFichier = s.RecupereReponse(demande, fichierReponse);
							
							if (demandeSuiteFichier != null) {
								Logger.Log("Demande du fichier " + demandeSuiteFichier.Md5f.Chemin);
								s.FaireDemande(demandeSuiteFichier);
								demandesFaites++;
							}
							
						} else {
							if (demandesFaites < C.MAX_DEMANDES_SIMULTANEES) {
								if (!s.FichierDeDemandeExiste(demande)) {
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
