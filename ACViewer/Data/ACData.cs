using ACE.Common;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACViewer
{
    /// <summary>
    /// Loads data from cell.dat and portal.dat
    /// </summary>
    public static class ACData
    {
        /// <summary>
        /// The folder containing AC .dat files
        /// </summary>
        public static string ACFolder;

        /// <summary>
        /// The folder containing the ACE config.json
        /// For connecting to the database to load dynamic weenies
        /// </summary>
        public static string ACEFolder;

        /// <summary>
        /// LandHeightTable for building mesh
        /// </summary>
        public static RegionDesc RegionDesc;

        public static void Init()
        {
            ACFolder = @"j:\AC\";
            ACEFolder = @"c:\ACE\";

            ConfigManager.Initialize(ACEFolder + "Config.json");
            DatManager.Initialize(ACFolder);

            // load the region file from portal.dat
            RegionDesc = DatManager.PortalDat.ReadFromDat<RegionDesc>(0x13000000);
        }
    }
}
