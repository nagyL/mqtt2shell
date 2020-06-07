namespace Mqtt2Shell
{
    public interface IShellAdapter
    {
        void Execute(string commandAndArguments);
    }
}