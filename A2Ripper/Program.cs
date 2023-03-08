using System;
using System.IO;
using System.Text;

namespace A2Ripper
{
    class Program
    {
        private static readonly int CHUNK_SIZE = 24;
        static void Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("Correct usage: a2ripper \"path to .ind file\" \"path to .img file\" \"output folder\" ");
                Console.WriteLine("Example: a2ripper \"C:\\Program Files (x86)\\Davilex\\A2 Racer III\\Tour 2\\rcs.ind\" \"C:\\Program Files (x86)\\Davilex\\A2 Racer III\\Tour 2\\rcs.img\" \"C:\\a2ripperoutput\" ");
                return;
            }

            string indFile = args[0];
            string imgFile = args[1];
            string outputDir = args[2];
            using (BinaryReader imgFileReader = new BinaryReader(new FileStream(imgFile, FileMode.Open)))
            {
                using (FileStream fs = new FileStream(indFile, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        br.ReadBytes(2); //Skip first 2 bytes (amount of files in archive?)
                        byte[] chunk;

                        chunk = br.ReadBytes(CHUNK_SIZE);
                        while (chunk.Length > 0)
                        {
                            var currentFile = ParseBytes(chunk, chunk.Length);
                            chunk = br.ReadBytes(CHUNK_SIZE);

                            var nextFile = ParseBytes(chunk, chunk.Length);

                            uint filesize = (nextFile.byteOffset - 1) - currentFile.byteOffset;

                            byte[] outputFileBytes = new byte[filesize];
                            imgFileReader.BaseStream.Seek(currentFile.byteOffset, SeekOrigin.Begin);
                            imgFileReader.Read(outputFileBytes, 0, Convert.ToInt32(filesize));

                            string outputFileName = currentFile.filename.TrimEnd('\0');

                            if (!currentFile.filename.Contains('*')) //Skip " *resitems*  in file
                            {
                                File.WriteAllBytes(outputDir + "\\" + outputFileName, outputFileBytes);
                            }
                            Console.WriteLine("File " + outputFileName + " saved" );

                        }
                    }
                }
            }

        }


        public static (string filename, uint byteOffset) ParseBytes(byte[] bdata, int len)
        {
            if(len < 24)
            {
                throw new Exception("invalid data length");
            }
            var fileBytes = new Byte[20];
            var offsetBytes = new Byte[4];
            Array.Copy(bdata, 0, fileBytes, 0, 20); 
            Array.Copy(bdata, 20, offsetBytes, 0, 4);

            string filename = Encoding.UTF8.GetString(fileBytes);


            uint imgOffset = BitConverter.ToUInt32(offsetBytes);

            return (filename, imgOffset);
        }

        public static void DumpBytes(byte[] bdata, int len)
        {
            int i;
            int j = 0;
            char dchar;
            // 3 * 16 chars for hex display, 16 chars for text and 8 chars
            // for the 'gutter' int the middle.
            StringBuilder dumptext = new StringBuilder("        ", 16 * 4 + 8);
            for (i = 0; i < len; i++)
            {
                dumptext.Insert(j * 3, String.Format("{0:X2} ", (int)bdata[i]));
                dchar = (char)bdata[i];
                //' replace 'non-printable' chars with a '.'.
                if (Char.IsWhiteSpace(dchar) || Char.IsControl(dchar))
                {
                    dchar = '.';
                }
                dumptext.Append(dchar);
                j++;
                if (j == 16)
                {
                    Console.WriteLine(dumptext);
                    dumptext.Length = 0;
                    dumptext.Append("        ");
                    j = 0;
                }
            }
            // display the remaining line
            if (j > 0)
            {
                for (i = j; i < 16; i++)
                {
                    dumptext.Insert(j * 3, "   ");
                }
                Console.WriteLine(dumptext);
            }
        }

        public static void ReadIndexFile(string indFile)
        {

            Console.WriteLine("Hello World! " + indFile);

            byte[] indFileBytes = File.ReadAllBytes(indFile);
            Console.WriteLine("First byte: {0}", indFileBytes[0]);
            Console.WriteLine("Last byte: {0}",
                indFileBytes[indFileBytes.Length - 1]);
            Console.WriteLine(indFileBytes.Length);

            int offset = 0;
            do
            {
                byte[] dirbytes = { indFileBytes[0 + offset], indFileBytes[1 + offset] };

                for (int i = 0; i == 20; i++)
                {

                }
                byte[] filenamebytes = { indFileBytes[2 + offset],
                    indFileBytes[3 + offset], indFileBytes[4 + offset],
                    indFileBytes[5 + offset], indFileBytes[6  + offset],
                    indFileBytes[7  + offset], indFileBytes[8 + offset],
                    indFileBytes[9 + offset], indFileBytes[10 + offset],
                    indFileBytes[11 + offset], indFileBytes[12 + offset],
                    indFileBytes[13 + offset], indFileBytes[14 + offset]
                };
                string filename = System.Text.Encoding.UTF8.GetString(filenamebytes);

                byte[] imgOffsetBytes = { indFileBytes[15 + offset], indFileBytes[16 + offset] };

                int imgOffset = BitConverter.ToInt16(imgOffsetBytes);

                int dirID = BitConverter.ToInt16(dirbytes);
                Console.WriteLine("Dir ID: " + dirID + " File name: " + filename + "Byte offset in IMG file: " + imgOffset);
                offset = offset + 24;
            } while (offset < indFileBytes.Length);

        }
    }
}
