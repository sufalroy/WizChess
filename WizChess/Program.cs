
namespace WizChess
{
    class Program
    {
        static void Main()
        {
            Game game = new Game(1280, 720);
            game.Run();
            game.Dispose();
        }
    }
}
