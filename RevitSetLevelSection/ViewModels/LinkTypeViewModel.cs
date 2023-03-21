﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Factories;
using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class LinkTypeViewModel : BaseViewModel {
        private readonly RevitLinkType _revitLinkType;
        private readonly RevitRepository _revitRepository;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IDesignOptionFactory _designOptionFactory;
        private readonly LinkInstanceRepository _linkInstanceRepository;
        
        private bool _isLoaded;
        private string _buildPart;
        
        private ObservableCollection<string> _buildParts;
        private ObservableCollection<DesignOptionsViewModel> _designOptions;

        public LinkTypeViewModel(RevitLinkType revitLinkType, 
            RevitRepository revitRepository, 
            IViewModelFactory viewModelFactory, 
            IDesignOptionFactory designOptionFactory) {
            _revitLinkType = revitLinkType;
            _revitRepository = revitRepository;
            _viewModelFactory = viewModelFactory;
            _designOptionFactory = designOptionFactory;

            _linkInstanceRepository =
                new LinkInstanceRepository(_revitRepository, _revitLinkType);

            IsLoaded = _linkInstanceRepository.LinkIsLoaded();
            
            BuildParts = new ObservableCollection<string>(GetPartNames());
            BuildPart = BuildParts.FirstOrDefault();
            
            LoadLinkDocumentCommand = new RelayCommand(LoadLinkDocument, CanLoadLinkDocument);
            DesignOptions = IsLoaded
                ? new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions())
                : new ObservableCollection<DesignOptionsViewModel>();
        }

        public int Id => _revitLinkType.Id.IntegerValue;
        public string Name => _revitLinkType.Name;
        public ICommand LoadLinkDocumentCommand { get; }

        public bool IsLoaded {
            get => _isLoaded;
            set => this.RaiseAndSetIfChanged(ref _isLoaded, value);
        }
        
        public string BuildPart {
            get => _buildPart;
            set => this.RaiseAndSetIfChanged(ref _buildPart, value);
        }
        
        public bool HasAreas => _linkInstanceRepository.GetAreas().Any();
        public bool HasAreaScheme => _linkInstanceRepository.GetAreaScheme() != null;

        public ObservableCollection<string> BuildParts {
            get => _buildParts;
            set => this.RaiseAndSetIfChanged(ref _buildParts, value);
        }

        public ObservableCollection<DesignOptionsViewModel> DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }

        private IEnumerable<DesignOptionsViewModel> GetDesignOptions() {
            if(!_linkInstanceRepository.LinkIsLoaded()) {
                yield break;
            }

            yield return _viewModelFactory.Create(_designOptionFactory.Create(), _linkInstanceRepository);
            foreach(DesignOption designOption in _linkInstanceRepository.GetDesignOptions()) {
                yield return _viewModelFactory.Create(_designOptionFactory.Create(designOption), _linkInstanceRepository);
            }
        }

        public IEnumerable<string> GetPartNames() {
            return _linkInstanceRepository.GetPartNames().Distinct();
        }
        
        public IEnumerable<string> GetPartNames(IEnumerable<string> paramNames) {
            return _linkInstanceRepository.GetPartNames(paramNames).Distinct();
        }

        public IAreaRepository GetAreaRepository() {
            return _linkInstanceRepository;
        }

        private void LoadLinkDocument(object param) {
            IsLoaded = _linkInstanceRepository.LoadLinkDocument();
            DesignOptions = new ObservableCollection<DesignOptionsViewModel>(GetDesignOptions());
            
            BuildParts = new ObservableCollection<string>(GetPartNames());
            BuildPart = BuildParts.FirstOrDefault();
        }

        private bool CanLoadLinkDocument(object param) {
            return !IsLoaded;
        }
    }
}