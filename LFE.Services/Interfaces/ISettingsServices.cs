using System;
using System.Collections.Generic;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface ISettingsServices :IDisposable
    {
        #region wizard steps
        List<WizardStepTooltipDTO> GetSteps();
        bool SaveStepTooltip(WizardStepTooltipDTO token, out string error);

        #endregion
    }
}
