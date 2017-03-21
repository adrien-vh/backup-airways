using System;
using System.IO;

namespace BackupAirways.Synchros
{
	public class Demande
	{
		private		Md5Fichier	_md5f;
		private 	int 		_noPart = -1;
		private 	string 		_fichier;
		
		public Md5Fichier 	Md5f		{ get { return _md5f; } }
		public int			NoPart 		{ get { return _noPart; } }
		public string		Fichier 	{ get { return _fichier; } }
		
		public readonly string	Demandeur = "";
		public 			string	FichierReponse { get { return string.Format(C.FORMAT__FICHIER_REPONSE, _md5f.Md5, _noPart); } }
		
				
		public Demande (Md5Fichier md5f, string nomClient, int noPart = 1)
		{
			_md5f 			= md5f;
			_noPart 		= noPart;
			_fichier		= string.Format(C.FORMAT__FICHIER_DEMANDE, md5f.Md5, _noPart, nomClient);
		}
		
		public Demande(string fichierDemande)
		{
			var nomFichier 	= Path.GetFileName(fichierDemande);
			var infos 		= nomFichier.Split('.');
			
			_fichier 		= nomFichier;
			_md5f 			= Md5Fichier.FromString(File.ReadAllText(fichierDemande));
			_noPart			= int.Parse(infos[1]);
			Demandeur		= infos[2];
			
		}
	}
}
