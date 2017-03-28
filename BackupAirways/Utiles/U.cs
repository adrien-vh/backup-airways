using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace BackupAirways
{
	static internal class U
	{
		/// <summary>
		/// Extrait une partie d'un fichier
		/// </summary>
		/// <param name="fichierIn">fichier dont il faut extraire les données</param>
		/// <param name="fichierOut">fichier à écrire</param>
		/// <param name="debut">où commencer à lire dans le fichier</param>
		/// <param name="longueur">taille à lire en MO</param>
		/// <returns>position du curseur à la fin de la lecture</returns>
		public static long ExtractFilePart(string fichierIn, string fichierOut, long debut, int longueur) {
			const 	int 	BUFFER_SIZE = 20 * 1024;
					byte[] 	buffer 		= new byte[BUFFER_SIZE];
		    		long	retour		= 0;
		    		
		    using (Stream input = File.OpenRead(fichierIn)) {
		    	input.Seek(debut, SeekOrigin.Begin);
		    	using (Stream output = File.Create(fichierOut)) {
		    		int restantALire = longueur * 1024 * 1024;
		    		int nbOctetsLus  = 0;
		    		while (restantALire > 0 && (nbOctetsLus = input.Read(buffer, 0, Math.Min(restantALire, BUFFER_SIZE))) > 0) {
		    			output.Write(buffer, 0, nbOctetsLus);
		    			restantALire -= nbOctetsLus;
		    		}
		    		if (nbOctetsLus == 0) {
		    			retour = 0;
		    		} else {
		    			retour = input.Position;
		    		}
		    	}
		    }
		   
		    return retour;
		}
		
		public static void AssembleFileParts(string[] fichiersIn, string fichierOut) {
			using (Stream output = File.Create(fichierOut)) {
				foreach (string fichierIn in fichiersIn) {
					using (Stream input = File.OpenRead(fichierIn)) {
						input.CopyTo(output);
					}
				}
			}
		}
		
		/*public static void SplitFile(string inputFile, int chunkSize, string path)
		{
		    const int BUFFER_SIZE = 20 * 1024;
		    byte[] buffer = new byte[BUFFER_SIZE];
		
		    using (Stream input = File.OpenRead(inputFile))
		    {
		        int index = 0;
		        while (input.Position < input.Length)
		        {
		            using (Stream output = File.Create(path + "\\" + index))
		            {
		                int remaining = chunkSize, bytesRead;
		                while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
		                        Math.Min(remaining, BUFFER_SIZE))) > 0)
		                {
		                    output.Write(buffer, 0, bytesRead);
		                    remaining -= bytesRead;
		                }
		            }
		            index++;
		            Thread.Sleep(500); // experimental; perhaps try it
		        }
		    }
		}*/
		
		/// <summary>
		/// Détermine si le path passé en paramètre correspond à un fichier/dossier système
		/// </summary>
		/// <param name="path">Chemin à tester</param>
		/// <returns>True s'il s'agit d'un fichier/dossier système, False sinon</returns>
		public static bool IsSystem(string path)
		{
		    FileAttributes attributes = File.GetAttributes(path);
		    return (attributes & FileAttributes.System) != 0;
		}
		
		/// <summary>
		/// Lit une sous-clé du registre HKCU
		/// </summary>
		/// <param name="path">Chemin de la clé</param>
		/// <param name="sousCle">Nom de la sous clé</param>
		/// <returns>Valeur de la sous clé</returns>
		public static string ReadRegistryCUKey(string path, string sousCle)
		{
			try
			{
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
				{
					if (key != null)
					{
						Object o = key.GetValue(sousCle);
						if (o != null)
						{
							return o.ToString();
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Log(e.Message);
			}
			return null;
		}
		
		/// <summary>
		/// Calcul le hash MD5 d'une chaine de caractère
		/// </summary>
		/// <param name="input">Chaîne à hasher</param>
		/// <returns>Hash MD5</returns>
		public static string MD5Hash(string input)
		{
		    MD5		md5 		= System.Security.Cryptography.MD5.Create();
		    byte[] 	inputBytes 	= System.Text.Encoding.ASCII.GetBytes(input);
		    byte[] 	hash 		= md5.ComputeHash(inputBytes);
			var 	sb 			= new StringBuilder();
		
		    for (int i = 0; i < hash.Length; i++)
		    {
		        sb.Append(hash[i].ToString("X2"));
		    }
		
		    return sb.ToString();
		
		}
		
		/// <summary>
		/// Taille d'un dossier
		/// </summary>
		/// <param name="dossier">Dossier dont il faut calculer la taille</param>
		/// <returns>Taille du dosser en MB</returns>
		public static long tailleDossier (string dossier)
		{
			var fichiers = Directory.EnumerateFiles(dossier, "*", SearchOption.AllDirectories);
			long sum = (from file in fichiers let fileInfo = new FileInfo(file) select fileInfo .Length).Sum();
			return sum;
		}
		
	}
}
