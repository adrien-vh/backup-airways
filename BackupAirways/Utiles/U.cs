using System;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace Saw
{
	static internal class U
	{
		
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
				Logger.Information(e.Message);
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
