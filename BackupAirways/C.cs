using System.IO;

namespace BackupAirways
{
	public enum TypeTransaction { Demande, Reponse };
	public enum TypeSynchro 	{ Esclave, Maitre, Inutilisee };
	
	static internal class C
	{		
		public const int	MAX_DEMANDES_SIMULTANEES	= 50,
							LOG_MESSAGE					= 1,
							INTERVALLE_SYNCHRO_MINUTES	= 5,
							TAILLE_MAX_FICHIER			= 20;
		
		public const string PREFIXE 					= "http://localhost:8000/",
							MUTEX						= "backupairways",
							FICHIER_MAITRE				= "maitre.md5",
							FICHIER_CONF_SYNCHRO		= ".conf",
							FICHIER_CONF				= ".conf",
							FICHIER_PREFIXE_WEB			= "current_url.txt",
							DOSSIER_TRAVAIL				= ".backupAirways",
							
							EXT__REPONSE				= "rep",
							EXT__DEMANDE				= "dem",
							
							FORMAT__FICHIER_DEMANDE		= "{0}.{1}.{2}." + C.EXT__DEMANDE,
							FORMAT__FICHIER_REPONSE		= "{0}.{1}.{2}." + C.EXT__REPONSE;

		public static bool	IS_LINUX					{ get { return Directory.Exists ("/etc/"); } }
	}
	
	static internal class CJS
	{
		public const string 	ACTION__LISTE_DOSSIERS 			= "listedossiers",
								ACTION__LISTE_SYNCHROS			= "listesynchros",
								ACTION__C_JS 					= "c.js",
								ACTION__CHEMINS_DRIVES			= "cheminsdrives",
								ACTION__SET_DOSSIER_TAMPON		= "setdossiertampon",
								ACTION__ETAT_INITIALISATION		= "etatinitialisation",
								ACTION__GET_NOM_MACHINE			= "getnommachine",
								ACTION__CHANGE_NOM_CLIENT		= "changenomclient",
								ACTION__GET_LISTE_CLIENTS		= "getlisteclients",
								ACTION__NOUVELLE_SYNCHRO		= "nouvellesynchro",
								ACTION__JOINDRE_SYNCHRO			= "joindresynchro",
								ACTION__SUPPRIME_SYNCHRO		= "supprimesynchro",
								ACTION__SUPPRIME_CLIENT_SYNCHRO	= "supprimeclientsynchro",
								ACTION__CREATION_DOSSIER		= "creationdossier",
			
								PARAM__NOM_SYNCHRO				= "nomsynchro",
								PARAM__DOSSIER 					= "dossier",
								PARAM__CLE						= "cle",
								PARAM__VALEUR					= "valeur",
								PARAM__DOSSIERS_SEUL			= "dossierseulement",
								PARAM__FICHIERS_SEUL			= "fichiersseulement",
								PARAM__DOSSIER_TAMPON			= "dossiertampon",
								PARAM__NOM_MACHINE				= "nommachine",
								PARAM__EST_INITIALISE			= "estinitialise",
								PARAM__SYNCHROS_MAITRES			= "synchrosmaitres",
								PARAM__SYNCHROS_ESCLAVES		= "synchrosesclaves",
								PARAM__SYNCHROS_INUTILISEES		= "synchrosinutilisees",
		
								REP__ERREUR						= "erreur",
								REP__MESSAGE					= "message",
								REP__CHEMIN_GDRIVE				= "chemingoogledrive",
								REP__VALEUR						= "valeur",
								
								VAL__OK							= "ok";
	}
}
