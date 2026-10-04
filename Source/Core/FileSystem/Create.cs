using Automa.Source.Definitions;

namespace Automa.Source.Core.FileSystem
{

// Automa create f-sys opeartion
    internal record Create(string path,string? target = null) : AssignType
    {

       public int Start(){

            try
            {

                string? parent = Directory.GetParent(path).FullName; // ensure parent dir exists before creating

                if (!Path.Exists(parent) || parent is null || !Path.IsPathRooted(path)) // might move isRooted soon
                { // Error if not rooted, doesn't exist or parent string is null
                    throw new Exception("Parent directory doesnt exist or Path is not rooted");
                }

                if (Path.HasExtension(path))
                {
                    File.Create(path).Close();
                }
                else
                {
                    Directory.CreateDirectory(path);
                }

                return 0;
            }catch(Exception ex) {

                Console.Error.WriteLine(ex);
                return 1;
            }

       }

    }
}
