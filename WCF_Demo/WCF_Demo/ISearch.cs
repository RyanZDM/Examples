using System.ServiceModel;
using WCF_Demo.Dal;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISearch" in both code and config file together.
	[ServiceContract]
	public interface ISearch
	{
		[OperationContract]
		User DoWork(int userId);
	}
}
