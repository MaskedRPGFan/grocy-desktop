﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GrocyDesktop
{
	public class GrocyEnvironmentManager
	{
		public GrocyEnvironmentManager(string grocyBasePath, string grocyDataPath, int desiredPort = -1)
		{
			this.BasePath = grocyBasePath;
			this.DataPath = grocyDataPath;

			if (desiredPort == -1)
			{
				this.Port = Extensions.GetRandomFreePort();
			}
			else
			{
				if (Extensions.IsPortFree(desiredPort))
				{
					this.Port = desiredPort;
				}
				else
				{
					this.Port = Extensions.GetRandomFreePort();
				}
			}
		}

		private string BasePath;
		private string DataPath;
		public int Port { get; private set; }

		public string IpUrl
		{
			get
			{
				return "http://" + Extensions.GetNetworkIp() + ":" + this.Port.ToString();
			}
		}

		public string HostnameUrl
		{
			get
			{
				return "http://" + Extensions.GetHostname() + ":" + this.Port.ToString();
			}
		}

		public string LocalUrl
		{
			get
			{
				return "http://localhost:" + this.Port.ToString();
			}
		}

		public void Setup()
		{
			File.WriteAllText(Path.Combine(this.BasePath, "embedded.txt"), this.DataPath);
			this.SetSetting("CULTURE", this.GuessLocalization());
			this.SetSetting("BASE_URL", "/");
			this.SetSetting("CURRENCY", new RegionInfo(CultureInfo.CurrentCulture.LCID).ISOCurrencySymbol);

			Extensions.CopyFolder(Path.Combine(this.BasePath, "data"), this.DataPath);
			File.Copy(Path.Combine(this.BasePath, "config-dist.php"), Path.Combine(this.DataPath, "config.php"), true);

			foreach (string item in Directory.GetFiles(Path.Combine(this.DataPath, "viewcache")))
			{
				File.Delete(item);
			}
		}

		public void SetSetting(string name, string value)
		{
			string settingOverridesFolderPath = Path.Combine(this.DataPath, "settingoverrides");
			if (!Directory.Exists(settingOverridesFolderPath))
			{
				Directory.CreateDirectory(settingOverridesFolderPath);
			}

			File.WriteAllText(Path.Combine(settingOverridesFolderPath, name + ".txt"), value);
		}

		private List<string> GetAvailableLocalizations()
		{
			List<string> list = new List<string>();
			foreach (string item in Directory.GetDirectories(Path.Combine(this.BasePath, "localization")))
			{
				list.Add(Path.GetFileName(item));
			}
			return list;
		}

		private string GuessLocalization()
		{
			string systemCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();

			foreach (string item in this.GetAvailableLocalizations())
			{
				if (item.StartsWith(systemCulture, System.StringComparison.OrdinalIgnoreCase))
				{
					return systemCulture;
				}
			}

			return "en";
		}
	}
}
