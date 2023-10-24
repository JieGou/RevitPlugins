using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Diagram.Core.Layout;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.CustomParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        private readonly RevitRepository _revitRepository;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IFillAdskParamFactory _fillAdskParamFactory;

        private string _errorText;
        private LinkTypeViewModel _linkType;
        private ObservableCollection<LinkTypeViewModel> _linkTypes;
        private ObservableCollection<FillParamViewModel> _fillParams;

        public MainViewModel(RevitRepository revitRepository)
        {
            if(revitRepository is null)
            {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
            _viewModelFactory = viewModelFactory;
            _fillAdskParamFactory = fillAdskParamFactory;

            LoadViewCommand = new RelayCommand(LoadView);
            UpdateBuildPartCommand = new RelayCommand(UpdateBuildPart);
            UpdateElementsCommand = new RelayCommand(UpdateElements, CanUpdateElement);
        }

        public ICommand CheckRussianTextCommand { get; }
        public ICommand UpdateElementsCommand { get; set; }

        public void CheckRussianText(object args)
        {
            foreach(FillMassParamViewModel fillMassParamViewModel in FillParams.OfType<FillMassParamViewModel>())
            {
                fillMassParamViewModel.CheckRussianTextCommand.Execute(null);
                fillMassParamViewModel.UpdatePartParamNameCommand.Execute(null);
            }
        }

        public string ErrorText
        {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public LinkTypeViewModel LinkType
        {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; }
        public ObservableCollection<FillParamViewModel> FillParams { get; }

        private IEnumerable<FillParamViewModel> GetFillParams()
        {
            yield return new FillLevelParamViewModel(_revitRepository)
            {
                RevitParam = SharedParamsConfig.Instance.BuildingWorksLevel
            };

            yield return new FillMassParamViewModel(this, _revitRepository)
            {
                IsRequired = true,
                AdskParamName = RevitRepository.AdskBuildingNumberName,
                PartParamName = LinkInstanceRepository.BuildingWorksBlockName,
                RevitParam = SharedParamsConfig.Instance.BuildingWorksBlock
            };

            yield return new FillMassParamViewModel(this, _revitRepository)
            {
                AdskParamName = RevitRepository.AdskSectionNumberName,
                PartParamName = LinkInstanceRepository.BuildingWorksSectionName,
                RevitParam = SharedParamsConfig.Instance.BuildingWorksSection
            };

            yield return new FillMassParamViewModel(this, _revitRepository)
            {
                PartParamName = LinkInstanceRepository.BuildingWorksTypingName,
                RevitParam = SharedParamsConfig.Instance.BuildingWorksTyping
            };
        }

        private IEnumerable<LinkTypeViewModel> GetLinkTypes()
        {
            return _revitRepository.GetRevitLinkTypes()
                .Select(item =>
                    new LinkTypeViewModel(item, _revitRepository));
        }

        private void UpdateElements(object param)
        {
            SaveConfig();

            using(var transactionGroup =
                  _revitRepository.StartTransactionGroup("Установка уровня/секции"))
            {
                foreach(FillParamViewModel fillParamViewModel in FillParams.Where(item => item.IsEnabled))
                {
                    fillParamViewModel.UpdateElements();
                }

                transaction.Commit();
            }
        }

        private bool CanUpdateElement(object param)
        {
            if(LinkType == null)
            {
                ErrorText = "Выберите координационный файл.";
                return false;
            }

            if(LinkType != null && !LinkType.IsLoaded)
            {
                ErrorText = "Выбранная связь выгружена.";
                return false;
            }

            if(string.IsNullOrEmpty(LinkType.BuildPart))
            {
                ErrorText = "Выберите раздел.";
                return false;
            }

            if(!FillParams.Any(item => item.IsEnabled))
            {
                ErrorText = "Выберите хотя бы один параметр.";
                return false;
            }

            string errorText = FillParams
                .Select(item => item.GetErrorText())
                .FirstOrDefault(item => !string.IsNullOrEmpty(item));

            if(!string.IsNullOrEmpty(errorText))
            {
                ErrorText = errorText;
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void SetConfig()
        {
            SetLevelSectionSettings settings =
                SetLevelSectionConfig.GetPrintConfig()
                    .GetSettings(_revitRepository.Document);
            if(settings == null)
            {
                return;
            }

            LinkType = LinkTypes
                           .FirstOrDefault(item => item.Id == settings.LinkFileId)
                       ?? LinkTypes.FirstOrDefault();

            if(LinkType != null)
            {
                LinkType.BuildPart = LinkType?.BuildParts.Contains(settings.BuildPart) == true
                    ? settings.BuildPart
                    : null;
            }

            foreach(FillParamViewModel fillParam in FillParams)
            {
                ParamSettings paramSettings = settings.ParamSettings
                    .FirstOrDefault(item => item.ParamId.Equals(fillParam.RevitParam.Id));

                if(paramSettings != null)
                {
                    fillParam.SetParamSettings(paramSettings);
                }
            }
        }

        private void SaveConfig()
        {
            SetLevelSectionConfig config = SetLevelSectionConfig.GetPrintConfig();
            SetLevelSectionSettings settings = config.GetSettings(_revitRepository.Document);
            if(settings == null)
            {
                settings = config.AddSettings(_revitRepository.Document);
            }

            settings.BuildPart = LinkType.BuildPart;
            settings.ParamSettings.Clear();
            foreach(FillParamViewModel fillParam in FillParams)
            {
                ParamSettings paramSettings = fillParam.GetParamSettings();
                settings.ParamSettings.Add(paramSettings);
            }

            config.SaveProjectConfig();
        }
    }
}