namespace Source
{
    public static class Kernel
    {
        public static void Main()
        {
            Console.WriteLine("Hello, World!");
            SetPixel();
        }

        // We could do something like this for custom methods
        public static extern void SetPixel();
    }
}