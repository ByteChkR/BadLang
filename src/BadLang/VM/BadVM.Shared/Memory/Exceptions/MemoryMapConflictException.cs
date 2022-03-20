using BadVM.Shared.Exceptions;

namespace BadVM.Shared.Memory.Exceptions;

public class MemoryMapConflictException : RuntimeException
{

    public MemoryMapEntry Entry { get; }

    public MemoryMapEntry NewEntry { get; }

    #region Public

    public MemoryMapConflictException( MemoryMapEntry entry, MemoryMapEntry newEntry ) : base(
         $"Entry {entry} intersects with existing entry {newEntry}"
        )
    {
        Entry = entry;
        NewEntry = newEntry;
    }

    #endregion

}
