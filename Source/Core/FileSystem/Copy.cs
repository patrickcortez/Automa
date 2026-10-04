using Automa.Source.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core.FileSystem
{
// Automa Copy Handler
    internal record CopyInstruction(string source,string destination,string? target = null) : AssignType
    {

        private int CopyRecurse(string src,string dst){ // recursively copy a dir, if it is
            
            try{

                if(!Path.Exists(dst)){  // create file/folder if doesn't exist         
                    if(Path.HasExtension(dst)){ // will change this soon..
                        File.Create(dst).Close();
                    }else{
                        Directory.CreateDirectory(dst);
                    }
                }


                if(!Path.Exists(src)){
                    throw new Exception($"Source: {src} doesn't exist!");
                }

                bool recurse = Directory.Exists(src);

                if(!recurse){ // if the source isnt a directory, dont recurse.

                    if(File.Exists(src)){
                        string file = Path.GetFileName(src);
                        string newdest = Path.Combine(destination, file);
                        File.Copy(src, newdest, true);
                        return 0;    
                    }

                    throw new FileNotFoundException($"File {src} does not exist!");
                }


                DirectoryInfo entry = new(src);


                foreach(var dir in entry.GetDirectories()){ // ensure to copy dirs first
                    string newDir = Path.Combine(dst, dir.Name);
                    CopyRecurse(dir.FullName, newDir);
                }


                foreach(var files in entry.GetFiles()){ // then file
                    string newFile = Path.Combine(dst, files.Name);
                    File.Copy(files.FullName, newFile, true);
                }

                return 0;


            }catch(Exception ex){
                Console.Error.WriteLine(ex);
                return 1;
            }

        }
       

        public int Start(){
            try{

                return CopyRecurse(source,destination);
            }catch(Exception ex){
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

    }
}
