﻿using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Базовый класс полого экземпляра семейства
    /// </summary>
    internal abstract class OpeningRealBase : IOpeningReal {
        /// <summary>
        /// Экземпляр семейства чистового отверстия
        /// </summary>
        private protected readonly FamilyInstance _familyInstance;

        /// <summary>
        /// Закэшированный солид в координатах файла
        /// </summary>
        private protected Solid _solid;

        /// <summary>
        /// Закэшированный BBox
        /// </summary>
        private protected BoundingBoxXYZ _boundingBox;


        /// <summary>
        /// Базовый конструктор, устанавливающий <see cref="_familyInstance"/>, <see cref="_solid"/>, <see cref="_boundingBox"/>
        /// </summary>
        /// <param name="openingReal">Экземпляр семейства проема в стене или перекрытии</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр является null</exception>
        /// <exception cref="ArgumentException">Исключение, если экземпляр семейства не имеет хоста</exception>
        protected OpeningRealBase(FamilyInstance openingReal) {
            if(openingReal is null) { throw new ArgumentNullException(nameof(openingReal)); }
            if(openingReal.Host is null) { throw new ArgumentException($"{nameof(openingReal)} с Id {openingReal.Id} не содержит ссылки на хост элемент"); }
            _familyInstance = openingReal;

            SetTransformedBBoxXYZ();
            SetSolid();
        }


        public abstract Solid GetSolid();

        public abstract BoundingBoxXYZ GetTransformedBBoxXYZ();


        /// <summary>
        /// Возвращает хост экземпляра семейства отверстия
        /// </summary>
        /// <returns></returns>
        public Element GetHost() {
            return _familyInstance.Host;
        }

        public FamilyInstance GetFamilyInstance() {
            return _familyInstance;
        }


        /// <summary>
        /// Возвращает значение параметра, или пустую строку, если параметра у семейства нет. Значения параметров с типом данных "длина" конвертируются в мм и округляются до 1 мм.
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private protected string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
            if(_familyInstance is null) {
                throw new ArgumentNullException(nameof(_familyInstance));
            }
            string value = string.Empty;
            if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
#if REVIT_2022_OR_GREATER
                if(_familyInstance.GetSharedParam(paramName).Definition.GetDataType() == SpecTypeId.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(GetFamilyInstanceDoubleParamValueOrZero(paramName), UnitTypeId.Millimeters)).ToString();
                }
#elif REVIT_2021
                if(_familyInstance.GetSharedParam(paramName).Definition.ParameterType == ParameterType.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(GetFamilyInstanceDoubleParamValueOrZero(paramName), UnitTypeId.Millimeters)).ToString();
                }
#else
                if(_familyInstance.GetSharedParam(paramName).Definition.UnitType == UnitType.UT_Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(GetFamilyInstanceDoubleParamValueOrZero(paramName), DisplayUnitType.DUT_MILLIMETERS)).ToString();
                }
#endif
                object paramValue = _familyInstance.GetParamValue(paramName);
                if(!(paramValue is null)) {
                    if(paramValue is double doubleValue) {
                        value = Math.Round(doubleValue).ToString();
                    } else {
                        value = paramValue.ToString();
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Возвращает значение double параметра экземпляра семейства задания на отверстие в единицах ревита, или 0, если параметр отсутствует
        /// </summary>
        /// <param name="paramName">Название параметра</param>
        /// <returns></returns>
        private protected double GetFamilyInstanceDoubleParamValueOrZero(string paramName) {
            if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
                return _familyInstance.GetSharedParamValue<double>(paramName);
            } else {
                return 0;
            }
        }


        private Solid CreateRawSolid() {
            return GetTransformedBBoxXYZ().CreateSolid();
        }

        /// <summary>
        /// Устанавливает значение полю <see cref="_boundingBox"/>
        /// </summary>
        private void SetTransformedBBoxXYZ() {
            _boundingBox = _familyInstance.GetBoundingBox();
        }

        /// <summary>
        /// Устанавливает значение полю <see cref="_solid"/>
        /// </summary>
        private void SetSolid() {
            XYZ openingLocation = (_familyInstance.Location as LocationPoint).Point;
            var hostElement = GetHost();
            Solid hostSolidCut = hostElement.GetSolid();
            try {
                Solid hostSolidOriginal = (hostElement as HostObject).GetHostElementOriginalSolid();
                var openings = SolidUtils.SplitVolumes(BooleanOperationsUtils.ExecuteBooleanOperation(hostSolidOriginal, hostSolidCut, BooleanOperationsType.Difference));
                var thisOpeningSolid = openings.OrderBy(solidOpening => (solidOpening.ComputeCentroid() - openingLocation).GetLength()).FirstOrDefault();
                if(thisOpeningSolid != null) {
                    _solid = thisOpeningSolid;
                } else {
                    _solid = CreateRawSolid();
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                _solid = CreateRawSolid();
            }
        }
    }
}
