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


    public  async Task ProgressBar(Task task, int c = 10)
    {
        while (!(new[] { TaskStatus.Canceled, TaskStatus.RanToCompletion, TaskStatus.Faulted }).Contains(task.Status))
        {
            ProgressBar(c: c);
            await Task.Delay(100);
        }
        ClearProgressBar(c);
    }

    int indeterminate;
    public void ProgressBar(double? progress = null, int c = 10)
    {
        var chars = new char[c + 2];
        chars[0] = '[';
        chars[c + 1] = ']';
        for (int i = 0; i < c; i++)
        {
            if (progress == null)
            {
                chars[i + 1] = indeterminate % c == i ? '.' : ' ';
            }
            else
            {
                chars[i + 1] = progress > i / (double)c ? '0' : '-';
            }
        }
        indeterminate++;
        Write(new string(chars), true);
        Jump(column: -(c + 2));
    }

    public void ClearProgressBar(int c = 10)
    {
        Write(new string(' ', c + 2), true);
        Jump(column: -(c + 2));
    }
}
