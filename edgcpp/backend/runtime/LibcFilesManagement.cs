#nullable enable
using System;
using System.IO;

namespace Edg.Runtime;

public sealed unsafe partial class LibcFiles
{
    public void Rewind(void* file)
    {
        var entry=Find(file);if(entry is null)return;
        lock(entry.Gate)
        {
            if(entry.Closed){Fail(null,Ebadf);return;}
            Fseek(file,0,0);
            // Unlike fseek, rewind clears the error indicator on failure too.
            entry.Error=false;
        }
    }

    public void Setbuf(void* file,sbyte* buffer) => Setvbuf(file,buffer,buffer==null?2:0,8192);

    public void Perror(void* file,sbyte* message,sbyte* errorText)
    {
        var entry=Find(file);if(entry is null)return;
        lock(entry.Gate)
        {
            // perror must not establish/change orientation or encoding. Internal
            // raw writes preserve both, including when stderr is wide oriented.
            if(message!=null && *message!=0)
            {
                PerrorText(entry,message);
                byte* separator=stackalloc byte[2];separator[0]=(byte)':';separator[1]=(byte)' ';
                WriteBytes(separator,1,2,entry);
            }
            PerrorText(entry,errorText);
            byte newline=(byte)'\n';WriteBytes(&newline,1,1,entry);
        }
    }
    private void PerrorText(Entry entry,sbyte* text)
    {
        if(text==null)return;
        ulong length=0;while(text[length]!=0)++length;
        if(length!=0)WriteBytes(text,1,length,entry);
    }

    private static void ResetAssociation(Entry entry)
    {
        entry.Closed=entry.End=entry.Error=entry.Started=false;
        entry.Direction=entry.Orientation=entry.FailureEpoch=0;
        entry.Pushback=entry.WidePushback=-1;entry.WidePushbackBytes=0;
        entry.Utf8Encoding=false;entry.DecodeState=entry.DecodeValue=0;
        entry.ReadOffset=entry.ReadCount=0;entry.Used=0;
    }

    private bool ChangeMode(Entry entry,sbyte* mode)
    {
        string spelling=Decode(mode);
        if(spelling.Length==0 || "rwa".IndexOf(spelling[0])<0){setErrno(Einval);return false;}
        bool update=false,binary=false,exclusive=false;
        for(int i=1;i<spelling.Length;++i)
        {
            if(spelling[i]=='+'&&!update)update=true;
            else if(spelling[i]=='b'&&!binary)binary=true;
            else if(spelling[i]=='x'&&!exclusive&&spelling[0]=='w')exclusive=true;
            else {setErrno(Einval);return false;}
        }
        bool read=spelling[0]=='r'||update,write=spelling[0]!='r'||update,append=spelling[0]=='a';
        if((read&&!entry.Stream.CanRead)||(write&&!entry.Stream.CanWrite)){setErrno(Ebadf);return false;}
        if(entry.Stream is FileStream file)LibcAppend.SetAppend(file,append);
        else if(fileDescriptors is not null)fileDescriptors.SetAppend(entry.Stream,append);
        else if(append){setErrno(95);return false;}
        entry.Readable=read;entry.Writable=write;entry.Append=append;
        // Musl's NULL-filename profile changes access/append state without
        // truncating or repositioning the underlying file; no access upgrade.
        ResetAssociation(entry);return true;
    }

    public void* Freopen(sbyte* path,sbyte* mode,void* file)
    {
        var entry=Find(file);if(entry is null)return null;
        lock(entry.Gate)
        {
            if(entry.Closed){Fail(null,Ebadf);return null;}
            Fflush(file); // Source freopen ignores failure of its closing flush.
            try
            {
                if(path==null)
                {
                    if(ChangeMode(entry,mode))return file;
                }
                else
                {
                    Entry? replacement=OpenEntry(path,mode);
                    if(replacement is not null)
                    {
                        // Keep the exact Entry/Gate: operations already waiting
                        // on this FILE must observe its new association.
                        try {if(entry.OwnStream)entry.Stream.Dispose();}
                        catch(Exception error) when(PlatformFailure(error)) { }
                        entry.Stream=replacement.Stream;entry.Readable=replacement.Readable;
                        entry.Writable=replacement.Writable;entry.Append=replacement.Append;
                        entry.OwnStream=true;ResetAssociation(entry);
                        return file;
                    }
                }
            }
            catch(Exception error) when(PlatformFailure(error)){setErrno(ErrorNumber(error));}
            // Failure closes the original association. Preserve the opening
            // errno across cleanup, without needing an ambient errno getter.
            try {if(entry.OwnStream)entry.Stream.Dispose();}
            catch(Exception error) when(PlatformFailure(error)) { }
            entry.Closed=true;
            if(entry.OwnBuffer)System.Runtime.InteropServices.NativeMemory.Free(entry.Buffer);
            entry.Buffer=null;entry.Capacity=entry.Used=0;
            return null;
        }
    }

