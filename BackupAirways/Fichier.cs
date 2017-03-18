/*
 * Created by SharpDevelop.
 * User: vanhyftea
 * Date: 06/03/2017
 * Time: 11:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;

namespace Saw
{
	/// <summary>
	/// Description of Fichier.
	/// </summary>
	public class Fichier
	{
		public string Nom;
		public string CheminComplet;
		public bool EstDossier;
		public string Extension;
		
		public static string ExtensionFichier(string chemin)
		{
			int posPoint = chemin.LastIndexOf('.');
			
			if (posPoint < 0) {
				return null;
			}
			return chemin.Substring(chemin.LastIndexOf('.') + 1);
		}
		
		public Fichier(string chemin, string label = null)
		{
			FileAttributes attr = File.GetAttributes(chemin);
			CheminComplet = chemin;
			Nom = CheminComplet.Length == 3 ? (label ?? "Disque ") + " (" + CheminComplet.Substring(0, 2) + ")" : CheminComplet.Substring(CheminComplet.LastIndexOf('\\') + 1);
			EstDossier = attr.HasFlag(FileAttributes.Directory);
			Extension = EstDossier ? "" : ExtensionFichier(chemin);
		}
	}
}
