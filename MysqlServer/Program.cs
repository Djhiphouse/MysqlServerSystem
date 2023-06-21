using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MysqlServer
{
	internal class Program
	{

		public enum Options
		{
			Register,
			LicenseLogin,
			Login,
			License,
			GenerateLicense,
			Deletelicense,
			Acountsharing,
			Loginrequest,
			Blocked,
			HWIDBlock,
			UnBlockHWID,
			Security,
			AntiCopy,
			ScreenShot,
			GetallUser,
			Variable,
			VersionCheck,
			Status,
			IP,
			Session,
			SessionCheck
		}
		public static MySqlConnection conn;
		private static string myConnectionString = "server=45.131.111.215;port=3306;uid=djhiphouse;pwd=K9R6!b-lxxahoJ2W;database=my-personal-manager;Max Pool Size=9000;Pooling=true;";

		public static int OnlineUser { get; set; }

		static void Main(string[] args)
		{
			LicenseWebServer webServer = new LicenseWebServer();
			ChatServer chat = new ChatServer();
			TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), 6005);
			tcpListener.Start();
			Task.Run(() =>
			{
				webServer.Start();
				chat.Main();
			});
			//init();
			while (true)
			{
				Console.Write("Waiting for a connection... ");
				TcpClient client = tcpListener.AcceptTcpClient();
				Task.Run(() => process(client));
			}

		}

		public static async Task process(TcpClient client)
		{
			try
			{

				StreamReader streamReader = new StreamReader(client.GetStream());
				StreamWriter writer = new StreamWriter(client.GetStream());
				Console.WriteLine("Connected!");
				IPEndPoint clientEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
				System.Net.IPAddress clientIPAddress = clientEndpoint.Address;
				try
				{

					Console.WriteLine("Waiting for Client");
					string text = await streamReader.ReadLineAsync();
					Console.WriteLine("Received:" + text);

					if (text.StartsWith(Options.VersionCheck.ToString()))
					{
						String Good = text.Replace("  ", " ");
						string[] array = Good.Replace(Options.VersionCheck.ToString(), "").Split(' ');
						Console.WriteLine("Checking Version....");
						//Console.WriteLine(CheckCurrentVersion(String.Join("", array).ToString()));
						await writer.WriteLineAsync(await AutoUpdate(array[1], array[2]));

					}
					if (text.StartsWith(Options.SessionCheck.ToString()))
					{
						String Good = text.Replace("  ", " ");
						string[] array = Good.Replace(Options.VersionCheck.ToString(), "").Split(' ');
						Console.WriteLine("Checking Killed Session....");
						//Console.WriteLine(CheckCurrentVersion(String.Join("", array).ToString()));
						await writer.WriteLineAsync(await CheckforKilledSession(array[1], array[2]) + "");

					}
					if (text.StartsWith(Options.Session.ToString()))
					{
						bool running = true;
						String Good = text.Replace("  ", " ");
						string[] array = Good.Replace(Options.Session.ToString(), "").Split(' ');
						Console.WriteLine("Created Session +++++++++++++++++++++++++++++++");
						//Console.WriteLine(CheckCurrentVersion(String.Join("", array).ToString()));
						await CreateSession(array[1], array[2], array[3], Int32.Parse(array[4]), array[5], array[6], array[7], client, false);
						OnlineUser++;
						await UpdateOnlineUser();
						while (running)
						{
							try
							{
								// Keep checking if the client is still connected
								await Task.Delay(1000); // Adjust the delay as needed
								await writer.WriteAsync("Ping");
								await writer.FlushAsync();
								//Console.WriteLine("Sended");
							}
							catch (Exception e)
							{
								Console.WriteLine("Session Closed");
								await CreateSession(array[1], array[2], array[3], Int32.Parse(array[4]), array[5], array[6], array[7], client, true);
								OnlineUser--;
								await UpdateOnlineUser();
								running = false;
								break;
							}

						}

						// Client is closed, perform cleanup or deletion of session entry


						Console.WriteLine("Closed Session ------------------------------------------------------");


					}
					if (text.StartsWith(Options.Status.ToString()))
					{
						String Good = text.Replace("  ", " ");
						string[] array = Good.Replace(Options.Status.ToString(), "").Split(' ');
						Console.WriteLine("Checking Status....");
						//Console.WriteLine(CheckCurrentVersion(String.Join("", array).ToString()));
						bool check = await isDisabled(array[1]);
						await writer.WriteLineAsync("" + check);

					}
					if (text.StartsWith(Options.Variable.ToString()))
					{
						String Good = text.Replace("  ", " ");
						string[] array = Good.Replace(Options.Variable.ToString(), "").Split(' ');
						//Console.WriteLine("fetch Variable");
						//Console.WriteLine(CheckCurrentVersion(String.Join("", array).ToString()));
						string result = await GetVariable(Good.Replace(Options.Variable.ToString(), "").Replace(" ", ""));
						await writer.WriteLineAsync(result);

					}


					else if (text.StartsWith(Options.LicenseLogin.ToString()))
					{
						Console.WriteLine("Checking login");
						String Good = text.Replace("  ", " ");
						string[] array2 = Good.Replace(Options.LicenseLogin.ToString(), "").Split(' ');
						bool check = await LicenseLoginCheck(array2[1], array2[2], array2[3], clientIPAddress.ToString());
						await writer.WriteLineAsync(check.ToString() ?? "");
						Console.WriteLine("License Login: {0}", check);
					}
					else if (text.StartsWith(Options.Blocked.ToString()))
					{
						String Good = text.Replace("  ", " ");
						string text5 = Good.Replace(Options.Blocked.ToString() + " ", "");
						Console.WriteLine("HWID:" + text5);
						bool check = await Blocked(text5);
						await writer.WriteLineAsync(check.ToString() ?? "");
						Console.WriteLine("Check HWID: {0}", check);
					}
					else if (text.StartsWith(Options.IP.ToString()))
					{
						String Good = text.Replace("  ", " ");
						string[] array2 = Good.Replace(Options.IP.ToString(), "").Split(' ');
						bool check = await IPBlocked(array2[0], array2[1]);
						await writer.WriteLineAsync(check.ToString() ?? "");
						Console.WriteLine("Check IP " + array2[1] + " :  {0}", check);
					}







					//await Log("IP: " + clientIPAddress.ToString() + " Connected!");

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.StackTrace);
				}
				await writer.FlushAsync();
				Console.WriteLine("Client Disconnected!");
				client.Close();
			}
			catch (Exception e)
			{
				await conn.ClearAllPoolsAsync();
			}

		}
		public static async Task<bool> CheckforKilledSession(string Hwid, string application)
		{
			using (MySqlConnection conn = new MySqlConnection(myConnectionString))
			{
				try
				{
					await conn.OpenAsync();

					MySqlCommand cmd = new MySqlCommand("SELECT * FROM session WHERE hwid=@hwid AND application=@Application AND killed=@killed;", conn);
					cmd.Parameters.AddWithValue("@hwid", Hwid);
					cmd.Parameters.AddWithValue("@Application", application);
					cmd.Parameters.AddWithValue("@killed", "1");
					using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
					{
						if (!reader.HasRows)
						{
							Console.WriteLine("Cannot find Session With HWID: " + Hwid);
							return false;
						}
					}

					return true;
					await conn.CloseAsync();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					await conn.CloseAsync();
					return false;
				}
				finally
				{
					await conn.CloseAsync();
				}
			}
		}
		public static async Task<bool> IPBlocked(string IP, String Hwid)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				//await Log("Update Blacklist IP: " + IP);
				MySqlCommand UpdateIp = new MySqlCommand("UPDATE user SET ip= '" + IP + "'  WHERE hwid='" + Hwid + "';", conn);//
				await UpdateIp.ExecuteNonQueryAsync();
				MySqlCommand IPCheck = new MySqlCommand("SELECT * FROM blacklist WHERE ip= '" + IP + "';", conn);//
				MySqlDataReader Check = IPCheck.ExecuteReader();
				await ((DbDataReader)(object)Check).ReadAsync();

				//Console.WriteLine("Status of HWID:" + HWID + "#     " + Data.GetInt32("blocked"));
				if (((DbDataReader)(object)Check).HasRows)
				{
					await Log("Blocked IP: " + IP + " Try to Use tool!", "Blocked");
					await conn.CloseAsync();
					return true;
				}
				await conn.CloseAsync();
				return false;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return false;
			}
		}
		public static async Task<bool> isDisabled(String Application)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("SELECT `Application`, `Status` FROM `my-personal-manager`.`settings` WHERE  `Application`='" + Application + "';", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				Console.WriteLine("Checking Application!");

				if (Data.GetInt32("Status") == 1)
				{
					//await Log("Blocked HWID: " + HWID + " Try to Use tool");
					Console.WriteLine("Application is Blocked!");
					await Data.CloseAsync();
					return true;
				}
				else
				{
					await Data.CloseAsync();
					return false;
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return false;
			}
		}

		public static async Task<String> AutoUpdate(String Application, String Version)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("SELECT `Application`, `Version` FROM `my-personal-manager`.`settings` WHERE  `Application`='" + Application + "';", conn);
				MySqlCommand UpdateLink = new MySqlCommand("SELECT `Application`, `Updatelink` FROM `my-personal-manager`.`settings` WHERE  `Application`='" + Application + "';", conn);
				MySqlCommand UpdateMessage = new MySqlCommand("SELECT `Application`, `UpdateMsg` FROM `my-personal-manager`.`settings` WHERE  `Application`='" + Application + "';", conn);
				MySqlDataReader Data = val.ExecuteReader();

				await ((DbDataReader)(object)Data).ReadAsync();

				if (!((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("Cant not find Application : " + Application);
					await conn.CloseAsync();
					return "Not found";
				}

				if (!((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("Cant not find Update");
					await conn.CloseAsync();
					return "Update Not found";
				}

				if (Data.GetString("Version") != Version)
				{
					await Data.CloseAsync();
					MySqlDataReader Updater = UpdateLink.ExecuteReader();
					await ((DbDataReader)(object)Updater).ReadAsync();
					await Log("User Update his Application: " + Application + " Old Version: " + Version, Application);

					String resultt = Updater.GetString("Updatelink");
					await Data.CloseAsync();
					return resultt;
				}

				await Data.CloseAsync();
				MySqlDataReader Updatemsgdata = UpdateMessage.ExecuteReader();
				await ((DbDataReader)(object)Updatemsgdata).ReadAsync();
				String result = Updatemsgdata.GetString("UpdateMsg");
				await Data.CloseAsync();
				return Updatemsgdata.GetString("UpdateMsg");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return "Not found";
			}
		}

		public static bool IsKeyExpired(DateTime expireDate)
		{
			DateTime currentDate = DateTime.Now.Date; // Get the current date without time
			return DateTime.Now >= expireDate;
		}
		public static async Task<bool> LicenseLoginCheck(string License, string hwid, string Application, String ip)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				//Console.WriteLine("#########" + Application + "##########");
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("SELECT * FROM license WHERE licensekey=\"" + License + "\";", conn);//
				MySqlCommand hwidcmd = new MySqlCommand("UPDATE `my-personal-manager`.`license` SET `hwid`='" + hwid + "', `used`=1 WHERE  `licensekey`='" + License + "';", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				//Key  icht gefunden
				if (!((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("Try to Find invalid Key!");
					await Log("User: " + License + " HWID: " + hwid + " Try to Login with invalid Key.", Application);
					await conn.CloseAsync();
					return false;
				}




				if (Data.GetInt32("blocked") == 1)
				{
					await Log("User: " + License + " HWID: " + hwid + " Try to Login with Blocked License.", Application);
					await conn.CloseAsync();
					return false;
				}


				if (Data.GetString("application") != Application)
				{
					await Log("User: " + License + " HWID: " + hwid + " Try to Login with Wrong Application.", Application);
					await conn.CloseAsync();
					return false;
				}




				if (Data.GetInt32("valid") == 0)
				{
					await Log("User: " + License + " HWID: " + hwid + " Try to Login with invalid Key.", Application);
					Console.WriteLine("Key invalid or Blocked");
					await conn.CloseAsync();
					return false;
				}
				String expiredate = Data.GetString("expire_date");
				//Data.GetString("created_at")
				if (expiredate != "0")
				{
					DateTime expireDate = DateTime.Parse(expiredate);

					if (IsKeyExpired(expireDate))
					{
						Console.WriteLine("Key Expired");
						await Log("User: " + License + " HWID: " + hwid + " Try to Login with Expired Key.", Application);
						await conn.CloseAsync();
						return false;
					}
				}


				//HWID check & Wenn falsch Login denied
				if (Data.GetString("hwid") == hwid && Data.GetInt32("used") == 1)
				{
					///WebHookManager.LoginInfo("https://discord.com/api/webhooks/1005222076715647068/kjJr98OQn_r9FYK7ZhONZnyfYPTACgMiHSuYe_KiaOkNzBZ3-YqgKH0kdMC3eiZ399YZ", License, hwid);
					///
					Console.WriteLine("HWID Correkt and Checked");



					await Log("User: " + License + " HWID: " + hwid + " Logged in Sucessfully.", Application);

					await RegisterUser(hwid, ip, 0, Application);
					Console.WriteLine("a User are Login Sucessfully!");
					await conn.CloseAsync();
					return true;
				}
				else
				{
					//WebHookManager.AcountSharing("https://discord.com/api/webhooks/1005222261025931264/Drm90w4uZX2C4QAgFYuUGjafDTmYFyGtzwsLsoehwiSQWrEmVnp-UGY1rSk1oXFjh4RE", Name, hwid, Password);


					Console.WriteLine("Hwid 0 setup to real hwid");

					if (Data.GetString("hwid") == "0" && Data.GetInt32("used") == 0)
					{
						await Data.CloseAsync();
						await hwidcmd.ExecuteNonQueryAsync();
						await Log("User: " + License + " HWID: " + hwid + " Logged in Sucessfully.", Application);
						await RegisterUserUpdate();
						await RegisterUser(hwid, ip, 0, Application);
						await UpdateLicensedate(License);
						Console.WriteLine("a User are Login Sucessfully!");
						await conn.CloseAsync();
						return true;
					}
					else
					{
						await Log("User: " + License + " HWID: " + hwid + " Try to Login with Wrong HWID or Already used key.", Application);
						await conn.CloseAsync();
						return false;
					}

				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: " + e.ToString());
				await conn.CloseAsync();
				return false;


			}
		}

		public static async Task<Task> UpdateLicensedate(String License)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("SELECT * FROM license WHERE licensekey='" + License + "'", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				if (!((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("not found");
				}
				int days = Data.GetInt32("days");

				DateTime expire = DateTime.Now.AddDays(days);
				DateTime currentdate = DateTime.Now;
				string epiredate = expire.ToString("yyyy-MM-dd");
				string CurrentDATE = currentdate.ToString("yyyy-MM-dd");

				await Data.CloseAsync();

				MySqlCommand update = new MySqlCommand("UPDATE `my-personal-manager`.`license` SET `expire_date`='" + epiredate + "', `used_date`='" + CurrentDATE + "' WHERE  `licensekey`='" + License + "';", conn);
				await update.ExecuteNonQueryAsync();

				await conn.CloseAsync();
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
		}
		public static async Task<Task> Log(String Log, String Appliaction)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("INSERT INTO `my-personal-manager`.`logs` (`log`, `time`, `application`) VALUES ('" + Log + "', '" + DateTime.Now + "', '" + Appliaction + "');", conn);
				await val.ExecuteNonQueryAsync();
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
		}
		public static async Task<int> CreateSession(string hwid, string pcname, string license, int status, string date, string clock, string application, TcpClient session, bool delete)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await conn.OpenAsync();
				if (!delete)
				{
					MySqlCommand openSessionCommand = new MySqlCommand("INSERT INTO `my-personal-manager`.`session` (`hwid`, `pcname`, `license`, `status`, `clock`, `date`, `application`, `killed`) VALUES (@hwid, @pcname, @license, @status, @clock, @date,@application, @killed);", conn);
					openSessionCommand.Parameters.AddWithValue("@hwid", hwid);
					openSessionCommand.Parameters.AddWithValue("@pcname", pcname);
					openSessionCommand.Parameters.AddWithValue("@license", license);
					openSessionCommand.Parameters.AddWithValue("@status", status);
					openSessionCommand.Parameters.AddWithValue("@clock", clock);
					openSessionCommand.Parameters.AddWithValue("@date", date);
					openSessionCommand.Parameters.AddWithValue("@application", application);
					openSessionCommand.Parameters.AddWithValue("@killed", "0");
					await openSessionCommand.ExecuteNonQueryAsync();
					Console.WriteLine($"User Hwid: {hwid} Session Created at {clock} {date}!");
					await Log("Session Created From HWID: " + hwid + " License: " + license + " AT Time:" + clock + " Date:" + date, application);
					await conn.CloseAsync();
				}
				else
				{
					MySqlCommand closeSessionCommand = new MySqlCommand("DELETE FROM session WHERE hwid = @hwid;", conn);
					closeSessionCommand.Parameters.AddWithValue("@hwid", hwid);
					await closeSessionCommand.ExecuteNonQueryAsync();
					await Log("Session Closed From HWID: " + hwid + " License: " + license + " AT Time" + DateTime.Now, application);
					Console.WriteLine($"Session closed: {hwid}");
					await conn.CloseAsync();
				}

				return 0; // Session closed
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return -1; // Error occurred
			}
			finally
			{
				await conn.CloseAsync();
			}
		}

		public static async Task<Task> UpdateOnlineUser()
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand onlineuser = new MySqlCommand("UPDATE applicationinfo SET onlineUser='" + OnlineUser + "';", conn);
				MySqlCommand connections = new MySqlCommand("UPDATE applicationinfo SET connections='" + OnlineUser + "';", conn);
				await onlineuser.ExecuteNonQueryAsync();
				await connections.ExecuteNonQueryAsync();
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
		}
		public static async Task<Task> RegisterUserUpdate()
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("SELECT * FROM applicationinfo", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				if (!((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("Cant not find RegisterUser!");
					await conn.CloseAsync();
					return Task.CompletedTask;
				}
				int Regusers = Data.GetInt32("registeredUser");
				await Data.CloseAsync();
				MySqlCommand Reguserentry = new MySqlCommand("UPDATE applicationinfo SET registeredUser='" + Regusers + 1 + "';", conn);
				await Reguserentry.ExecuteNonQueryAsync();

				await conn.CloseAsync();
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
		}
		public static async Task<Task> RegisterUser(String Hwid, String Ip, int Blocked, String Application)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand reguser = new MySqlCommand("INSERT INTO `my-personal-manager`.`user` (`hwid`, `ip`, `blocked`, `application`) VALUES ('" + Hwid + "', '" + Ip + "', " + Blocked + ", '" + Application + "');", conn);
				MySqlCommand val = new MySqlCommand("Select * from user where `hwid`='" + Hwid + "' and `application`='" + Application + "';", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				if (!((DbDataReader)(object)Data).HasRows)
				{
					await Data.CloseAsync();
					await Log("Register HWID " + Hwid, Application);
					await reguser.ExecuteNonQueryAsync();
				}
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
		}
		public static async Task<Task> Register(String hwid, String ip, bool blocked)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				int block;
				if (blocked)
					block = 1;
				else
					block = 0;
				//UPDATE `my-personal-manager`.`license` SET `blocked`=1 WHERE  `licensekey`='Key-)x&9&&bLqKa!f(fZyOBloefG';
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("INSERT INTO `my-personal-manager`.`user` (`hwid`, `ip`, `blocked`) VALUES('" + hwid + "', '" + ip + "', '" + block + "'); ", conn);

				MySqlCommand blockhim = new MySqlCommand("UPDATE `my-personal-manager`.`license` SET `blocked`=" + block + " WHERE  `hwid`='" + hwid + "';", conn);
				await blockhim.ExecuteNonQueryAsync();


				///await val.ExecuteNonQueryAsync();
				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return Task.CompletedTask;
			}
		}
		public static async Task<String> GetVariable(String Variable)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("SELECT `name`, `value` FROM `my-personal-manager`.`variable` WHERE  `name`='" + Variable + "'", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				if (!((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("Cant not find Variable : " + Variable);
					return "Not found";
				}
				//await Log("Client Fetch Vairable: " + Variable);
				return Data.GetString("value");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return "Not found";
			}
			finally
			{
				await conn.CloseAsync();
			}
		}
		public static async Task<bool> Blocked(string HWID)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand check1 = new MySqlCommand("SELECT * FROM `my-personal-manager`.`license` WHERE `hwid` = '" + HWID + "' AND `blocked` = 1;", conn);//
				MySqlCommand check2 = new MySqlCommand("SELECT * FROM `my-personal-manager`.`user` WHERE `hwid` = '" + HWID + "' AND `blocked` = 1;\r\n;", conn);//
				MySqlDataReader CheckMethod1 = check1.ExecuteReader();
				await ((DbDataReader)(object)CheckMethod1).ReadAsync();

				//Console.WriteLine("Status of HWID:" + HWID + "#     " + Data.GetInt32("blocked"));
				if (((DbDataReader)(object)CheckMethod1).HasRows)
				{
					await Log("Blocked HWID: " + HWID + " Try to Use tool Check!", "Check");
					await conn.CloseAsync();
					return true;
				}
				else
				{
					await CheckMethod1.CloseAsync();
					MySqlDataReader CheckMethod2 = check2.ExecuteReader();
					await ((DbDataReader)(object)CheckMethod2).ReadAsync();
					if (((DbDataReader)(object)CheckMethod2).HasRows)
					{
						await Log("Blocked HWID: " + HWID + " Try to Use tool Check!", "Check");
						await conn.CloseAsync();
						return true;
					}
				}
				await conn.CloseAsync();
				return false;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				await conn.CloseAsync();
				return false;
			}
		}

		public static async Task<bool> GenerateLicenseKey(Enum LicenseKey, string license, String Application, int Days)
		{
			MySqlConnection conn = new MySqlConnection(myConnectionString);
			try
			{
				await ((DbConnection)(object)conn).OpenAsync();
				MySqlCommand val = new MySqlCommand("INSERT INTO `my-personal-manager`.`license` (`licensekey`, `created_at`, `expire_date`, `used_date`, `hwid`, `blocked`, `used`, `application`, `days`) VALUES ('" + LicenseKey + "', '" + DateTime.Now + "', '0', '0', '1', 0, 0, 0, '" + Application + "', " + Days + ");", conn);
				MySqlDataReader Data = val.ExecuteReader();
				await ((DbDataReader)(object)Data).ReadAsync();
				if (((DbDataReader)(object)Data).HasRows)
				{
					Console.WriteLine("Try to Generate Already used License!");
					return false;
				}
				await conn.CloseAsync();
				return true;
			}
			finally
			{
				await conn.CloseAsync();
			}
		}


	}
	public class LicenseWebServer
	{
		private static string myConnectionString = "server=45.131.111.215;port=3306;uid=djhiphouse;pwd=K9R6!b-lxxahoJ2W;database=my-personal-manager;Max Pool Size=9000;Pooling=true;";
		public HttpListener listener = new HttpListener();

		public LicenseWebServer()
		{
			listener.Prefixes.Add("http://45.131.111.215:9999/GetLicense/");
		}

		public async Task<Task> Start()
		{
			listener.Start();
			Console.WriteLine("Server started.");


			while (true)
			{
				// Wait for an incoming request
				HttpListenerContext context = await listener.GetContextAsync();
				Console.WriteLine("Web request received!");

				// Get the request and response objects
				HttpListenerRequest request = context.Request;
				HttpListenerResponse response = context.Response;

				// Extract the URL parameters
				string application = GetUrlParameter(request.Url, "application");
				string sellerKey = GetUrlParameter(request.Url, "sellerkey");
				string daysString = GetUrlParameter(request.Url, "days");
				int days = 0;
				int.TryParse(daysString, out days);

				// Perform the necessary checks
				string responseString = await ProcessRequest(application, sellerKey, days);

				// Send the response
				await SendResponse(response, responseString);
			}
		}
		private async Task<string> ProcessRequest(string application, string sellerKey, int days)
		{
			if (string.IsNullOrEmpty(sellerKey) || string.IsNullOrEmpty(application))
			{
				// Invalid parameters, send an error response
				return "<html><body><h1>Invalid parameters!</h1></body></html>";
			}

			if (await CheckSellerKey(sellerKey, application))
			{
				string key = GenerateLicenseKey(application, days);
				return "Key: " + application + "-" + key + "";
			}

			return "<html><body><h1>Invalid SellerKey or Application!</h1></body></html>";
		}
		private string GetUrlParameter(Uri url, string parameterName)
		{
			string queryString = url.Query;
			NameValueCollection queryParameters = HttpUtility.ParseQueryString(queryString);
			return queryParameters[parameterName];
		}


		public static string GenerateLicenseKey(string application, int days)
		{
			using (MySqlConnection conn = new MySqlConnection(myConnectionString))
			{
				try
				{
					conn.Open();
					string regx = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
					StringBuilder licenseKey = new StringBuilder();
					Random random = new Random();

					for (int i = 0; i < 24; i++)
					{
						int j = random.Next(regx.Length);
						licenseKey.Append(regx[j]);
					}

					string formattedDate = DateTime.Now.ToString("dd.MM.yyyy");

					MySqlCommand cmd = new MySqlCommand("INSERT INTO `my-personal-manager`.`license` (`licensekey`, `created_at`, `expire_date`, `used_date`, `hwid`, `blocked`, `used`, `application`, `days`) VALUES (@LicenseKey, @CreatedAt, '0', '0', '0', 0, 0, @Application, @Days);", conn);
					cmd.Parameters.AddWithValue("@LicenseKey", application + "-" + licenseKey.ToString());
					cmd.Parameters.AddWithValue("@CreatedAt", formattedDate);
					cmd.Parameters.AddWithValue("@Application", application);
					cmd.Parameters.AddWithValue("@Days", days);

					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.HasRows)
						{
							Console.WriteLine("Try to Generate Already used License!");
							Console.WriteLine("Already row");
							return "Failed: License already used.";
						}
					}

					Console.WriteLine("Return Success");
					return licenseKey.ToString();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					return "Failed";
				}
				finally
				{
					Console.WriteLine("Close");
					conn.Close();
				}
			}



		}

		public static async Task<bool> CheckSellerKey(string sellerKey, string application)
		{
			using (MySqlConnection conn = new MySqlConnection(myConnectionString))
			{
				try
				{
					await conn.OpenAsync();

					MySqlCommand cmd = new MySqlCommand("SELECT * FROM sellerapi WHERE apikey=@SellerKey AND application=@Application;", conn);
					cmd.Parameters.AddWithValue("@SellerKey", sellerKey);
					cmd.Parameters.AddWithValue("@Application", application);

					using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
					{
						if (!reader.HasRows)
						{
							Console.WriteLine("Cannot find SellerKey: " + sellerKey);
							return false;
						}
					}

					return true;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					return false;
				}
				finally
				{
					await conn.CloseAsync();
				}
			}
		}





		private static async Task SendResponse(HttpListenerResponse response, string responseString)
		{
			// Convert the response string to bytes
			byte[] buffer = Encoding.UTF8.GetBytes(responseString);

			// Set the response headers and content length
			response.ContentLength64 = buffer.Length;
			response.ContentType = "text/html";

			// Write the response to the client
			await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

			// Close the response
			response.Close();
		}
	}
	public class ChatServer
	{
		private const string url = "http://45.131.111.215:9999/GetChat/";
		private static string myConnectionString = "server=45.131.111.215;port=3306;uid=djhiphouse;pwd=K9R6!b-lxxahoJ2W;database=my-personal-manager;Max Pool Size=9000;Pooling=true;";
		public void Main()
		{
			using (HttpListener listener = new HttpListener())
			{
				listener.Prefixes.Add(url);
				listener.Start();
				Console.WriteLine("Server started. Listening for incoming requests...");

				while (true)
				{
					HttpListenerContext context = listener.GetContext();
					ProcessRequest(context);
				}
			}
		}

		public static void ProcessRequest(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;

			if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/GetChat/")
			{
				// Retrieve the chat data
				List<string> chatData = Getchat();

				// Convert the chat data to a single string
				string responseData = string.Join(Environment.NewLine, chatData);

				// Create a response
				byte[] responseBytes = Encoding.UTF8.GetBytes(responseData);
				context.Response.ContentLength64 = responseBytes.Length;
				context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
				context.Response.OutputStream.Close();
			}
			else if (request.HttpMethod == "POST")
			{
				{
					using (StreamReader reader = new StreamReader(request.InputStream))
					{
						string requestBody = reader.ReadToEnd();
						Console.WriteLine("Received request body: " + requestBody);

						// Parse the request body and extract the parameters
						string sender = GetParameterValue(requestBody, "Sender");
						string message = GetParameterValue(requestBody, "Message");

						// Perform the necessary logic to handle the chat message
						// ...

						// Create a response
						// Create a response
						string responseMessage = "Message received and processed: SENDER: " + sender + "   MESSAGE: " + message;
						byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseMessage);
						context.Response.ContentLength64 = responseBytes.Length;
						context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
						context.Response.OutputStream.Close();



						using (MySqlConnection conn = new MySqlConnection(myConnectionString))
						{
							try
							{
								conn.Open();

								using (MySqlCommand cmd = new MySqlCommand("INSERT INTO `my-personal-manager`.`chat` (`sender`, `message`) VALUES (@Sender, @Message);", conn))
								{
									cmd.Parameters.AddWithValue("@Sender", sender);
									cmd.Parameters.AddWithValue("@Message", message.Replace("+", " "));

									// Execute the query
									cmd.ExecuteNonQuery();
								}
							}
							catch (Exception e)
							{
								Console.WriteLine(e.ToString());

							}
							finally
							{
								conn.Close();
							}
						}
					}

					context.Response.Close();
				}
			}
		}

		public static string GetParameterValue(string requestBody, string parameterName)
		{
			// Simple implementation to extract parameter value from request body
			string[] parts = requestBody.Split('&');

			foreach (string part in parts)
			{
				string[] pair = part.Split('=');
				if (pair.Length == 2 && pair[0] == parameterName)
				{
					return pair[1];
				}
			}

			return null;
		}
		public static List<String> Getchat()
		{
			List<string> chatData = new List<string>();
			string query = "SELECT sender, message FROM chat";

			using (MySqlConnection connection = new MySqlConnection(myConnectionString))
			{
				// Open the database connection
				connection.Open();

				using (MySqlCommand command = new MySqlCommand(query, connection))
				{
					using (MySqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							string sender = reader.GetString(0);
							string message = reader.GetString(1);

							// Combine the sender and message into a single string or format it as needed
							string chatEntry = sender + ": " + message;

							// Add the chat entry to the list
							chatData.Add(chatEntry);
						}
					}
				}
			}

			return chatData;
		}
	}
}
