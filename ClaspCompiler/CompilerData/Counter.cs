namespace ClaspCompiler.CompilerData
{
    internal sealed class Counter
    {
        private int _count;
        public Counter(int start = 0) => _count = start;
        public int GetValue() => _count++;
    }
}
