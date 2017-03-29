using System;
using System.IO;

namespace BackupAirways.Synchros
{
	public class Demande
	{
		private		Md5Fichier	_md5f;
		private 	long 		_partStart = -1;
		private 	string 		_fichierDemande;
		
		public Md5Fichier 	Md5f			{ get { return _md5f; } }
		public long			PartStart		{ get { return _partStart; } set { _partStart = value; } }
		public string		FichierDemande 	{ get { return _fichierDemande; } }
		
		public readonly string	Demandeur = "";
		//public 			string	FichierReponse { get { return string.Format(C.FORMAT__FICHIER_REPONSE, _md5f.Md5, _noPart); } }
		
				
		public Demande (Md5Fichier md5f, string nomClient, long partStart = 0) {
			_md5f 			= md5f;
			_partStart 		= partStart;
			_fichierDemande	= string.Format(C.FORMAT__FICHIER_DEMANDE, md5f.Md5, _partStart, nomClient);
		}
		
		public Demande(string fichierDemande) {
			var nomFichier 	= Path.GetFileName(fichierDemande);
			var infos 		= nomFichier.Split('.');
			
			_fichierDemande 	= nomFichier;
			_md5f 				= Md5Fichier.FromString(File.ReadAllText(fichierDemande));
			_partStart			= long.Parse(infos[1]);
			Demandeur			= infos[2];
		}
		
		public string FichierReponse(long partEnd) {
			return string.Format(C.FORMAT__FICHIER_REPONSE, _md5f.Md5, _partStart, partEnd);
		}
		
		public string FichierReponseExistant(string dossier) {
			var fichiers = Directory.GetFiles(dossier, string.Format(C.FORMAT__FICHIER_REPONSE, _md5f.Md5, _partStart, "*"), SearchOption.TopDirectoryOnly);
			
			return fichiers.Length > 0 ? fichiers[0] : null;			
		}
	}
}
