using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SEMC.CrashLink.BackgroudService.model;

namespace SEMC.CrashLink.BackgroudService.interfaces
{
    public interface IValidateFileName
    {
        bool validate(string fileName);
    }

}
