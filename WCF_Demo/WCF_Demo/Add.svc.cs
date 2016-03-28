using System.Diagnostics;
using WCF_Demo.Dal;


namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Add" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Add.svc or Add.svc.cs at the Solution Explorer and start debugging.
	public class Add : IAdd
	{
		public void DoWork(User user)
		{
			Trace.Assert(user != null);

			using (var edm = new TestEntities())
			{
				edm.Users.Add(user);
				edm.SaveChanges();
			}
		}
	}
}
