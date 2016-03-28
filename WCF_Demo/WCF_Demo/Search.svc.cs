using System.Linq;
using WCF_Demo.Dal;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Search" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Search.svc or Search.svc.cs at the Solution Explorer and start debugging.
	public class Search : ISearch
	{
		public User DoWork(int userId)
		{
			using (var edm = new TestEntities())
			{
				return edm.Users.SingleOrDefault(s => s.UserID == userId);
			}
		}
	}
}
