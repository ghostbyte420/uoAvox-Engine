using System;
using System.IO;
using System.Security.Cryptography;

namespace Server.Engine.Facet
{
	public static class FacetEditingSettings
	{
		#region Manually Defined Save Directories

		public static string FileExportRootFolderPath => Path.Combine(Core.BaseDirectory, "Export", "Facet");

		public static string ModifiedClientFilesSavePath => Path.Combine(FileExportRootFolderPath, "MapFiles");

		public static string LiveRealTimeChangesSavePath => Path.Combine(FileExportRootFolderPath, "Changes");

		#endregion

		#region Server Facet Module: LumberHarvest

		public static bool LumberHarvestModuleEnabled { get; set; } = true;

		public static string LumberHarvestFallenTreeSaveLocation => Path.Combine(FileExportRootFolderPath, "Modules", "LumberHarvest");

		#endregion

		#region Unique Server ID Generation

		private static string _UID;

		public static string UniqueServerID => _UID ??= AcquireUID();

		private static string AcquireUID()
		{
			string uid = null;

			var cfg = Path.Combine(FileExportRootFolderPath, "UniqueServerID.cfg");

			if (File.Exists(cfg))
			{
				uid = File.ReadAllText(cfg);
			}

			var old = uid;

			if (String.IsNullOrWhiteSpace(uid))
			{
				uid = RandomNumberGenerator.GetHexString(28);
			}
			else if (uid.Length > 28)
			{
				uid = uid.Substring(0, 28);
			}

			if (old != uid)
			{
				File.WriteAllText(cfg, uid);
			}

			return uid;
		}

		#endregion
	}
}