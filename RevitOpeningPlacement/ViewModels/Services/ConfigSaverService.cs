﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitClashDetective.Models;

namespace RevitOpeningPlacement.ViewModels.Services {
    internal class ConfigSaverService {
        public void Save(ProjectConfig config) {
            var saveWindow = GetPlatformService<ISaveFileDialogService>();
            saveWindow.AddExtension = true;
            saveWindow.Filter = "ClashConfig |*.json";
            saveWindow.FilterIndex = 1;
            if(!saveWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "config")) {
                throw new OperationCanceledException();
            }
            var configSaver = new ConfigSaver();
            configSaver.Save(config, saveWindow.File.FullName);
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
