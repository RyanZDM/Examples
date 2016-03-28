#define _USE_NLOG_

using Topshelf;

namespace ServiceTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = HostFactory.New(config =>
			{
#if _USE_NLOG_
				config.UseNLog();
#else
				config.UseLog4Net();
#endif
				config.SetServiceName("ExampleService");
				config.SetDisplayName("Example Service Display Name");
				config.SetDescription("Example service description.");
				//config.DependsOn("<todo>");
				config.StartAutomatically();
				config.EnableSessionChanged();
				config.RunAsLocalSystem();

				config.EnableServiceRecovery(sr =>
				{
					sr.RestartService(1);
					sr.RestartService(1);
					sr.RunProgram(0, @"C:\Windows\Test.exe");
				});
				
				config.Service<ExampleService>(x =>
				{
					x.ConstructUsing(setting => new ExampleService() );
					x.WhenStarted(s => s.Start());
					x.WhenStopped(s => s.Stop());
					x.WhenShutdown(s => s.Shutdown());
					x.WhenSessionChanged((s, hostControl, sessionArgs) => s.OnSessionChanged(sessionArgs));
				});
			});

			host.Run();
		}
	}
}
