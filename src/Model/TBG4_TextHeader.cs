namespace weiduize.Model
{
    public class TBG4_TextHeader
    {
        public int Offset { get; set; }
        public int Length { get; set; }
        public char[] SoundFilename { get; set; } // 8
        public int VolumeVariance { get; set; }
        public int PitchVariance { get; set; }
        public short EntryType { get; set; }
        public int StrrefOffset { get; set; } // We can use the same TextHeader in multiple locations in the file (called strrefs here)
        public int StrrefCount { get; set; }  //
        public int Offset2 { get; set; } //?
        public int Count2 { get; set; } //?
        public int Offset3 { get; set; } //?
        public int Count3 { get; set; } //?
    }
}