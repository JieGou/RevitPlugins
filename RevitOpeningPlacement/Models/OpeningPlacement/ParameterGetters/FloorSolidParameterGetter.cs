﻿using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class FloorSolidParameterGetter : IParametersGetter {
        private readonly ISolidProvider _solidProvider;
        private readonly IPointFinder _pointFinder;
        private readonly MepCategory[] _mepCategories;
        private readonly Element _mepElement;
        private readonly Element _structureElement;

        public FloorSolidParameterGetter(
            ISolidProvider solidProvider,
            IPointFinder pointFinder,
            Element mepElement,
            Element structureElement,
            params MepCategory[] mepCategories) {

            _solidProvider = solidProvider;
            _mepCategories = mepCategories;
            _pointFinder = pointFinder;
            _structureElement = structureElement;
            _mepElement = mepElement;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            //габариты отверстия
            var sizeInit = new FloorOpeningSizeInitializer(_solidProvider.GetSolid(), _mepCategories);
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();

            //отметки отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetOfOpeningInFloorValueGetter(_pointFinder, sizeInit.GetThickness())).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_mepElement, _structureElement)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_mepElement)).GetParamValue();
        }
    }
}
