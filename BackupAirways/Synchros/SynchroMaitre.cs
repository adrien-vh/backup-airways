using System;
using System.IO;
using System.Collections.Generic;

namespace BackupAirways.Synchros
{
	public class SynchroMaitre : Synchro
	{	
		
		public SynchroMaitre(string nom, Conf conf) : base(nom, conf)
		{
			_type				= TypeSynchro.Maitre;
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
		
		public void SupprimeReponsesSansDemande () {
			string nomFichierSansExtension;
			
			foreach (string fichierReponse in Directory.GetFiles(_dossierTamponSynchro, "*." + C.EXT__REPONSE, SearchOption.TopDirectoryOnly))	{
				nomFichierSansExtension = Path.GetFileNameWithoutExtension(fichierReponse);
				if (Directory.GetFiles(_dossierTamponSynchro, nomFichierSansExtension + ".rep", SearchOption.TopDirectoryOnly).Length == 0) {
					File.Delete(fichierReponse);
				}
			}
		}

		
		public bool FichierExiste (Demande demande)
		{
			var nomFichier = _dossier + "\\" + demande.Md5f.Chemin;
			return nomFichier != null;
		}
		
		
		
		public long FourniReponse (Demande demande)
		{
			var fichierTampon 	= _dossierTamponSynchro + "\\" + demande.FichierReponse;
			
			File.Copy(Dossier + "\\" + demande.Md5f.Chemin, fichierTampon);
			
			return new FileInfo(fichierTampon).Length;
		}
		
		
	}
}
