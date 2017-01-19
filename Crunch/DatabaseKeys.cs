namespace Crunch
{
    public static class DatabaseKeys {
        #region DB Key Constants
        public const string ExperimentKeyFormat = "e:{0}";
        public const string ExperimentCurrentCountKeyFormat = "e:{0}:_all";
        public const string ExperimentVariantVersionKeyFormat = "e:{0}:{1}:v{2}";
        public const string ExperimentGoalCountKeyFormat = "e:{0}:{1}:v:{2}:g:{3}";
        public const string ParticipantKeyFormat = "p:{0}";
        public const string ParticipantGoalsFormat = "p:{0}:_goals";
        #endregion
    }
}
