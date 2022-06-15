﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal class DocInfo {
        public DocInfo(string name, Document doc, Transform transform) {
            Name = name;
            Doc = doc;
            Transform = transform;
        }
        public Document Doc { get; }
        public Transform Transform { get; }
        public string Name { get; }
    }
}
