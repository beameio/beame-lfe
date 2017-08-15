using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class QuizEnums
    {
        public enum eQuizStatuses
        {
            [Description("Draft")] DRAFT          = 1
            ,[Description("Published")] PUBLISHED = 2
        }

        public enum eUserQuizAviability
        {
            [Description("Available")] Available,
            [Description("Not available")] NotAvailable
        }

        public enum eUserQuizAviabilityReasons
        {
            [Description("Await")] Waiting,
            [Description("Success")] Passed,
            [Description("Failed")] Failed,
            [Description("Max Attempts Exceed")] MaxAttemptsExceed
        }

        public enum eScoreRules
        {
            [Description("Equal score to all questions")] EQUAL = 1
            ,[Description("Score per question")] SCORE_PER_QUESTION = 2
        }

        public enum eQuizValidationRules
        {
            [Description("Question Exists")] ANY_QUEST        = 1,
            [Description("Question Score Valid")] SCORE_VALID = 2,
            [Description("Answers Exists")] ANSWERS_VALID      = 3,
            [Description("Active Questions")] ANY_ACTIVE = 4,
            [Description("Correct Answer Exists")] ANSWERS_CORRECT_VALID = 5
        }

        [Flags]
        public enum eQuizQuestionTypes
        {
             [Description("American")] American               = 1
            ,[Description("Yes/No")] YesNo                   = 2
            ,[Description("Multiple Choice")] MultipleChoice = 4
            ,[Description("Single Choice")] SingleChoice     = 8
        }

        public enum eUserQuizStatuses
        {
             [Description("In Progress")] IN_PROGRESS = 1
            ,[Description("Completed")] COMPLETED = 2
            ,[Description("Unfinished")] UNFINISHED = 3
        }       
    }
}
