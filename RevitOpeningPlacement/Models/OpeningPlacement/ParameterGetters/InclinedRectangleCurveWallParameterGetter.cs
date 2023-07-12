﻿using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class InclinedRectangleCurveWallParameterGetter : IParametersGetter {
        private readonly MepCurveClash<Wall> _clash;
        private readonly MepCategory _mepCategory;
        private readonly IPointFinder _pointFinder;

        public InclinedRectangleCurveWallParameterGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder) {
            _clash = clash;
            _mepCategory = mepCategory;
            _pointFinder = pointFinder;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            var heightValueGetter = new InclinedSizeInitializer(_clash, _mepCategory).GetRectangleMepHeightGetter();
            //габариты отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, heightValueGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new InclinedSizeInitializer(_clash, _mepCategory).GetRectangleMepWidthGetter()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash)).GetParamValue();

            //отметки отверстия
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetOfRectangleOpeningInWallValueGetter(_pointFinder, heightValueGetter)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetOfRectangleOpeningInWallValueGetter(_pointFinder)).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
        }
    }
}
