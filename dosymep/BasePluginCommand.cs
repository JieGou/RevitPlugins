using System;
using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace dosymep.Bim4Everyone {
    public abstract class BasePluginCommand : IExternalCommand {
        public BasePluginCommand() {
            PluginName = GetType().Name;
        }

        public bool FromGui { get; set; } = true;

        /// <summary>
        /// Provides access to the extension logger.
        /// </summary>
        protected ILoggerService PluginLoggerService { get; private set; }

        /// <summary>
        /// Provides access to the platform logger.
        /// </summary>
        protected ILoggerService LoggerService => GetPlatformService<ILoggerService>();

        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            FromGui = GetFromGui(commandData.JournalData);
            
            PluginLoggerService = LoggerService.ForPluginContext(PluginName);
            PluginLoggerService.Information("Running the extension command.");

            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Execute(commandData.Application);
                PluginLoggerService.Information("Выход из команды расширения.");
            } catch(OperationCanceledException) {
                PluginLoggerService.Warning("Отмена выполнения команды расширения.");

                if(!FromGui) {
                    return Result.Cancelled;
                }
                
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                PluginLoggerService.Warning("Отмена выполнения команды расширения.");
                
                if(!FromGui) {
                    return Result.Cancelled;
                }
                
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            } catch(Exception ex) {
                PluginLoggerService.Warning(ex, "Error in extension command.");
                
                if(!FromGui) {
                    message = ex.Message;
                    return Result.Failed;
                }
                
#if DEBUG
                TaskDialog.Show(PluginName, ex.ToString());
#else
                TaskDialog.Show(PluginName, ex.Message);
#endif
                GetPlatformService<INotificationService>()
                    .CreateFatalNotification(PluginName, "Выполнение скрипта завершено с ошибкой.")
                    .ShowAsync();
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }

        private bool GetFromGui(IDictionary<string, string> journalData) {
            if(journalData.TryGetValue(PlatformCommandIds.ExecutedFromUI, out string fromGui)) {
                return bool.Parse(fromGui);
            }

            return true;
        }

        /// <summary>
        /// Наименование расширения для логгера
        /// </summary>
        protected string PluginName { get; set; }

        /// <summary>
        /// Revit command method
        /// </summary>
        /// <param name="uiApplication">Приложение Revit.</param>
        protected abstract void Execute(UIApplication uiApplication);

        protected void Notification(Window window) {
            Notification(window.ShowDialog());
        }
        
        protected void Notification(bool? dialogResult) {
            if(dialogResult == null) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выход из скрипта.", "C#")
                    .ShowAsync();
            } else if(dialogResult == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else if(dialogResult == false) {
                throw new OperationCanceledException();
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}