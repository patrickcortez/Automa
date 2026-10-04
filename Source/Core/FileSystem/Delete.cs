using Automa.Source.Definitions;

namespace Automa.Source.Core.FileSystem
{

    // Automa delete f-sys operation
    internal record Delete(string source, string? target = null) : AssignType
    {

        public int Start()
        {
            try
            {
                if (!Path.Exists(source))
                {
                    throw new Exception($"Path: {source} does not exist");
                }


                bool isDirectory = Directory.Exists(source); // if target is a directory

                if (isDirectory) // recursively delete
                {
                    Directory.Delete(source, true);
                }
                else // otherwise delete file
                {
                    File.Delete(source);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }

        }
    }
}
