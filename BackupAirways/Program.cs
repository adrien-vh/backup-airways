using System;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Saw
{
	class Program
	{
		
		
		public static void test()
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
		}
		/*
		public Program()
		{	
			//test();
			initWebserver();
			
			_synchrosMaitre 	= Synchro.MaitreFromDB();
			_synchrosEsclave 	= Synchro.EsclaveFromDB();
									
			while (false)
			{
				traiteSynchrosMaitres();
				traiteSynchrosEsclaves();
					
				Thread.Sleep(60000);
			}
			
			_threadServer.Join();
		}	*/	
		


		
		public static void Main(string[] args)
		{
			bool isFirstInstance;
			//Program.test();
			using (var mtx = new Mutex(true, "Saw", out isFirstInstance)) {
				if (isFirstInstance) {
					var baw = Baw.Instance;
				} else {
					Process.Start(C.PREFIXE);
				}
			}
		}
	}
}