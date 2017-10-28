using System.IO;

public class LineReader
{
    protected FileInfo theSourceFile = null;
    protected StreamReader reader = null;
    public string text = " "; // assigned to allow first line to be read below
    public string[] bits;

    public void AssignFile(string fileName)
    {
        theSourceFile = new FileInfo(fileName);
        reader = theSourceFile.OpenText();
    }

    public void ReadLine()
    {
        if (text != null)
        {
            text = reader.ReadLine();
        }
    }

    public void SplitBits()
    {
        bits = text.Split(' ');
    }

	public void CloseFile()
	{
		reader.Close ();
	}
}
