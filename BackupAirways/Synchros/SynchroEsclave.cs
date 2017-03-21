using System;
using System.IO;
using System.Collections.Generic;

namespace BackupAirways.Synchros
{
	public class SynchroEsclave : Synchro
	{	
		
		public SynchroEsclave(string nom, Conf conf) : base(nom, conf)
		{
			_type 				= TypeSynchro.Esclave;
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
				
		public void RecupereReponse (Demande demande) 
		{
			
			string fichierSource		= _dossierTamponSynchro + "\\" + demande.FichierReponse;			
			string fichierDestination;
						
			if (File.Exists(fichierSource))
			{
				fichierDestination 	= Dossier + "\\" + demande.Md5f.Chemin;
				Directory.CreateDirectory(Path.GetDirectoryName(fichierDestination));
				File.Copy(fichierSource, fichierDestination, true);
			
				SupprimeDemande(demande);
				
				if (Directory.GetFiles(_dossierTamponSynchro, demande.Md5f.Md5 + "." + demande.NoPart + ".*." + C.EXT__DEMANDE).Length == 0)
				{
					File.Delete(fichierSource);
				}
			}		
		}
		
		public void FaireDemande (Demande demande) 
		{
			var fichierDemande = _dossierTamponSynchro + "\\" + demande.Fichier;
			if (!File.Exists(fichierDemande))
			{
				File.WriteAllText(fichierDemande, demande.Md5f.ToString());
			}
			
		}
		
		public List<Demande> GenDemandes (List<Md5Fichier> md5fs, int nbMax)
		{
			var	retour		= new List<Demande>();
			
			foreach (Md5Fichier md5f in md5fs)
			{
				if (retour.Count > nbMax) 
				{
					break;
				}
				retour.Add(new Demande(md5f, _conf.NomClient));
			}
						
			return retour;
		}
		
		public void SupprimeFichier (Md5Fichier md5f)
		{
			if (File.Exists(_dossier + "\\" + md5f.Chemin))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(_dossier + "\\.backupAirways\\" + md5f.Chemin));
				File.Move(_dossier + "\\" + md5f.Chemin, _dossier + "\\.backupAirways\\" + md5f.Chemin);
			}
			
			foreach (string fichier in Directory.GetFiles(_dossierTamponSynchro, md5f.Md5 + "*." + _conf.NomClient + "*.dem"))
			{
				File.Delete(fichier);
			}
		}
	}
}
