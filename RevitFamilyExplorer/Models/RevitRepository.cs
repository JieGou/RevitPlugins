﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitFamilyExplorer.Models {
    internal class RevitRepository {
        private readonly UIApplication _uiApplication;

        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public Application Application {
            get { return _uiApplication.Application; }
        }

        public Document Document {
            get { return _uiApplication.ActiveUIDocument.Document; }
        }

        internal IEnumerable<FamilyType> GetFamilyTypes(FileInfo familyFile) {
            var familyDocument = Application.OpenDocumentFile(familyFile.FullName);
            try {
                if(!familyDocument.IsFamilyDocument) {
                    throw new ArgumentException($"Переданный файл не является документом семейства. {familyFile}");
                }

                return familyDocument.FamilyManager.Types.Cast<FamilyType>();
            } finally {
                //familyDocument.Close(false);
            }
        }
    }
}
