using System.Linq;
using WCF_Demo.Dal;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Get" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Get.svc or Get.svc.cs at the Solution Explorer and start debugging.
	public class Get : IGet
	{
		public User DoWork(int userId)
		{
			using (var edm = new TestEntities())
			{
				return edm.Users.Single(s => s.UserID == userId);
			}
		}
	}
}
