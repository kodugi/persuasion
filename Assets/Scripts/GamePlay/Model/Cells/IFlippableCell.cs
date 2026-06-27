namespace GamePlay
{
    public interface IFlippableCell
    {
        public bool CanBeFlippedBy(Cell first, Cell second);
    }
}