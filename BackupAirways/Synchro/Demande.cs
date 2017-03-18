﻿using System;
using System.IO;

namespace Saw
{
	public class Demande : Transaction
	{
		public readonly string	Demandeur = "";
		public 			string	FichierReponse { get { return string.Format(C.FORMAT__FICHIER_REPONSE, _md5f.Md5, _noPart); } }
		
				
		public Demande (Md5Fichier md5f, int noPart = 1)
		{
			_md5f 			= md5f;
			_noPart 		= noPart;
			_fichier		= string.Format(C.FORMAT__FICHIER_DEMANDE, md5f.Md5, _noPart, C.NOM_CLIENT);
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
