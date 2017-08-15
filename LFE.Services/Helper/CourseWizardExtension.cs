

using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Helper
{
    public static class CourseWizardExtension
    {
        public static BreadcrumbStepDTO SetBreadcrumbItemClass(this BreadcrumbStepDTO token)
        {
            var dto = token;
            dto.SpanCssClass = WizardNavigationStepClass(token.Mode);
            return dto;
        }

        public static string WizardStep2NextButtonText(this CourseEnums.eWizardSteps step)
        {
            switch (step)
            {
                //case CourseEnums.eWizardSteps.Introduction:
                case CourseEnums.eWizardSteps.VideoManager:
               // case CourseEnums.eWizardSteps.ChapterManage:
                case CourseEnums.eWizardSteps.ChapterContents:
                    return "next >";
                case CourseEnums.eWizardSteps.Publish:
                    return "finish";
                default:
                    return "save & next >";
            }
        }

        public static bool WizardStep2NextButtonState(this CourseEnums.eWizardSteps step)
        {
            switch (step)
            {
                //case CourseEnums.eWizardSteps.Introduction:
                case CourseEnums.eWizardSteps.VideoManager:
               // case CourseEnums.eWizardSteps.ChapterManage:
                case CourseEnums.eWizardSteps.ChapterContents:
                    return false;
                default:
                    return true;
            }
        }

        private static string WizardNavigationStepClass(CourseEnums.eWizardSetpModes mode)
        {
            switch (mode)
            {
                case CourseEnums.eWizardSetpModes.Dummy:
                    return "group";
                case CourseEnums.eWizardSetpModes.Disable:
                    return "disabled";
                case CourseEnums.eWizardSetpModes.Current:
                    return "current";
                case CourseEnums.eWizardSetpModes.Allowed:
                    return "allowed";
                default:
                    return string.Empty;
            }
        }

    }
}
