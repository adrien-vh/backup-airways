using System;
using System.IO;
using Newtonsoft.Json;

namespace Saw
{
	public sealed class Conf
	{
		private static Conf instance = Conf.getConf();
		public static Conf Instance { get { return instance; } }
		
		private string 	_dossierTampon		= "";
		private int 	_tailleMaxTampon	= 100;
		private string 	_nomClient			= "";
		
		public 	string 	DossierTampon 	{ get { return _dossierTampon; } 	set { _dossierTampon = value; sauve(); } }
		public 	int 	TailleMaxTampon	{ get { return _tailleMaxTampon; } 	set { _tailleMaxTampon = value; sauve(); } }
		public 	string	NomClient		{ get { return _nomClient; } 		set { _nomClient = value; sauve(); } }
		
		private static Conf getConf() {
			Conf retour;
			
			if (File.Exists(C.FICHIER_CONF))
			{
				return JsonConvert.DeserializeObject<Conf>(File.ReadAllText(C.FICHIER_CONF));
			}
			else
			{	
				retour = new Conf();
				File.WriteAllText(C.FICHIER_CONF, JsonConvert.SerializeObject(retour));
				return retour;
			}
		}
		
		private Conf() { }
		
		private void sauve()
		{
			File.WriteAllText(C.FICHIER_CONF, JsonConvert.SerializeObject(this));
		}
	}
}
