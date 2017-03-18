using System;

namespace Saw
{
	public abstract class Transaction
	{
		protected Md5Fichier	_md5f;
		protected int 			_noPart = -1;
		protected string 		_fichier;
		
		public Md5Fichier 	Md5f		{ get { return _md5f; } }
		public int			NoPart 		{ get { return _noPart; } }
		public string		Fichier 	{ get { return _fichier; } }
	}
}
