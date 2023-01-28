using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class KugelblitzLogger
{
    private KugelblitzScript _module;
    private string _line;

    public KugelblitzLogger(KugelblitzScript module)
    {
        _module = module;
        _line = "";
    }

    public void AppendFormat(string text, params object[] args)
    {
        _line += string.Format(text, args);
    }

    public void WriteLine()
    {
        _module.Log(_line);
        _line = "";
    }

    public void WriteFormat(string text, params object[] args)
    {
        _line = string.Format(text, args);
        WriteLine();
    }
}