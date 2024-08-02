namespace RevitDeclarations.Models {
    internal class ParameterToolTip {
        public string ParamDocumentToolTip => "Проект на основе которого выводится список параметров для выбора";
        public string LastConfigToolTip => "Подгрузка параметров, которые использовались\n" +
            "при последнем запуске скрипта в этом проекте";
        public string CompanyConfigToolTip => "Подгрузка параметров А101";

        public string FilterRoomsParamToolTip => "Текстовый параметр.\nИспользуется" +
            "для фильтрации помещений, относящихся к квартирам.";
        public string FilterRoomsValueToolTip => "Значение для фильтрации помещений квартир по параметру выше.\n" +
            "В выборке стаются тоьлко те помещения, которые СОДЕРЖАТ указанный текст в параметре.";
        public string GroupBySectionParamToolTip => "Параметр, по которому помещения группируются " +
            "по секциям.\nЕсли помещения имеют одинаковое значение этого параметра, " +
            "то считается,\nчто они относятся к одной секции.";
        public string GroupByGroupParamToolTip => "Параметр, по которому помещения группируются " +
            "по квартирам.\nЕсли помещения на одном этаже имеют одинаковое значение этого параметра и находятся " +
            "в одной секции,\nто считается, что они относятся к одной квартире.";
        public string MultiStoreyParamToolTip => "Параметр, по которому определяются двухуровневые квартиры.\n" +
            "Все помещения, имеющие одинаковое значения этого параметра, будут считаться одной квартирой,\n" +
            "даже если они находятся на разных уровнях." +
            "Для одноуровневых квартир параметр можно не заполнять.";

        public string FullApartNumParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Сквозной номер квартиры\"";
        public string DepartmentParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Назначение\"";
        public string LevelParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Этаж расположения\"";
        public string SectionParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер подъезда\"";
        public string BuildingParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер корпуса\"";
        public string ApartNumParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Номер на площадке\"";
        public string ApartAreaParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Общая площадь без пониж. коэффициента\"";
        public string ApartAreaCoefParamToolTip => "Текстовый параметр, значение которого выводится\n" +
            "в столбец декларации \"Общая площадь с пониж. коэффициентом\"";
        public string ApartAreaLivingParamToolTip => "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Жилая площадь\"";
        public string RoomsAmountParamToolTip => "Целочисленный параметр, значение которого выводится\n" +
            "в столбец декларации \"Количество комнат\"";
        public string ProjectNameToolTip => "Значение, которое выводится в столбец декларации \"ИД Объекта\"";
        public string ApartAreaNonSumParamToolTip => "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Площадь квартиры без летних помещений\"";
        public string RoomsHeightParamToolTip => "Числовой параметр, значение которого выводится\n" +
            "в столбец декларации \"Высота потолка\"";

        public string RoomAreaParamToolTip => "Числовой параметр, значение которого выводится\n" +
            "в столбец соответствующего не летнего помещения квартиры";
        public string RoomAreaCoefParamToolTip => "Числовой параметр, значение которого выводится\n" +
            "в столбец соответствующего летнего помещения квартиры";
    }
}
