﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Shell;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyFileViewModel : BaseViewModel {
        private FileInfo _familyFile;
        private BitmapSource _image;

        public FamilyFileViewModel(FileInfo familyFile) {
            _familyFile = familyFile;
            //Image = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;
        }

        public string Name {
            get { return _familyFile.Name; }
        }

        public BitmapSource Image {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }

        public void Refresh(FileInfo newFileInfo) {
            _familyFile = newFileInfo;
            //Image = ShellFile.FromFilePath(_familyFile.FullName).Thumbnail.BitmapSource;
            
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(Image));
        }
    }
}