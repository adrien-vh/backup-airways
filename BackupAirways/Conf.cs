using System;
using System.IO;
using Newtonsoft.Json;

namespace BackupAirways
{
	public class Conf
	{		
		private string 	_dossierTampon		= "";
		private int 	_tailleMaxTampon	= 100;
		private string 	_nomClient			= "";
		private string  _fichierConf		= null;
		
		public 	string 	DossierTampon 	{ get { return _dossierTampon; } 	set { _dossierTampon = value; sauve(); } }
		public 	int 	TailleMaxTampon	{ get { return _tailleMaxTampon; } 	set { _tailleMaxTampon = value; sauve(); } }
		public 	string	NomClient		{ get { return _nomClient; } 		set { _nomClient = value; sauve(); } }
		
		public static Conf getConf(string fichierConf) {
			Conf retour;
			
			if (File.Exists(fichierConf)) {
				retour = JsonConvert.DeserializeObject<Conf>(File.ReadAllText(fichierConf));
				retour._fichierConf = fichierConf;
				return retour;
			} else {	
				retour = new Conf(fichierConf);
				File.WriteAllText(fichierConf, JsonConvert.SerializeObject(retour));
				return retour;
			}
		}
		
		private Conf(string fichierConf) {
			_fichierConf = fichierConf;
		}
		
		private Conf() {}
		
		private void sauve()
		{
			if (_fichierConf != null) {
				File.WriteAllText(_fichierConf, JsonConvert.SerializeObject(this));
			}
		}
	}
}
