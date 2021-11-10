﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class LevelViewModel : ElementViewModel<Level> {
        private bool _isSelected;

        public LevelViewModel(Level level, RevitRepository revitRepository, IEnumerable<SpatialElement> spatialElements) : base(level, revitRepository) {
            SpartialElements = new ObservableCollection<SpatialElementViewModel>(GetSpatialElements(revitRepository, spatialElements));
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
#if D2020 || R2020
        public bool IsLivingLevel {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, DisplayUnitType.DUT_METERS) > 2; }
        }

        public string Elevation {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, DisplayUnitType.DUT_METERS) + " " + UnitUtils.GetTypeCatalogString(DisplayUnitType.DUT_METERS); }
        }
#elif D2021 || R2021 || D2022 || R2022
        public bool IsLivingLevel {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, UnitTypeId.Meters) > 2; }
        }

        public string Elevation {
            get { return UnitUtils.ConvertFromInternalUnits(Element.Elevation, UnitTypeId.Meters) + " " + UnitUtils.GetTypeCatalogStringForUnit(UnitTypeId.Meters); }
        }
#endif

        public int RoomsCount {
            get { return SpartialElements.Count; }
        }

        public override string LevelName {
            get { return Name; }
        }

        public ObservableCollection<SpatialElementViewModel> SpartialElements { get; }

        public IEnumerable<DoorViewModel> GetDoors(IEnumerable<PhaseViewModel> phases) {
            return RevitRepository.GetDoors()
                .SelectMany(item => phases.Select(phase => new DoorViewModel(item, phase, RevitRepository)))
                .Where(item => item.LevelId == Element.Id)
                .Where(item => SpartialElements.Contains(item.ToRoom) || SpartialElements.Contains(item.FromRoom));
        }

        public IEnumerable<SpatialElementViewModel> GetSpatialElementViewModels(PhaseViewModel phase) {
            return SpartialElements.Where(item => item.Phase == phase);
        }

        public IEnumerable<SpatialElementViewModel> GetSpatialElementViewModels(IEnumerable<PhaseViewModel> phases) {
            return SpartialElements.Where(item => phases.Contains(item.Phase));
        }

        private static IEnumerable<SpatialElementViewModel> GetSpatialElements(RevitRepository revitRepository, IEnumerable<SpatialElement> spatialElements) {
            return spatialElements.Select(item => new SpatialElementViewModel(item, revitRepository)).Where(item => item.Phase != null).Where(item => item.IsPlaced);
        }
    }
}
