namespace weiduize.Model
{
    public class TBG3_TextHeader
    {
        public int Offset { get; set; } // offset into this TBG file to the text
        public int Length { get; set; } // length of the text entry in this TBG file
        public int StrrefWriteLocation { get; set; } // offset into the output file to write this text
        public char[] SoundFilename { get; set; } // 8
    }
}