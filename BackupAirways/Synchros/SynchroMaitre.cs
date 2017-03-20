using System;
using System.IO;

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
		
		public Reponse[] ReponsesExistantes () {
			var fichiersReponses 	= Directory.GetFiles(_dossierTamponSynchro, "*." + C.EXT__REPONSE, SearchOption.TopDirectoryOnly);
			var retour				= new Reponse[fichiersReponses.Length];
			var compteur			= 0;
			
			foreach (string fichierReponse in fichiersReponses)
			{
				retour[compteur] = new Reponse(fichierReponse);
				compteur++;
			}
			
			return retour;
		}
		
		public bool FichierExiste (Transaction transaction)
		{
			var nomFichier = _dossier + "\\" + transaction.Md5f.Chemin;
			return nomFichier != null;
		}
		
		
		
		public long FourniReponse (Demande demande)
		{
			var fichierTampon 	= _dossierTamponSynchro + "\\" + demande.FichierReponse;
			var fichierMeta		= fichierTampon + "." + C.EXT__META;
			
			File.Copy(Dossier + "\\" + demande.Md5f.Chemin, fichierTampon);
			
			File.WriteAllText(fichierMeta, string.Empty);
			File.WriteAllText(fichierMeta, demande.Md5f.ToString());
			
			return new FileInfo(fichierTampon).Length;
		}
		
		
	}
}
