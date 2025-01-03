public class ConsoleCommand {
    public string main;
    public string desc;
    
    public delegate void Action(string[] args, Player player);
    public Action moment;
}
