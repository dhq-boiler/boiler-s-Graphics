namespace boilersGraphics.Models
{
    public class InOutPair
    {
        public InOutPair(int @in, int @out)
        {
            In = @in;
            Out = @out;
        }

        public int In { get; set; }
        public int Out { get; set; }

        public override string ToString()
        {
            return $"{In} => {Out}";
        }
    }
}
