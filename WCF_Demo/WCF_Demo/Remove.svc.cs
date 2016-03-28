using System.Linq;
using WCF_Demo.Dal;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Remove" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Remove.svc or Remove.svc.cs at the Solution Explorer and start debugging.
	public class Remove : IRemove
	{
		public bool DoWork(int userId)
		{
			var removed = false;
			using (var edm = new TestEntities())
			{
				var user = edm.Users.SingleOrDefault(s => s.UserID == userId);
				if (user != null)
				{
					edm.Users.Remove(user);
					edm.SaveChanges();
					removed = true;
				}
			}

			return removed;
		}
	}
}
