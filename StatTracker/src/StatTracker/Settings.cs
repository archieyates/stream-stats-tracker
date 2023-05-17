namespace StatTracker
{
    internal class Settings
    {
        public bool AutoGenerateLookup { get; set; } = false;
        public bool UseTimeStamps { get; set; } = false;

        public object GetPropertyValue(string PropName) { return GetType().GetProperty(PropName)?.GetValue(this, null); }
        public void SetPropertyValue(string PropName, Object PropValue) { GetType().GetProperty(PropName)?.SetValue(this, PropValue, null); }
    }
}
