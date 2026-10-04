using Automa.Source.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core.FileSystem
{
    internal record MoveInstruction(string source,string destination,string? target = null) : AssignType
    {

        public int Start(){
            try{
                
                if(!Path.Exists(source)){
                    throw new Exception($"Source: {source} does not exist");
                }

                if (!Path.Exists(source))
                {
                    throw new Exception($"Source: {destination} does not exist");
                }

                if(File.Exists(destination)){
                    throw new Exception($"Can't move {source} to {destination}");
                }

                bool isDirectory=Directory.Exists(source);

                if (isDirectory)
                {
                    string dirname = Path.GetDirectoryName(source);
                    string newdest = Path.Combine(destination, dirname);
                    Directory.Move(source, newdest);
                }else{
                    string file = Path.GetFileName(source);
                    string newdest = Path.Combine(destination, file);
                    File.Move(source, newdest);
                }
                return 0;
            }
            catch(Exception ex){
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
        
    }
}
