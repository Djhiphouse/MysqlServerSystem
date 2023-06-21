using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MysqlServer
{
    public class WebHookManager
    {
		public static void AcountSharing(string URL, string Name, string HWID, string Password)
		{
			WebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
			webRequest.ContentType = "application/json";
			webRequest.Method = "POST";
			using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
			{
				string value = JsonConvert.SerializeObject((object)new
				{
					username = "Account Sharing | DJ HIP HOUSE#2002",
					embeds = new[]
					{
					new
					{
						description = "\n [>] Username: " + Name + "\n [>] Password: ||" + Password + "||\n [>] HWID: ||" + HWID + "||",
						title = "Account Sharing Detected",
						color = "15548997"
					}
				}
				});
				streamWriter.Write(value);
			}
			_ = (HttpWebResponse)webRequest.GetResponse();
		}

		public static async Task<bool> Security(string URL, string IP, string HWID)
		{
			WebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
			webRequest.ContentType = "application/json";
			webRequest.Method = "POST";
			using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
			{
				string value = JsonConvert.SerializeObject((object)new
				{
					username = "Account Sharing | DJ HIP HOUSE#2002",
					embeds = new[]
					{
					new
					{
						description = "\n [>] HWID: " + HWID + "\n [>] Password: ||" + IP + "||\n",
						title = "Account Sharing Detected",
						color = "15548997"
					}
				}
				});
				streamWriter.Write(value);
			}
			_ = (HttpWebResponse)webRequest.GetResponse();
			return true;
		}

		public static async Task<bool> AntiCopy(string URL, string HWID, string IP)
		{
			WebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
			webRequest.ContentType = "application/json";
			webRequest.Method = "POST";
			using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
			{
				string value = JsonConvert.SerializeObject((object)new
				{
					username = "AntiCopy | DJ HIP HOUSE#2002",
					embeds = new[]
					{
					new
					{
						description = "\n [>] HWID: " + HWID + "\n [>] IP: ||" + IP + "||\n",
						title = "Methode Sharing Detected",
						color = "15548997"
					}
				}
				});
				streamWriter.Write(value);
			}
			_ = (HttpWebResponse)webRequest.GetResponse();
			return true;
		}

		public static async Task<bool> AntiScreenshot(string URL, string HWID, string IP)
		{
			WebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
			webRequest.ContentType = "application/json";
			webRequest.Method = "POST";
			using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
			{
				string value = JsonConvert.SerializeObject((object)new
				{
					username = "AntiScreemShot | DJ HIP HOUSE#2002",
					embeds = new[]
					{
					new
					{
						description = "\n [>] HWID: " + HWID + "\n [>] IP: ||" + IP + "||\n",
						title = "Methode Sharing Detected",
						color = "15548997"
					}
				}
				});
				streamWriter.Write(value);
			}
			_ = (HttpWebResponse)webRequest.GetResponse();
			return true;
		}

		public static void LoginInfo(string URL, string Name, string Password, string HWID)
		{
			WebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);
			webRequest.ContentType = "application/json";
			webRequest.Method = "POST";
			using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
			{
				string value = JsonConvert.SerializeObject((object)new
				{
					username = "Login | By DJ HIP HOUSE#2002",
					embeds = new[]
					{
					new
					{
						description = "\n [>] Username: " + Name + "\n [>] Password: ||" + Password + "||\n [>] HWID: ||" + HWID + "||",
						title = "Login Detected",
						color = "15548997"
					}
				}
				});
				streamWriter.Write(value);
			}
			_ = (HttpWebResponse)webRequest.GetResponse();
		}

	}
}
