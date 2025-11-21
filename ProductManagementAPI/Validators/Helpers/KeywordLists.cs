namespace ProductManagementAPI.Validators.Helpers
{
    public static class KeywordLists
    {
        public static readonly List<string> InappropriateWords = new() { "badword1", "badword2" };
        public static readonly List<string> TechnologyKeywords = new() { "computer", "laptop", "smartphone", "camera" };
        public static readonly List<string> HomeInappropriateWords = new() { "forbiddenhome1", "forbiddenhome2" };
    }
}