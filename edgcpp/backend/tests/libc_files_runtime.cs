using System;
using System.IO;
using System.Text;
using System.Threading;
using Edg.Runtime;

internal static unsafe class LibcFilesRuntimeTests
{
    private static void Require(bool value, string message)
    { if (!value) throw new Exception(message); }
    private static byte[] CString(string value) => Encoding.UTF8.GetBytes(value + "\0");
    private static void* Open(LibcFiles files, string path, string mode)
    {
        fixed (byte* p = CString(path), m = CString(mode)) return files.Fopen((sbyte*)p, (sbyte*)m);
    }
    private sealed class BrokenStream : MemoryStream
    {
        public override int Read(Span<byte> buffer) => throw new IOException("injected read failure");
        public override void Write(ReadOnlySpan<byte> buffer) => throw new IOException("injected write failure");
    }
    private sealed class ShortStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override int Read(Span<byte> buffer) => base.Read(buffer[..Math.Min(1, buffer.Length)]);
    }
    private sealed class CountingStream(byte[] bytes) : MemoryStream(bytes)
    {
        internal int Reads;
        public override int Read(Span<byte> buffer) { ++Reads; return base.Read(buffer); }
    }
    private sealed class PartialFailureStream : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> buffer)
        { if(buffer.Length!=0)base.Write(buffer[..1]);throw new IOException("injected after prefix"); }
    }

    private static int Main(string[] args)
    {
        string directory = Path.GetFullPath(args[0]); Directory.CreateDirectory(directory);
        int error = 0, otherError = 0;
        using var output = new MemoryStream(); using var diagnostics = new MemoryStream();
        using var input = new ShortStream([0, 128, 255]);
        using var files = new LibcFiles(value => error = value, input, output, diagnostics, directory);
        using var other = new LibcFiles(value => otherError = value, new MemoryStream(), new MemoryStream(), new MemoryStream(), directory);
        byte* data = stackalloc byte[64]; sbyte* buffer = stackalloc sbyte[16];

        Require(files.Setvbuf(files.Stdout, buffer, 0, 16) == 0, "set full buffer");
        data[0] = 65; data[1] = 0; data[2] = 255;
        Require(files.Fwrite(data, 1, 3, files.Stdout) == 3 && output.Length == 0, "binary output buffered");
        Require(files.Ftell(files.Stdout) == 3, "tell includes pending output");
        Require(files.Fflush(files.Stdout) == 0 && output.ToArray().AsSpan().SequenceEqual(new byte[] {65,0,255}), "flush byte exact");
        Require(files.Setvbuf(files.Stdout, null, 2, 0) != 0, "setvbuf after use rejected");

        Require(files.Fread(data, 2, 2, files.Stdin) == 1, "partial object count");
        Require(data[0] == 0 && data[1] == 128 && data[2] == 255, "short stream reads complete");
        Require(files.Feof(files.Stdin) != 0 && files.Ferror(files.Stdin) == 0, "EOF distinct from error");
        Require(files.Ungetc(254, files.Stdin) == 254 && files.Feof(files.Stdin) == 0, "pushback clears EOF");
        Require(files.Ftell(files.Stdin) == 2 && files.Fgetc(files.Stdin) == 254 && files.Ftell(files.Stdin) == 3, "pushback position");
        Require(files.Fgetc(files.Stdin) == -1 && files.Feof(files.Stdin) != 0, "EOF after pushed byte");
        error = 71; files.Clearerr(files.Stdin);
        Require(files.Feof(files.Stdin) == 0 && files.Ferror(files.Stdin) == 0 && error == 71, "clearerr preserves errno");
        Require(files.Fputc(1, files.Stdin) == -1 && files.Ferror(files.Stdin) != 0 && error == 9, "wrong stream direction error");
        files.Clearerr(files.Stdin);

        void** slot = files.StdoutLocation; void* original = *slot;
        GC.Collect(2,GCCollectionMode.Forced,true,true); GC.WaitForPendingFinalizers();
        Require(slot == files.StdoutLocation && *slot == original, "standard global pinned");
        *slot = files.Stderr;
        fixed(byte* text = CString("message")) Require(files.Puts((sbyte*)text) == 0, "puts selected output");
        Require(Encoding.UTF8.GetString(diagnostics.ToArray()) == "message\n", "standard global write updates calls");
        *slot = original;
        Require(other.Stdout != files.Stdout && other.StdoutLocation != files.StdoutLocation, "owner handles/globals isolated");
        Require(other.Fclose(files.Stdout) == -1 && otherError == 9, "foreign owner handle rejected");

        using var lineOutput = new MemoryStream();
        using(var line = new LibcFiles(_ => {}, new MemoryStream(),lineOutput,new MemoryStream(),directory))
        {
            Require(line.Setvbuf(line.Stdout, null, 1, 8) == 0, "line buffer");
            data[0] = 97; data[1] = 10; data[2] = 98;
            Require(line.Fwrite(data,1,3,line.Stdout)==3 && lineOutput.Length==2, "line flush excludes trailing fragment");
            Require(line.Fflush(null)==0 && lineOutput.Length==3, "flush all");
        }
        Require(lineOutput.CanWrite,"host owns standard streams");

        using var lineInput = new CountingStream(Encoding.UTF8.GetBytes("abc\ndef"));
        using(var lines = new LibcFiles(value=>error=value,lineInput,new MemoryStream(),new MemoryStream(),directory))
        {
            Require(lines.Fgets((sbyte*)data,3,lines.Stdin)==(sbyte*)data && data[0]=='a' && data[1]=='b' && data[2]==0,"fgets bounded prefix");
            Require(lines.Ftell(lines.Stdin)==2 && lineInput.Reads==1,"input readahead logical position");
            Require(lines.Fflush(lines.Stdin)==0 && lineInput.Position==2,"input fflush restores unread offset");
            Require(lines.Fgets((sbyte*)data,64,lines.Stdin)==(sbyte*)data && data[0]=='c' && data[1]=='\n' && data[2]==0,"fgets includes newline");
            Require(lines.Getchar()=='d' && lines.Ftell(lines.Stdin)==5,"getchar buffered position");
            Require(lines.Fgets((sbyte*)data,1,lines.Stdin)==(sbyte*)data && data[0]==0 && lines.Ftell(lines.Stdin)==5,"fgets size1 does not consume");
            Require(lines.Fgets((sbyte*)data,64,lines.Stdin)==(sbyte*)data && data[0]=='e' && data[1]=='f' && data[2]==0 && lines.Feof(lines.Stdin)!=0,"fgets partial line at EOF");
            data[0]=77;Require(lines.Fgets((sbyte*)data,64,lines.Stdin)==null && data[0]==77,"fgets empty EOF preserves buffer");
            Require(lines.Fgets((sbyte*)data,0,lines.Stdin)==null,"fgets size0");
        }

        string path = "bytes.bin";
        void* f = Open(files,path,"w+b"); Require(f != null,"open binary update");
        for(int i=0;i<16;++i)data[i]=(byte)(i*17);
        Require(files.Fwrite(data,2,8,f)==8 && files.Ftell(f)==16,"buffered write position");
        Require(files.Fseek(f,0,0)==0,"seek flushes");
        long position=-1;Require(files.Fgetpos(f,&position)==0 && position==0,"getpos");
        Require(files.Fread(data,1,16,f)==16 && files.Feof(f)==0,"exact end no EOF yet");
        for(int i=0;i<16;++i)Require(data[i]==i*17,"file byte content");
        Require(files.Fgetc(f)==-1 && files.Feof(f)!=0,"read beyond EOF");
        Require(files.Fsetpos(f,&position)==0 && files.Feof(f)==0 && files.Fgetc(f)==0,"setpos clears EOF");
        Require(files.Ungetc(42,f)==42 && files.Fseek(f,1,1)==0 && files.Ftell(f)==1,"SEEK_CUR adjusts pushback");
        Require(files.Fclose(f)==0 && files.Fclose(f)==-1 && error==9,"double close detected");
        void* newer=Open(files,path,"rb");Require(newer!=null && newer!=f,"closed token not reused");files.Fclose(newer);
        Require(Open(files,path,"wx")==null && error==17,"exclusive create existing");
        Require(Open(files,"absent.bin","rb")==null && error==2,"missing file errno");
        Require(Open(files,path,"invalid")==null && error==22,"invalid mode errno");
        f=Open(files,path,"a+b");Require(f!=null,"append update");
        Require(files.Ftell(f)==0 && files.Fgetc(f)==0,"a+ reads initially from beginning");
        Require(files.Fseek(f,0,0)==0 && files.Fputc(77,f)==77 && files.Fflush(f)==0,"append ignores seek for output");
        Require(files.Fseek(f,-1,2)==0 && files.Fgetc(f)==77,"append readback");files.Fclose(f);

        using(var broken = new LibcFiles(value=>error=value,new BrokenStream(),new BrokenStream(),new MemoryStream(),directory))
        {
            Require(broken.Fread(data,1,1,broken.Stdin)==0 && broken.Ferror(broken.Stdin)!=0 && broken.Feof(broken.Stdin)==0,"read failure not EOF");
            Require(broken.Setvbuf(broken.Stdout,null,0,2)==0,"broken full buffer");
            Require(broken.Fwrite(data,1,2,broken.Stdout)==0 && broken.Ferror(broken.Stdout)!=0,"write failure partial count");
        }
        using var partialOutput=new PartialFailureStream();
        using(var partial=new LibcFiles(value=>error=value,new MemoryStream(),partialOutput,new MemoryStream(),directory))
        {
            Require(partial.Setvbuf(partial.Stdout,null,0,2)==0,"partial error buffer");
            Require(partial.Fwrite(data,1,2,partial.Stdout)==0 && partial.Ferror(partial.Stdout)!=0,"partial external failure reported");
            Require(partialOutput.Length==1 && partial.Fflush(partial.Stdout)==0 && partialOutput.Length==1,"failed buffer not duplicated on retry");
        }
        using(var chars=new LibcFiles(_=>{},new MemoryStream(),new MemoryStream(),new MemoryStream(),directory))
            Require(chars.Putchar(511)==255,"putchar unsigned byte");
        string oldName="original-ž.txt",newName="renamed-ž.txt";
        File.WriteAllText(Path.Combine(directory,oldName),"source");File.WriteAllText(Path.Combine(directory,newName),"destination");
        fixed(byte* oldPath=CString(oldName),newPath=CString(newName))
        {
            Require(files.Rename((sbyte*)oldPath,(sbyte*)newPath)==0,"rename existing file");
            Require(!File.Exists(Path.Combine(directory,oldName)) && File.ReadAllText(Path.Combine(directory,newName))=="source","rename replacement bytes");
            Require(files.Remove((sbyte*)newPath)==0 && files.Remove((sbyte*)newPath)==-1 && error==2,"remove missing errno");
        }
        Directory.CreateDirectory(Path.Combine(directory,"empty-directory"));
        fixed(byte* p=CString("empty-directory"))Require(files.Remove((sbyte*)p)==0,"remove empty directory");
        void* ownerFile=Open(other,"owner.bin","wb");Require(ownerFile!=null,"second owner file");
        Require(other.Fwrite(data,1,3,ownerFile)==3,"owner pending bytes");other.Dispose();
        Require(File.ReadAllBytes(Path.Combine(directory,"owner.bin")).Length==3,"dispose flushes owned files");
        Require(other.Fclose(ownerFile)==-1 && otherError==9,"disposed owner handle rejected");
        const int writers=4, records=1000;
        string appendPath=Path.Combine(directory,"concurrent-append.bin");
        File.WriteAllBytes(appendPath,[]);
        using var barrier=new Barrier(writers);
        var threads=new Thread[writers];var failures=new Exception?[writers];
        for(int writer=0;writer<writers;++writer)
        {
            int id=writer;
            threads[id]=new Thread(()=>{
                try
                {
                    using var independent=new LibcFiles(_=>{},new MemoryStream(),new MemoryStream(),new MemoryStream(),directory);
                    void* append=Open(independent,"concurrent-append.bin","ab");
                    Require(append!=null && independent.Setvbuf(append,null,2,0)==0,"open independent append");
                    byte* record=stackalloc byte[8];
                    Require(barrier.SignalAndWait(TimeSpan.FromSeconds(30)),"append writers reached barrier");
                    for(int i=0;i<records;++i)
                    {
                        *(int*)record=id;*(int*)(record+4)=i;
                        if(i%17==0)Require(independent.Fseek(append,0,0)==0,"append explicit seek");
                        Require(independent.Fwrite(record,8,1,append)==1,"concurrent append full record");
                    }
                    Require(independent.Fclose(append)==0,"concurrent close");
                }
                catch(Exception ex){failures[id]=ex;}
            });
            threads[id].Start();
        }
        foreach(var thread in threads)thread.Join();
        foreach(var failure in failures)if(failure!=null)throw failure;
        byte[] appended=File.ReadAllBytes(appendPath);
        Require(appended.Length==writers*records*8,"concurrent append never overwrites");
        bool[,] seen=new bool[writers,records];
        for(int i=0;i<appended.Length;i+=8)
        {
            int writer=BitConverter.ToInt32(appended,i),record=BitConverter.ToInt32(appended,i+4);
            Require(writer>=0 && writer<writers && record>=0 && record<records && !seen[writer,record],"append records intact unique");
            seen[writer,record]=true;
        }
        Console.WriteLine("libc FILE runtime: byte/buffer/EOF/position/owner/error assertions and 4000 concurrent append records passed");
        return 0;
    }
}
