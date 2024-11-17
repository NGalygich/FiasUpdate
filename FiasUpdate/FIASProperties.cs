using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiasUpdate
{
    class FIASProperties
    {
        public static string DBName => "FIAS_GAR";
        //public static string GAR_Delta => $@"{GAR_Common}\gar_delta_xml";
        //public static string GAR_Full => $@"{GAR_Common}\gar_xml";
        //public static string GAR_XSD => $@"{GAR_Common}\gar_schemas";
        public static string SQLConnection => "Data Source=SRV-DB-FIAS;Initial Catalog=FIAS_GAR;User ID=sa;Password=compasdt;Connection Timeout=3600;TrustServerCertificate=True";
        //public static string SQLConnection => "Data Source=SRV-NAV-TEST-1;Initial Catalog=FIAS_GAR;Integrated Security=False;User ID=sa;Password=compasdt;TrustServerCertificate=True";
        //public static string SQLConnection2 => "Data Source=SRV-NAV-TEST-1;Initial Catalog=FIAS_GAR;Integrated Security=True;Application Name=FIAS Update";
       
        //private static string GAR_Common => Settings.Default.XMLPath;
    }
}
