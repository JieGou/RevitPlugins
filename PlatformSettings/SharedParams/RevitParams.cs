using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;

namespace PlatformSettings.SharedParams {
    public abstract class RevitParams {
        public string Name { get; set; }
        public string RevitParamsFilePath { get; set; }

        public abstract RevitParamsConfig GetConfig();

        public string GetConfigPath() {
            return string.IsNullOrEmpty(RevitParamsFilePath) ? null : new System.IO.FileInfo(RevitParamsFilePath).FullName;
        }

        public void SaveSettings(RevitParamsConfig revitParamsConfig, string configPath) {
            if(!string.IsNullOrEmpty(configPath)) {
                revitParamsConfig.Save(configPath);
            }
        }
    }
}
