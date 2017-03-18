namespace Saw
{
	public class ConfSynchro
	{		
		public string 	Client 		= "";
		public string 	Chemin 		= "";
		public int		NbFichiers 	= 0;
		
		public ConfSynchro(string client, string chemin) 
		{
			Client = client;
			Chemin = chemin;
		}
	}
}
