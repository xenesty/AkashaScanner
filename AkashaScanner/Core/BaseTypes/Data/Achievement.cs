namespace AkashaScanner.Core
{
    public sealed record Achievement
    {
        public int Id;
        public int CategoryId;

        public bool IsValid() => true;
    }
}
