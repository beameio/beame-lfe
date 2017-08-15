using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class TrainingEnums
    {
        public enum eTrainingStatus
        {
             [Description("WAIT")] WAIT                 = 1
            ,[Description("IN_PROGRESS")] IN_PROGRESS   = 2
            ,[Description("END")] END                   = 3
            ,[Description("CANCEL")] CANCEL             = 4
            ,[Description("REGISTERED")] REGISTERED     = 5
            ,[Description("PARTICIPATED")] PARTICIPATED = 6
        }
    }
}
