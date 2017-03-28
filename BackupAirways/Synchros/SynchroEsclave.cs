using System;
using System.IO;
using System.Collections.Generic;

namespace BackupAirways.Synchros
{
	public class SynchroEsclave : Synchro
	{	
		
		public SynchroEsclave(string nom, Conf conf) : base(nom, conf, TypeSynchro.Esclave)
		{
			_fichierListeMd5 	= _dossierTamponSynchro + "\\" + conf.NomClient + ".md5";
		}
		
		public DeltaMd5 DeltaFichiersAvecMaitre () {
			string fichierMaitre	= _dossierTamponSynchro + "\\" + C.FICHIER_MAITRE;
			string fichierEsclave	= _dossierTamponSynchro + "\\" + _conf.NomClient + ".md5";
			string[] md5Maitre;
			string[] md5Esclave;
			
			var pMaitre 		= 0;
			var pEsclave 		= 0;
			var ajouts 			= new List<Md5Fichier>();
			var suppressions 	= new List<Md5Fichier>();
			
			if (File.Exists(fichierEsclave) && File.Exists(fichierMaitre))
			{
			
				md5Maitre 		= File.ReadAllLines(fichierMaitre);
				md5Esclave 		= File.ReadAllLines(fichierEsclave);
				
				while (true)
				{
					if (pMaitre >= md5Maitre.Length && pEsclave >= md5Esclave.Length) break;
					
					if (pMaitre >= md5Maitre.Length)
					{
						suppressions.Add(Md5Fichier.FromString(md5Esclave[pEsclave]));
						pEsclave++;
					}
					else if (pEsclave >= md5Esclave.Length)
					{
						ajouts.Add(Md5Fichier.FromString(md5Maitre[pMaitre]));
						pMaitre++;
					}
					else if (string.Compare(md5Maitre[pMaitre], md5Esclave[pEsclave]) < 0)
					{
						ajouts.Add(Md5Fichier.FromString(md5Maitre[pMaitre]));
						pMaitre++;
					}
					else if (string.Compare(md5Maitre[pMaitre], md5Esclave[pEsclave]) > 0)
					{
						suppressions.Add(Md5Fichier.FromString(md5Esclave[pEsclave]));
						pEsclave++;
					}
					else if (string.Compare(md5Maitre[pMaitre], md5Esclave[pEsclave]) == 0)
					{
						pMaitre++;
						pEsclave++;
					}
				}
				
				_nbFichiersMaitre = md5Maitre.Length;
			}
			
			/*foreach(string ajout in ajouts)
			{
				Logger.Information("Ajout :" + ajout);
			}*/
			/*
			foreach(string suppression in suppressions)
			{
				Logger.Information("Suppression :" + suppression);
			}*/
			
			return new DeltaMd5(ajouts, suppressions);
		}
		
		/// <summary>
		/// Récupère la réponse à une demande
		/// </summary>
		/// <param name="demande">Demande faite par le client</param>
		/// <param name="fichierReponse">Fichier de réponse du maitre</param>
		/// <returns>True si toutes les parties du fichiers sont rassemblées </returns>
		public Demande RecupereReponse (Demande demande, string fichierReponse) 
		{
			string[]	partsExistantes;
			string 		fichierDestination;
			Demande		retour = null;
			
			var				infos 		= Path.GetFileName(fichierReponse).Split('.');
			List<long> 		curseurs;
			List<string> 	parts;
			
			if (infos[2] == "0") {
				partsExistantes = Directory.GetFiles(_dossier + "\\" + C.DOSSIER_TRAVAIL, demande.Md5f.Md5 + ".*", SearchOption.TopDirectoryOnly);
				if (partsExistantes.Length > 0) {
					curseurs 	= new List<long>();
					parts		= new List<string>();
					
					foreach (string part in partsExistantes) {
						curseurs.Add(long.Parse(Path.GetFileName(part).Split('.')[1]));
					}
					curseurs.Sort();
					curseurs.Add(0);
					
					fichierDestination	= Dossier + "\\" + C.DOSSIER_TRAVAIL + "\\" + demande.Md5f.Md5 + "." + infos[2];
					Directory.CreateDirectory(Path.GetDirectoryName(fichierDestination));
					File.Copy(fichierReponse, fichierDestination, true);
					
					foreach(long curseur in curseurs) {
						parts.Add(_dossier + "\\" + C.DOSSIER_TRAVAIL + "\\" + demande.Md5f.Md5 + "." + curseur);
					}
					
					fichierDestination 	= Dossier + "\\" + demande.Md5f.Chemin;
					
					U.AssembleFileParts(parts.ToArray(), fichierDestination);
					
					foreach (string part in parts) {
						File.Delete(part);
					}
				
				} else {
					fichierDestination 	= Dossier + "\\" + demande.Md5f.Chemin;
					Directory.CreateDirectory(Path.GetDirectoryName(fichierDestination));
					File.Copy(fichierReponse, fichierDestination, true);
				}
				
			} else {
				fichierDestination	= Dossier + "\\" + C.DOSSIER_TRAVAIL + "\\" + demande.Md5f.Md5 + "." + infos[2];
				Directory.CreateDirectory(Path.GetDirectoryName(fichierDestination));
				File.Copy(fichierReponse, fichierDestination, true);
				
				retour = new Demande(demande.Md5f, _confLocale.Client, long.Parse(infos[2]));
			}
			
			SupprimeDemande(demande);
			
			if (Directory.GetFiles(_dossierTamponSynchro, demande.Md5f.Md5 + "." + demande.NoPart + ".*." + C.EXT__DEMANDE).Length == 0) {
				File.Delete(fichierReponse);
			}	
			
			return retour;
		}
		
		public void FaireDemande (Demande demande) 
		{
			var fichierDemande = _dossierTamponSynchro + "\\" + demande.Fichier;
			
			if (!File.Exists(fichierDemande)) {
				File.WriteAllText(fichierDemande, demande.Md5f.ToString());
			}
			
		}
		
		public List<Demande> GenDemandes (List<Md5Fichier> md5fs, int nbMax)
		{
			var			retour		= new List<Demande>();
			string[] 	partsExistantes;
			long		posCurseur	= 0;
			
			foreach (Md5Fichier md5f in md5fs) {
				if (retour.Count > nbMax) {
					break;
				}
				
				try {
					partsExistantes = Directory.GetFiles(_dossier + "\\" + C.DOSSIER_TRAVAIL, md5f.Md5 + ".*", SearchOption.TopDirectoryOnly);
				} catch (Exception e) {
					Logger.Log(e.Message, global::Logger.LogLevel.ERROR);
					partsExistantes = new string[0];
				}
				
				if (partsExistantes.Length > 0) {
					foreach (string part in partsExistantes) {
						posCurseur = Math.Max(posCurseur, long.Parse(Path.GetFileName(part).Split('.')[1]));
					}
				}
				
				retour.Add(new Demande(md5f, _conf.NomClient, posCurseur));
			}
						
			return retour;
		}
		
		public void SupprimeFichier (Md5Fichier md5f)
		{
			if (File.Exists(_dossier + "\\" + md5f.Chemin))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(_dossier + "\\" + C.DOSSIER_TRAVAIL + "\\" + md5f.Chemin));
				File.Move(_dossier + "\\" + md5f.Chemin, _dossier + "\\" + C.DOSSIER_TRAVAIL + "\\" + md5f.Chemin);
			}
			
			foreach (string fichier in Directory.GetFiles(_dossierTamponSynchro, md5f.Md5 + "*." + _conf.NomClient + "*.dem"))
			{
				File.Delete(fichier);
			}
		}
	}
}
