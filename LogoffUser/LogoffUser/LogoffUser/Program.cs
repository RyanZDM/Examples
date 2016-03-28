using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogoffUser
{
	class Program
	{
		static void Main(string[] args)
		{
			NativeMethods.LogoffSession();
		}
	}
}
