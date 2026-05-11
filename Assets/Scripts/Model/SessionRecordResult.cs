namespace Model
{
    public struct SessionRecordResult
    {
        public bool IsNewGoldRecord { get; }
        public bool IsNewKillsRecord { get; }

        public SessionRecordResult(bool isNewGoldRecord, bool isNewKillsRecord)
        {
            IsNewGoldRecord = isNewGoldRecord;
            IsNewKillsRecord = isNewKillsRecord;
        }
    }
}
