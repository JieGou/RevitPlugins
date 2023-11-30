﻿using System.Collections.Generic;
using System.Threading.Tasks;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services {
    internal interface IModelObjectService {
        Task<ModelObject> SelectModelObjectDialog();
        Task<ModelObject> GetFromString(string folderName);
    }
}
