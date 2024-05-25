namespace MikManager.Util 
{
    public class Scanner(TextReader inputReader)
    {
        private readonly TextReader _inputReader = inputReader;

        public int NextInt()
        {
            while (true)
            {
                string? input = _inputReader.ReadLine();
                if (int.TryParse(input, out int result))
                    return result;
                else
                    Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }

        public string? NextLine()
        {
            return _inputReader.ReadLine();
        }
    }
}