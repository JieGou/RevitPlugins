﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSuperfilter.ViewModels;
using RevitSuperfilter.Views;

namespace RevitSuperfilter {
    [Transaction(TransactionMode.Manual)]
    public class SuperfilterCommand : BasePluginCommand {
        public SuperfilterCommand() {
            PluginName = "Superfilter";
        }

        protected override void Execute(UIApplication uiApplication) {
            var hasSelectedElements = uiApplication.ActiveUIDocument.GetSelectedElements().Any();
            var viewModel = new SuperfilterViewModel(uiApplication.Application, 
                uiApplication.ActiveUIDocument.Document, hasSelectedElements);

            var window = new MainWindow() { DataContext = viewModel };
            if(window.ShowDialog() == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }
    }
}
