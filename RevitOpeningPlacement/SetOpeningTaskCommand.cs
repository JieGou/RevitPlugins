﻿using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    public class SetOpeningTaskCommand : BasePluginCommand {
        public SetOpeningTaskCommand() {
            PluginName = "Настройка заданий на отверстия";
        }

        protected override void Execute(UIApplication uiApplication) {
            var openingConfig = OpeningConfig.GetOpeningConfig();
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            var viewModel = new MainViewModel(revitRepository, openingConfig);

            var window = new MainWindow() { Title = PluginName, DataContext = viewModel };
            var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };

            window.ShowDialog();
        }
    }
}