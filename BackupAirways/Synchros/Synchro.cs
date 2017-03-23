using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
	
namespace BackupAirways.Synchros
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
		protected Conf							_conf;
		protected int							_nbFichiersMaitre = 0;
				
		public bool 						Valide 				{ get { return _valide; } }
		public string 						Nom 				{ get { return _nom; } }
		public string 						Dossier 			{ get { return _dossier; } }
		public TypeSynchro 					Type				{ get { return _type; } }	
		public string						MessageErreur		{ get { return _messageErreur; } }
		public string						DossierSource		{ get { return _dossierSource; } }
		public string						Maitre				{ get { return _maitre; } }
		public List<ConfSynchro>			Clients 			{ get { return _clients; } }
		public ConfSynchro					ConfLocale			{ get { return _confLocale; } }
		public int							NbFichiersMaitre	{ get { return _nbFichiersMaitre; } }
		
		
		/// <summary>
		/// Constructeur
		/// </summary>
		/// <param name="nom">Nom de la synchro (correspond au nom du sous dossier dans le dossier tampon)</param>
	 	/// <param name="conf">Configuration</param>
		public Synchro(string nom, Conf conf)
		{
			ConfSynchro confClient, confMaitre;
			
			_conf					= conf;
			_nom 					= nom;
			_dossierTamponSynchro 	= conf.DossierTampon + "\\" + Nom;
			_fichierConf			= _dossierTamponSynchro + "\\" + C.FICHIER_CONF_SYNCHRO;
			
			if (!Directory.Exists(conf.DossierTampon))
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
					Logger.Log(_messageErreur);
				}
			}
						
			if (_valide)
			{
				if (!File.Exists(_fichierConf))
				{
					_valide 		= false;
					_messageErreur 	= "Le fichier de conf (" + _fichierConf + ") n'existe pas";
					Logger.Log(_messageErreur);
				}
			}
						
			if (_valide)
			{
				confMaitre 		= JsonConvert.DeserializeObject<ConfSynchro>(File.ReadAllText(_fichierConf));
				_maitre 		= confMaitre.Client;
				_dossierSource 	= confMaitre.Chemin;
				
				if (File.Exists(_dossierTamponSynchro + "\\maitre.md5")) {
					_nbFichiersMaitre = File.ReadAllLines(_dossierTamponSynchro + "\\maitre.md5").Length;
				}
				
				if (_maitre == _conf.NomClient)
				{
					_dossier 			= _dossierSource;
					_confLocale 		= confMaitre;
					_fichierConfLocale 	= _fichierConf;
				}
				
				foreach(string fichierClient in Directory.GetFiles(_dossierTamponSynchro, "*.client"))
				{
					confClient = JsonConvert.DeserializeObject<ConfSynchro>(File.ReadAllText(fichierClient));
					
					if (File.Exists(_dossierTamponSynchro + "\\" + confClient.Client + ".md5")) {
						confClient.NbFichiers = File.ReadAllLines(_dossierTamponSynchro + "\\" + confClient.Client + ".md5").Length;
					}
					_clients.Add(new ConfSynchro(confClient.Client, confClient.Chemin, confClient.NbFichiers));
					if (confClient.Client == _conf.NomClient)
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

		public void Rejoindre(string dossier)
		{
			var confSynchro = new ConfSynchro(_conf.NomClient, dossier);
			
			_fichierConfLocale 	= _dossierTamponSynchro + "\\." + _conf.NomClient + ".client";
			File.WriteAllText(_fichierConfLocale, JsonConvert.SerializeObject(confSynchro));
			_clients.Add(confSynchro);
		}
		
		public string ReponseExiste (Demande demande) {
			return File.Exists(_dossierTamponSynchro + "\\" + demande.FichierReponse) ? demande.FichierReponse : null;
		}
		
		public bool FichierDeDemandeExiste (Demande demande) {
			return File.Exists(_dossierTamponSynchro + "\\" + demande.Fichier);
		}
		
		public void GenListeFichiers ()	
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
				if (fichierTmp.Substring(0, Math.Min(fichierTmp.Length, 14)) != ".backupAirways") {
					md5 		= U.MD5Hash(fichierTmp);
					md5s.Add(md5 + "|" + fichierTmp);
					compteur ++;
				}
			}
			
			_confLocale.NbFichiers = compteur;
			ecrireConfLocale();
			
			md5s.Sort();
			
			File.WriteAllLines(fichierListeMd5Tmp, md5s);
			
			if (_type == TypeSynchro.Maitre) {
				_nbFichiersMaitre = md5s.Count;
			}
			
			if (!File.Exists(_fichierListeMd5)) {
				doCopy = true;
			} else if (U.MD5Hash(File.ReadAllText(fichierListeMd5Tmp)) != U.MD5Hash(File.ReadAllText(_fichierListeMd5)))
			{
				doCopy = true;
			}
		
			if (doCopy) {
				File.Copy(fichierListeMd5Tmp, _fichierListeMd5, true);
			}
		}
		
		public void SupprimeDemande (Demande demande) {
			File.Delete(_dossierTamponSynchro + "\\" + demande.Fichier);
		}
		
		private void ecrireConfLocale () {
			File.WriteAllText(_fichierConfLocale, JsonConvert.SerializeObject(ConfLocale));
		}
		
	}
}
	