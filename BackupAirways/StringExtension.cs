namespace Saw
{
	public static class StringExtension
	{
		public static char DernierCaractere(this string str)
        {
			return str[str.Length - 1];
        }
		
		public static string WithEndingSlash(this string str)
		{
			if (C.IS_LINUX) {
				return str.DernierCaractere () == '/' ? str : str + "/";
			}
			return str.DernierCaractere() == '\\' ? str : str + "\\";
		}
		
		public static string WithoutEndingSlash(this string str)
		{
			if (str[str.Length -1] == '\\' || str[str.Length -1] == '/')
			{
				return str.Remove(str.Length - 1);
			}
			return str;
		}
	}
}
