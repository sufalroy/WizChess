namespace WizChess 
{
    class Program
    {
        //[STAThread]
        static void Main()
        {
            Game game = new Game(1280, 720);
            game.Run();
            game.Dispose();
        }
    }
}
 