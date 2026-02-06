namespace shared
{
    public class Rect
    {
        public int X, Y;
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public Rect()
        {
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
            X = 0;
            Y = 0;
        }
        public Rect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            X = 0;
            Y = 0;
        }
    }
}