// TODO: -== FAIT ==- Gérer une liste des clients à part pour retrouver les clients liés à aucune synchro
// TODO: -== FAIT ==- Ne mettre à jour le md5 que s'il y eu des modifications
// TODO: -== FAIT ==- Ne générer les demandes que si elle n'existe pas déjà
// TODO: Gérer le déplacement d'un dossier de synchro
// TODO: -== FAIT ==- Ajouter la possibilité de créer un dossier depuis l'interface web
// TODO: -== GÉRÉ AUTREMENT ==-Logger à passer en argument au webserver
// TODO: Avertissement si le dossier choisi pour une synchro n'est pas vide
// TODO: -== FAIT ==- Revoir la classe mime pour ne pas instancier à chaque requête
// TODO: Changer sauvegarde en synchro
// TODO: Gérer les gros fichiers
// TODO: Revoir la fonction SynchroMaitre.SupprimeReponsesSansDemande (modification du format des fichiers de réponse)

using System;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.IO;

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
			/*long 	debut 	= 0;
			int 	noPart 	= 0;
			do {
				debut = U.ExtractFilePart(@"D:\Temp\test.jpg", @"D:\Temp\test.jpg.part" + noPart, debut, 3);
				noPart += 1;
			} while (debut != 0);
			
			string[] parts = { @"D:\Temp\test.jpg.part0", @"D:\Temp\test.jpg.part1", @"D:\Temp\test.jpg.part2", @"D:\Temp\test.jpg.part3", @"D:\Temp\test.jpg.part4", @"D:\Temp\test.jpg.part5" };
			
			U.AssembleFileParts(parts, @"D:\Temp\test.final.jpg");*/
		
			//Logger.Log(U.ExtractFilePart(@"D:\Temp\test.jpg", @"D:\Temp\test.jpg.part1", 0, 20).ToString());
			
			Conf 	conf 					= Conf.getConf(C.FICHIER_CONF);
			var 	gestionnaireSynchros 	= new GestionnaireSynchros(conf);
			var 	webGui 					= new WebGui(gestionnaireSynchros);
			
			webGui.ThreadWebServer.Join();
		}

		public static void Main(string[] args)
		{
			bool isFirstInstance;
			
			using (var mtx = new Mutex(true, C.MUTEX, out isFirstInstance)) {
				if (isFirstInstance) {
					var program = new Program();
				} else {
					Logger.Log("Le process est déjà démarré");
					Process.Start(File.ReadAllText(C.FICHIER_PREFIXE_WEB));
				}
			}
		}
	}
}