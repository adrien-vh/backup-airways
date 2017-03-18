using System;
using System.Collections.Generic;

namespace Saw
{
	public class DeltaMd5
	{
		public readonly List<Md5Fichier> Md5Ajoutes;
		public readonly List<Md5Fichier> Md5Supprimes;
		
		public DeltaMd5(List<Md5Fichier> md5Ajoutes, List<Md5Fichier> md5Supprimes)
		{
			Md5Ajoutes 		= md5Ajoutes;
			Md5Supprimes 	= md5Supprimes;
		}
	}
}
