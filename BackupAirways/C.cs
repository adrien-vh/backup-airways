using System.IO;

namespace BackupAirways
{
	public enum TypeTransaction { Demande, Reponse };
	public enum TypeSynchro 	{ Esclave, Maitre, Inutilisee };
	
	static internal class C
	{		
		public const int	MAX_DEMANDES_SIMULTANEES	= 50,
							LOG_MESSAGE					= 1,
							INTERVALLE_SYNCHRO_MINUTES	= 5;
		
		public const string PREFIXE 					= "http://localhost:8000/",
							FICHIER_MAITRE				= "maitre.md5",
							FICHIER_CONF_SYNCHRO		= ".conf",
							FICHIER_CONF				= ".conf",
							
							EXT__REPONSE				= "rep",
							EXT__DEMANDE				= "dem",
							EXT__META					= "meta",
							
							FORMAT__FICHIER_DEMANDE		= "{0}.{1}.{2}." + C.EXT__DEMANDE,
							FORMAT__FICHIER_REPONSE		= "{0}.{1}." + C.EXT__REPONSE;

		public static bool		IS_LINUX			{ get { return Directory.Exists ("/etc/"); } }
	}
	
	static internal class CJS
	{
		public const string ACTION__LISTE_DOSSIERS 		= "listedossiers",
							ACTION__LISTE_SAUVEGARDES	= "listesauvegardes",
							ACTION__C_JS 				= "c.js",
							ACTION__CHEMINS_DRIVES		= "cheminsdrives",
							ACTION__SET_DOSSIER_TAMPON	= "setdossiertampon",
							ACTION__ETAT_INITIALISATION	= "etatinitialisation",
							ACTION__GET_NOM_MACHINE		= "getnommachine",
							ACTION__CHANGE_NOM_CLIENT	= "changenomclient",
							ACTION__GET_LISTE_CLIENTS	= "getlisteclients",
							ACTION__NOUVELLE_SAUVEGARDE	= "nouvellesauvegarde",
							ACTION__JOINDRE_SAUVEGARDE	= "joindresauvegarde",
		
							PARAM__NOM_SAUVEGARDE		= "nomsauvegarde",
							PARAM__DOSSIER 				= "dossier",
							PARAM__CLE					= "cle",
							PARAM__VALEUR				= "valeur",
							PARAM__DOSSIERS_SEUL		= "dossierseulement",
							PARAM__FICHIERS_SEUL		= "fichiersseulement",
							PARAM__DOSSIER_TAMPON		= "dossiertampon",
							PARAM__NOM_MACHINE			= "nommachine",
							PARAM__EST_INITIALISE		= "estinitialise",
	
							REP__ERREUR					= "erreur",
							REP__MESSAGE				= "message",
							REP__CHEMIN_GDRIVE			= "chemingoogledrive",
							REP__VALEUR					= "valeur";
	}
}
