using System.ServiceModel;

namespace WCF_Demo
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRemove" in both code and config file together.
	[ServiceContract]
	public interface IRemove
	{
		[OperationContract]
		bool DoWork(int userId);
	}
}
