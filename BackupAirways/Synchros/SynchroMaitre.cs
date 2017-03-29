using System;
using System.IO;
using System.Collections.Generic;

namespace BackupAirways.Synchros
{
	public class SynchroMaitre : Synchro
	{	
		
		public SynchroMaitre(string nom, Conf conf) : base(nom, conf, TypeSynchro.Maitre) {
			_fichierListeMd5 	= _dossierTamponSynchro + "\\" + C.FICHIER_MAITRE;
		}
				
		public Demande[] GetDemandes () {
			var fichiersDemandes 	= Directory.GetFiles(_dossierTamponSynchro, "*." + C.EXT__DEMANDE, SearchOption.TopDirectoryOnly);
			var retour				= new Demande[fichiersDemandes.Length];
			var compteur			= 0;
			
			foreach (string fichierDemande in fichiersDemandes)
			{
				retour[compteur] = new Demande(fichierDemande);
				compteur++;
			}
			
			return retour;
		}
		
		
		
		
		/// <summary>
		/// Supprime les fichiers réponses ne correspondant plus à aucune demande
		/// </summary>
		public void SupprimeReponsesSansDemande () {
			string nomFichierSansExtension;
			
			foreach (string fichierReponse in Directory.GetFiles(_dossierTamponSynchro, "*." + C.EXT__REPONSE, SearchOption.TopDirectoryOnly))	{
				
				nomFichierSansExtension = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fichierReponse));
				
				if (Directory.GetFiles(_dossierTamponSynchro, nomFichierSansExtension + ".*." + C.EXT__DEMANDE, SearchOption.TopDirectoryOnly).Length == 0) {
					File.Delete(fichierReponse);
				}
			}
		}

		
		public bool FichierDemandeExiste (Demande demande) {
			var nomFichier = _dossier + "\\" + demande.Md5f.Chemin;
			return File.Exists(nomFichier);
		}
		
		
		
		public long FourniReponse (Demande demande) {
			string fichierTampon;
			var fichierTemp		= demande.Md5f.Md5 + ".temp";
			var fichierDemandé	= Dossier + "\\" + demande.Md5f.Chemin;
			var infosFichier 	= new FileInfo(fichierDemandé);
        	var taille 			= infosFichier.Length;
        	
        	if (taille > C.TAILLE_MAX_FICHIER * 1024 * 1024) {
        		
        		fichierTampon 	= _dossierTamponSynchro + "\\" + demande.FichierReponse(U.ExtractFilePart(fichierDemandé, fichierTemp, demande.PartStart, C.TAILLE_MAX_FICHIER)) ;
        		
        		if (!File.Exists(fichierTampon)) {
        			File.Copy(fichierTemp, fichierTampon);
        		}
        	} else {
        		fichierTampon 	= _dossierTamponSynchro + "\\" + demande.FichierReponse(0);
        		if (!File.Exists(fichierTampon)) {
        			File.Copy(fichierDemandé, fichierTampon);
        		}
        	}
			
			return new FileInfo(fichierTampon).Length;
		}
		
		
	}
}
