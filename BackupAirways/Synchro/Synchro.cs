using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
	
namespace Saw
{
	public class Synchro
	{
		protected string 						_dossierTamponSynchro;
		protected string 						_fichierConf;
		protected string 						_fichierListeMd5;
		protected bool 							_valide = true;
		protected string 						_nom;
		protected string 						_dossier = "";
		protected TypeSynchro					_type = TypeSynchro.Inutilisee;
		protected string						_messageErreur = "";
		protected string 						_dossierSource;
		protected string						_maitre;
		protected List<ConfSynchro> 			_clients =  new List<ConfSynchro>();
		protected ConfSynchro					_confLocale;
		protected string						_fichierConfLocale;
				
		public bool 						Valide 			{ get { return _valide; } }
		public string 						Nom 			{ get { return _nom; } }
		public string 						Dossier 		{ get { return _dossier; } }
		public TypeSynchro 					Type			{ get { return _type; } }	
		public string						MessageErreur	{ get { return _messageErreur; } }
		public string						DossierSource	{ get { return _dossierSource; } }
		public string						Maitre			{ get { return _maitre; } }
		public List<ConfSynchro>			Clients 		{ get { return _clients; } }
		public ConfSynchro					ConfLocale		{ get { return _confLocale; } }
		
		
		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="nom">Nom de la synchro (correspond au nom du sous dossier dans le dossier tampon)</param>
		protected Synchro(string nom)
		{
			ConfSynchro confClient, confMaitre;
			
			_nom 					= nom;
			_dossierTamponSynchro 	= C.DOSSIER_TAMPON + "\\" + Nom;
			_fichierConf			= _dossierTamponSynchro + "\\" + C.FICHIER_CONF_SYNCHRO;
			
			if (!Directory.Exists(C.DOSSIER_TAMPON))
			{
				_valide = false;
				_messageErreur = "Le dossier tampon n'existe pas.";
			}
			
			if (!_valide)
			{
				try
				{
					Directory.CreateDirectory(_dossierTamponSynchro);
				}
				catch(Exception e)
				{
					_valide 		= false;
					_messageErreur 	= "Impossible de créer le dossier tampon \"" + _dossierTamponSynchro + "\" (" + e.Message + ")";
					Logger.Error(_messageErreur);
				}
			}
						
			if (_valide)
			{
				if (!File.Exists(_fichierConf))
				{
					_valide 		= false;
					_messageErreur 	= "Le fichier de conf (" + _fichierConf + ") n'existe pas";
					Logger.Error(_messageErreur);
				}
			}
						
			if (_valide)
			{
				confMaitre 		= JsonConvert.DeserializeObject<ConfSynchro>(File.ReadAllText(_fichierConf));
				_maitre 		= confMaitre.Client;
				_dossierSource 	= confMaitre.Chemin;
				
				if (_maitre == C.NOM_CLIENT)
				{
					_dossier 			= _dossierSource;
					_confLocale 		= confMaitre;
					_fichierConfLocale 	= _fichierConf;
				}
				
				foreach(string fichierClient in Directory.GetFiles(_dossierTamponSynchro, "*.client"))
				{
					confClient = JsonConvert.DeserializeObject<ConfSynchro>(File.ReadAllText(fichierClient));
					_clients.Add(new ConfSynchro(confClient.Client, confClient.Chemin));
					if (confClient.Client == C.NOM_CLIENT)
					{
						_dossier 			= confClient.Chemin;
						_confLocale 		= confClient;
						_fichierConfLocale 	= fichierClient;
					}
					
				}
				//Logger.Log(_clients["bip"]);
				
			}
			
			if (!Directory.Exists(_dossier))
			{
				_valide = false;
				_messageErreur = "Le dossier à synchroniser n'existe pas.";
			}
		}
		
