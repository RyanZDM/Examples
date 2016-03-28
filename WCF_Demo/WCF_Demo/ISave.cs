using System.ServiceModel;
using WCF_Demo.Dal;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISave" in both code and config file together.
	[ServiceContract]
	public interface ISave
	{
		[OperationContract]
		void DoWork(User user);
	}
}
