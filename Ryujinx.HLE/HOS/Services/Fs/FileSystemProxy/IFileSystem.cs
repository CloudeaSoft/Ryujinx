using LibHac;
using LibHac.Fs;
using LibHac.FsSrv.Sf;

namespace Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy
{
    class IFileSystem : DisposableIpcService
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.Sf.IFileSystem> _fileSystem;

        public IFileSystem(ReferenceCountedDisposable<LibHac.FsSrv.Sf.IFileSystem> provider)
        {
            _fileSystem = provider;
        }

        public ReferenceCountedDisposable<LibHac.FsSrv.Sf.IFileSystem> GetBaseFileSystem()
        {
            return _fileSystem;
        }

        [CommandHipc(0)]
        // CreateFile(u32 createOption, u64 size, buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CreateFile(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            int createOption = context.RequestData.ReadInt32();
            context.RequestData.BaseStream.Position += 4;

            long size = context.RequestData.ReadInt64();

            return (ResultCode)_fileSystem.Target.CreateFile(in name, size, createOption).Value;
        }

        [CommandHipc(1)]
        // DeleteFile(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteFile(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            return (ResultCode)_fileSystem.Target.DeleteFile(in name).Value;
        }

        [CommandHipc(2)]
        // CreateDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CreateDirectory(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            return (ResultCode)_fileSystem.Target.CreateDirectory(in name).Value;
        }

        [CommandHipc(3)]
        // DeleteDirectory(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteDirectory(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            return (ResultCode)_fileSystem.Target.DeleteDirectory(in name).Value;
        }

        [CommandHipc(4)]
        // DeleteDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode DeleteDirectoryRecursively(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            return (ResultCode)_fileSystem.Target.DeleteDirectoryRecursively(in name).Value;
        }

        [CommandHipc(5)]
        // RenameFile(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public ResultCode RenameFile(ServiceCtx context)
        {
            ref readonly Path currentName = ref FileSystemProxyHelper.GetSfPath(context, index: 0);
            ref readonly Path newName = ref FileSystemProxyHelper.GetSfPath(context, index: 1);

            return (ResultCode)_fileSystem.Target.RenameFile(in currentName, in newName).Value;
        }

        [CommandHipc(6)]
        // RenameDirectory(buffer<bytes<0x301>, 0x19, 0x301> oldPath, buffer<bytes<0x301>, 0x19, 0x301> newPath)
        public ResultCode RenameDirectory(ServiceCtx context)
        {
            ref readonly Path currentName = ref FileSystemProxyHelper.GetSfPath(context, index: 0);
            ref readonly Path newName = ref FileSystemProxyHelper.GetSfPath(context, index: 1);

            return (ResultCode)_fileSystem.Target.RenameDirectory(in currentName, in newName).Value;
        }

        [CommandHipc(7)]
        // GetEntryType(buffer<bytes<0x301>, 0x19, 0x301> path) -> nn::fssrv::sf::DirectoryEntryType
        public ResultCode GetEntryType(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            Result result = _fileSystem.Target.GetEntryType(out uint entryType, in name);

            context.ResponseData.Write((int)entryType);

            return (ResultCode)result.Value;
        }

        [CommandHipc(8)]
        // OpenFile(u32 mode, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IFile> file
        public ResultCode OpenFile(ServiceCtx context)
        {
            uint mode = context.RequestData.ReadUInt32();

            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            Result result = _fileSystem.Target.OpenFile(out ReferenceCountedDisposable<LibHac.FsSrv.Sf.IFile> file, in name, mode);

            if (result.IsSuccess())
            {
                IFile fileInterface = new IFile(file);

                MakeObject(context, fileInterface);
            }

            return (ResultCode)result.Value;
        }

        [CommandHipc(9)]
        // OpenDirectory(u32 filter_flags, buffer<bytes<0x301>, 0x19, 0x301> path) -> object<nn::fssrv::sf::IDirectory> directory
        public ResultCode OpenDirectory(ServiceCtx context)
        {
            uint mode = context.RequestData.ReadUInt32();

            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            Result result = _fileSystem.Target.OpenDirectory(out ReferenceCountedDisposable<LibHac.FsSrv.Sf.IDirectory> dir, name, mode);

            if (result.IsSuccess())
            {
                IDirectory dirInterface = new IDirectory(dir);

                MakeObject(context, dirInterface);
            }

            return (ResultCode)result.Value;
        }

        [CommandHipc(10)]
        // Commit()
        public ResultCode Commit(ServiceCtx context)
        {
            return (ResultCode)_fileSystem.Target.Commit().Value;
        }

        [CommandHipc(11)]
        // GetFreeSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalFreeSpace
        public ResultCode GetFreeSpaceSize(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            Result result = _fileSystem.Target.GetFreeSpaceSize(out long size, in name);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        [CommandHipc(12)]
        // GetTotalSpaceSize(buffer<bytes<0x301>, 0x19, 0x301> path) -> u64 totalSize
        public ResultCode GetTotalSpaceSize(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            Result result = _fileSystem.Target.GetTotalSpaceSize(out long size, in name);

            context.ResponseData.Write(size);

            return (ResultCode)result.Value;
        }

        [CommandHipc(13)]
        // CleanDirectoryRecursively(buffer<bytes<0x301>, 0x19, 0x301> path)
        public ResultCode CleanDirectoryRecursively(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            return (ResultCode)_fileSystem.Target.CleanDirectoryRecursively(in name).Value;
        }

        [CommandHipc(14)]
        // GetFileTimeStampRaw(buffer<bytes<0x301>, 0x19, 0x301> path) -> bytes<0x20> timestamp
        public ResultCode GetFileTimeStampRaw(ServiceCtx context)
        {
            ref readonly Path name = ref FileSystemProxyHelper.GetSfPath(context);

            Result result = _fileSystem.Target.GetFileTimeStampRaw(out FileTimeStampRaw timestamp, in name);

            context.ResponseData.Write(timestamp.Created);
            context.ResponseData.Write(timestamp.Modified);
            context.ResponseData.Write(timestamp.Accessed);

            byte[] data = new byte[8];

            // is valid?
            data[0] = 1;

            context.ResponseData.Write(data);

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _fileSystem?.Dispose();
            }
        }
    }
}