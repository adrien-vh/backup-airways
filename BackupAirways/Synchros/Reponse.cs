using System;
using System.IO;

namespace BackupAirways.Synchros
{
	public class Reponse : Transaction
	{		
		public Reponse(string fichierReponse)
		{
			var nomFichier 	= Path.GetFileName(fichierReponse);
			var infos 		= nomFichier.Split('.');
			
			_fichier 	= nomFichier;
			_md5f 		= Md5Fichier.FromString(File.ReadAllText(fichierReponse + "." + C.EXT__META));
			_noPart		= int.Parse(infos[1]);
		}
	}
}
