using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bahar.Domain.Data.Dto
{
    public class AppSettings
    {
        public bool IsMediaEnabled { get; set; }

        /// <summary>
        /// The lowest Android versionCode allowed to run. A copy of the app below it
        /// shows a screen asking the reader to update and goes no further. 0 turns
        /// the check off. Read on every request, so raising it in appsettings.json on
        /// the server takes effect without a redeploy.
        /// </summary>
        public int MinVersionCode { get; set; }
    }

}
