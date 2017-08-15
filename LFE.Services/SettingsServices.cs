using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Application.Services.Base;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;

namespace LFE.Application.Services
{
    public class SettingsServices : ServiceBase, ISettingsServices
    {
       
        #region interface implementation
        public List<WizardStepTooltipDTO> GetSteps()
        {
            var list = WizardStepsRepository.GetAll().Select(x => x.Entity2StepTooltipDto()).OrderBy(x => x.StepId).ToList();

            return list;
        }

        public bool SaveStepTooltip(WizardStepTooltipDTO token,out string error)
        {
            error = string.Empty;
            try
            {
                var entity = WizardStepsRepository.GetById(token.StepId);

                if (entity == null)
                {
                    error = "entity not found";
                    return false;
                }

                entity.TooltipHTML = token.TooltipHtml;
                entity.UpdatedOn = DateTime.Now;

                WizardStepsRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save step tooltip",ex,token.StepId,CommonEnums.LoggerObjectTypes.AdminSettings);
                return false;
            }
        } 
        #endregion

    }
}
