using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
	internal class Program
	{
		static bool isClosing = false;
		static void Main(string[] args)
		{
			try
			{

				String Version = "1.0";
				String Application = "Blacksite";
				String IP = "45.131.111.215";
				String Key = "Blacksite-ZKOPG88N18QXW2MMJFYGI9C0";
				NetworkManager.LoginSystemV1 Webreq = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 test = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 ChatManager = new NetworkManager.LoginSystemV1(IP, Application, Version);

				Thread.Sleep(2000);
				#region Network & Security & Console
				NetworkManager.LoginSystemV1 manager = new NetworkManager.LoginSystemV1(IP, Application, Version);

				NetworkManager.LoginSystemV1 IP_Check = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 UpdateManager = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 variable = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 Webhook = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 managerNet = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 ApplicationCheck = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.LoginSystemV1 SessionManager = new NetworkManager.LoginSystemV1(IP, Application, Version);
				NetworkManager.ConsoleDisabler consoleDisabler = new NetworkManager.ConsoleDisabler();
				NetworkManager.Security security = new NetworkManager.Security();

				Task.Factory.StartNew(() => { Webreq.Webkey(); });
				/*
				Task.Run(() =>
				{
					while (true)
					{
						NetworkManager.LoginSystemV1 KilledSessionCheck = new NetworkManager.LoginSystemV1(IP, Application, Version);
						Thread.Sleep(5000);
						KilledSessionCheck.GetKilledSession(new NetworkManager.Security().CPUID(), Application);
						KilledSessionCheck.Close();
					}

				});
				*/


				consoleDisabler.EnableCloseButton();
				IP_Check.isIP_Blocked(NetworkManager.Security.Debugger.GetIP(), security.CPUID());
				//IP_Check.sendPacket("IP " + " " + Security.Debugger.GetIP() + " " + security.CPUID());
				String Discord = variable.GetVar("BlacksiteDiscord");
				String Web = Webhook.GetVar("DownloadFiles");
				NetworkManager.Security.Debugger debugger = new NetworkManager.Security.Debugger(Web);
				Thread SecurityLog = new Thread(debugger.DebuggerCheck);
				#endregion

				Console.WriteLine("URL: " + test.GetVar("DownloadFiles"));
				Console.WriteLine("Webhook:" + Web);
				SecurityLog.Start();
				Console.WriteLine("HWID: " + security.CPUID());
				ApplicationCheck.CheckApplication();
				manager.isBlocked();
				UpdateManager.CheckUpdate();
				Console.WriteLine("Variable: " + Discord);
				//if (manager.Bl)
				String HWID = security.CPUID();
				bool Login = managerNet.LicenseLogin(Key, HWID);
				Console.WriteLine("Login Acces: " + Login);
				SessionManager.OpenSession(security.CPUID(), Key);


				#region Manager Close
				manager.Close();
				UpdateManager.Close();
				ApplicationCheck.Close();
				variable.Close();
				Webhook.Close();
				IP_Check.Close();
				#endregion
				//	ApplicationClose(SessionManager.tcpClient);
				Thread.Sleep(-1);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

		}
		public static void ApplicationClose(TcpClient session)
		{
			session.Close();
			Environment.Exit(0);
		}
		public static bool IsKeyExpired(DateTime createDate, DateTime expireDate)
		{
			DateTime currentDate = DateTime.Now.Date; // Get the current date without time
			return createDate >= expireDate;
		}

	}
}
