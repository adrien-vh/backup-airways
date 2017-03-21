namespace BackupAirways.Synchros
{
	public class ConfSynchro
	{		
		public string 	Client 		= "";
		public string 	Chemin 		= "";
		public int		NbFichiers 	= 0;
		
		public ConfSynchro(string client, string chemin, int nbFichiers = 0)
		{
			Client 		= client;
			Chemin 		= chemin;
			NbFichiers  = nbFichiers;
		}
	}
}
