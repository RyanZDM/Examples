using System.Diagnostics;
using System.Linq;
using WCF_Demo.Dal;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Save" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Save.svc or Save.svc.cs at the Solution Explorer and start debugging.
	public class Save : ISave
	{
		public void DoWork(User user)
		{
			Trace.Assert(user != null);
			using (var edm = new TestEntities())
			{
				var originalUser = edm.Users.Find(user.UserID);
				originalUser.Name = user.Name;
				originalUser.Password = user.Password;
				originalUser.Describe = user.Describe;
				originalUser.SubmitTime = user.SubmitTime;
				edm.SaveChanges();
			}
		}
	}
}
