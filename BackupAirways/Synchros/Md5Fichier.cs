namespace BackupAirways.Synchros
{
	public class Md5Fichier
	{
		private readonly string _md5;
		private readonly string _chemin;
		
		public string Md5 		{ get { return _md5; } }
		public string Chemin 	{ get { return _chemin; } }
		
		public static Md5Fichier FromString(string chaine)
		{
			var infos = chaine.Split('|');
			return new Md5Fichier(infos[0], infos[1]);
		}
		
		public Md5Fichier(string md5, string chemin)
		{
			_md5 = md5;
			_chemin = chemin;
		}
		
		public override string ToString()
		{
			return _md5 + "|" + _chemin;
		}
	}
}