    private byte[]? temporaryName;
    private ulong temporaryNameSeed,temporaryNameSequence;

    partial void DisposeManagementState()
    {
        temporaryName=null;temporaryNameSeed=temporaryNameSequence=0;
    }

    public sbyte* Tmpnam(sbyte* buffer)
    {
        lock(gate)
        {
            if(disposed){setErrno(Ebadf);return null;}
            try
            {
                if(temporaryName is null)
                {
                    temporaryName=System.GC.AllocateArray<byte>(20,pinned:true);
                    System.Security.Cryptography.RandomNumberGenerator.Fill(temporaryName.AsSpan(0,8));
                    temporaryNameSeed=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(temporaryName);
                }
                byte* spelling=stackalloc byte[20];spelling[0]=(byte)'e';spelling[1]=(byte)'d';spelling[2]=(byte)'g';spelling[19]=0;
                for(int attempt=0;attempt<100;++attempt)
                {
                    // Within one owner each request has a distinct spelling,
                    // including calls using the single reusable static buffer.
                    ulong value=unchecked(temporaryNameSeed+temporaryNameSequence++);
                    for(int i=0;i<16;++i){uint digit=(uint)(value&15);spelling[18-i]=(byte)(digit<10?'0'+digit:'a'+digit-10);value>>=4;}
                    string relative=System.Text.Encoding.ASCII.GetString(new ReadOnlySpan<byte>(spelling,19));
                    string path=fileDescriptors?.ResolvePath(relative) ?? Path.Combine(workingDirectory,relative);
                    try{File.GetAttributes(path);continue;}
                    catch(FileNotFoundException) { }
                    catch(DirectoryNotFoundException) { }
                    // As required by tmpnam this tests availability without
                    // creating/reserving a file. Callers must handle races.
                    if(buffer!=null){Buffer.MemoryCopy(spelling,buffer,20,20);return buffer;}
                    fixed(byte* result=temporaryName){Buffer.MemoryCopy(spelling,result,20,20);return (sbyte*)result;}
                }
                setErrno(17);return null;
            }
            catch(Exception error) when(PlatformFailure(error)){setErrno(ErrorNumber(error));return null;}
        }
    }

    public void* Tmpfile()
    {
        Stream? stream=null;
        try
        {
            for(int attempt=0;attempt<100;++attempt)
            {
                string path=Path.Combine(Path.GetTempPath(),"edg-"+Path.GetRandomFileName());
                var options=new FileStreamOptions {Mode=FileMode.CreateNew,Access=FileAccess.ReadWrite,
                    Share=FileShare.ReadWrite|FileShare.Delete,BufferSize=1};
                if(!OperatingSystem.IsWindows())options.UnixCreateMode=UnixFileMode.UserRead|UnixFileMode.UserWrite;
                try {stream=fileDescriptors is not null ? fileDescriptors.Open(path,"w+x") : new FileStream(path,options);}
                catch(IOException error) when(ErrorNumber(error)==17){continue;}
                // BCL DeleteOnClose on Unix runs only during managed release.
                // Unlink now so process-wide Environment.Exit also reclaims it.
                try {File.Delete(path);}
                catch
                {
                    stream.Dispose();stream=null;
                    throw;
                }
                void* result=Add(new Entry(stream,true,true,false,true));stream=null;return result;
            }
            setErrno(17);return null;
        }
        catch(Exception error) when(PlatformFailure(error)){setErrno(ErrorNumber(error));return null;}
        finally {stream?.Dispose();}
    }
}
