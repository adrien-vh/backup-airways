﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Saw
{
	public class SynchroEsclave : Synchro
	{	
		
		public SynchroEsclave(string nom) : base(nom)
		{
			_type 				= TypeSynchro.Esclave;
			_fichierListeMd5 	= _dossierTamponSynchro + "\\" + C.NOM_CLIENT + ".md5";
		}
		
		public DeltaMd5 DeltaFichiersAvecMaitre () {
			string fichierMaitre	= _dossierTamponSynchro + "\\" + C.FICHIER_MAITRE;
			string fichierEsclave	= _dossierTamponSynchro + "\\" + C.NOM_CLIENT + ".md5";
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
			string fichierMeta			= _dossierTamponSynchro + "\\" + demande.FichierReponse + "." + C.EXT__META;
			string fichierDestination;
						
			if (File.Exists(fichierSource) && File.Exists(fichierMeta))
			{
				fichierDestination 	= Dossier + "\\" + demande.Md5f.Chemin;
				Directory.CreateDirectory(Path.GetDirectoryName(fichierDestination));
				File.Copy(fichierSource, fichierDestination, true);
			
				SupprimeTransaction(demande);
				
				if (Directory.GetFiles(_dossierTamponSynchro, demande.Md5f.Md5 + "." + demande.NoPart + ".*." + C.EXT__DEMANDE).Length == 0)
				{
					File.Delete(fichierSource);
					File.Delete(fichierMeta);
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
				retour.Add(new Demande(md5f));
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
			
			foreach (string fichier in Directory.GetFiles(_dossierTamponSynchro, md5f.Md5 + "*." + C.NOM_CLIENT + "*.dem"))
			{
				File.Delete(fichier);
			}
		}
	}
}
