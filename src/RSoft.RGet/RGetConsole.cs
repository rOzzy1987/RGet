namespace RSoft.RGet;

public class RGetConsole
{
    readonly RGetContext _context;

    public RGetConsole(RGetContext context)
    {
        _context = context;
    }

    public void LogTime()
    {
        if(_context.LogFile != null)
            File.AppendAllText(_context.LogFile.FullName, $"[{DateTime.Now.ToString("g")}] ");
    }

    public void WriteLine(bool consoleOnly = false)
    {
        Write(Environment.NewLine, consoleOnly);
    }

    public void WriteLine(object msg, bool consoleOnly = false)
    {
        Write(msg, consoleOnly);
        WriteLine(consoleOnly);
    }

    public void Write(object msg, bool consoleOnly = false)
    {
        if (_context.LogFile != null && !consoleOnly)
            File.AppendAllText(_context.LogFile.FullName, msg.ToString());
        if (_context.Quiet)
            return;
        Console.Write(msg);
    }

    public void Jump(int row = 0, int column = 0)
    {
        if (_context.Quiet) return;
        Console.CursorLeft += column;
        Console.CursorTop += row;
    }
}
