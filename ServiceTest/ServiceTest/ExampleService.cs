#define _USE_NLOG_

using Topshelf.Logging;
#if _USE_NLOG_
using NLog;
#else
using log4net;
#endif

using Topshelf;

namespace ServiceTest
{
	public class ExampleService
	{
		#region Private variables
#if _USE_NLOG_
		//private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly Logger logger;
#else
		private readonly ILog logger;
#endif
		#endregion

		public ExampleService()
		{
			logger = LogManager.GetLogger(GetType().FullName);
#if (!_USE_NLOG_)
			log4net.Config.XmlConfigurator.Configure();
#endif
		}

		public bool Start()
		{
			// TODO runtime change log file path
			//var target = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
			//target.FileName = "<path to log file>";

			logger.Info("Service started.");

			var log = HostLogger.Current.Get(GetType().FullName);
			log.Info("Service started. - Another way to write log. ");
	
			return true;
		}

		public bool Stop()
		{
			logger.Info("Service stopped.");
			return true;
		}

		public bool Shutdown()
		{
			logger.Info("Computer shutdown.");
			return true;
		}

		public void OnSessionChanged(SessionChangedArguments args)
		{
#if _USE_NLOG_
			logger.Info("Session changed. Code:[{0}], ID:[{1}].", args.ReasonCode, args.SessionId);
#else
			// logger.Info accepts two arguments only
			logger.InfoFormat("Session changed. Code:[{0}], ID:[{1}].", args.ReasonCode, args.SessionId);
#endif
		}
	}
}
