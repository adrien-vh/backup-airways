using System;
using System.Threading;
using System.Linq;
using System.Diagnostics;

using BackupAirways.GestionSynchros;
using BackupAirways.Gui;

namespace BackupAirways
{
	class Program
	{
		
		
		/*public static void test()
		{
			DateTime retval = DateTime.MinValue;
			string retour = "";
			Stopwatch sw = new Stopwatch();
			
			sw.Start();
			
			foreach (FileData f in FastDirectoryEnumerator.EnumerateFiles(@"D:\_perso\musique", "*.*", SearchOption.AllDirectories))
		    {
				
		        if (f.LastWriteTime > retval)
		        {
		            retval = f.LastWriteTime;
		            retour = f.Path;
		        }
		    }
			
			sw.Stop();
			
			Logger.Log(sw.ElapsedMilliseconds.ToString());
			
			Logger.Log(retour);
		}*/
		
		public Program() {	
			Conf 	conf 					= Conf.getConf(C.FICHIER_CONF);			
			var 	gestionnaireSynchros 	= new GestionnaireSynchros(conf);
			var 	webGui 					= new WebGui(gestionnaireSynchros);
		}

		public static void Main(string[] args)
		{
			bool isFirstInstance;
			//Program.test();
			using (var mtx = new Mutex(true, "Saw", out isFirstInstance)) {
				if (isFirstInstance) {
					var program = new Program();
				} else {
					Process.Start(C.PREFIXE);
				}
			}
		}
	}
}