		public static void GetSynchros(ref List<SynchroMaitre> synchrosMaitres, ref List<SynchroEsclave> synchrosEsclaves, ref List<Synchro> synchrosNonUtilisees)
		{
			ConfSynchro	confSynchro;
			string[]	chemins;
			
			synchrosMaitres.Clear();
			synchrosEsclaves.Clear();
			synchrosNonUtilisees.Clear();
			
			foreach(string fichier in Directory.GetFiles(C.DOSSIER_TAMPON, C.FICHIER_CONF_SYNCHRO, SearchOption.AllDirectories))
			{
	
				chemins = fichier.Replace(C.DOSSIER_TAMPON + "\\", "").Split('\\');
				if (chemins.Length == 2)
				{
					confSynchro = JsonConvert.DeserializeObject<ConfSynchro>(File.ReadAllText(fichier));
															
					if (confSynchro.Client == C.NOM_CLIENT)
					{
						synchrosMaitres.Add(new SynchroMaitre(chemins[0]));
					}
					#if MODE_DEMO
					if (File.Exists(C.DOSSIER_TAMPON + "\\" + chemins[0] + "\\." + C.NOM_CLIENT + ".client"))
					#else
					else if (File.Exists(C.DOSSIER_TAMPON + "\\" + chemins[0] + "\\." + C.NOM_CLIENT + ".client"))
					#endif
					{
						synchrosEsclaves.Add(new SynchroEsclave(chemins[0]));
					}
					else
					{
						synchrosNonUtilisees.Add(new Synchro(chemins[0]));
					}
				}
			}
		}		

		public static void InitFolder(string nom, string maitre, string dossier)
		{
			var 	confSynchro 			= new ConfSynchro(maitre, dossier);
			string 	dossierTamponSynchro 	= C.DOSSIER_TAMPON + "\\" + nom;
			string 	fichierConf				= dossierTamponSynchro + "\\" + C.FICHIER_CONF_SYNCHRO;
			
			Directory.CreateDirectory(dossierTamponSynchro);
			File.WriteAllText(fichierConf, JsonConvert.SerializeObject(confSynchro));
			
		}
		
		public void Rejoindre(string dossier)
		{
			var confSynchro = new ConfSynchro(C.NOM_CLIENT, dossier);
			
			_fichierConfLocale 	= _dossierTamponSynchro + "\\." + C.NOM_CLIENT + ".client";
			File.WriteAllText(_fichierConfLocale, JsonConvert.SerializeObject(confSynchro));
			_clients.Add(confSynchro);
		}
		
		public string ReponseExiste (Demande demande) {
			return File.Exists(_dossierTamponSynchro + "\\" + demande.FichierReponse) ? demande.FichierReponse : null;
		}
		
		public bool FichierTransactionExiste (Transaction t)
		{
			return File.Exists(_dossierTamponSynchro + "\\" + t.Fichier);
		}
		
		public void GenListeFichiersDbEtFichier ()	
		{
			var 	md5s 				= new List<string>();
			string 	md5;
			string 	fichierTmp;
			string 	fichierListeMd5Tmp 	= _nom + ".md5";
			bool	doCopy				= false;
			int		compteur			= 0;
			
			var fichiers = Directory.EnumerateFiles(Dossier , "*.*", SearchOption.AllDirectories);
			
			File.WriteAllText(fichierListeMd5Tmp, string.Empty);
			
			foreach (string fichier in fichiers)
			{
				fichierTmp 	= fichier.Replace(Dossier + "\\", "");
				if (fichierTmp.Substring(0, Math.Min(fichierTmp.Length, 14)) != ".backupAirways")
				{
					md5 		= U.MD5Hash(fichierTmp);
					md5s.Add(md5 + "|" + fichierTmp);
					compteur ++;
				}
			}
			
			_confLocale.NbFichiers = compteur;
			ecrireConfLocale();
			
			md5s.Sort();
			
			File.WriteAllLines(fichierListeMd5Tmp, md5s);
			
			if (!File.Exists(_fichierListeMd5))
			{
				doCopy = true;
			}
			else if (U.MD5Hash(File.ReadAllText(fichierListeMd5Tmp)) != U.MD5Hash(File.ReadAllText(_fichierListeMd5)))
			{
				doCopy = true;
			}
		
			if (doCopy) 
			{
				File.Copy(fichierListeMd5Tmp, _fichierListeMd5, true);
			}
		}
		
			
		public void SupprimeTransaction (Transaction transaction) 
		{
			File.Delete(_dossierTamponSynchro + "\\" + transaction.Fichier);
		}
		
		private void ecrireConfLocale ()
		{
			File.WriteAllText(_fichierConfLocale, JsonConvert.SerializeObject(ConfLocale));
		}
		
	}
}
	