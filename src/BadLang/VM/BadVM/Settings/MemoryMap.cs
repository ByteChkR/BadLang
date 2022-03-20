using BadVM.Shared.Memory;

namespace BadVM.Settings
{

    public class MemoryMap
    {

        public int StartAddress { get; set; }

        public int Length { get; set; }

        public bool ReadOnly { get; set; }

        #region Public

        public MemoryMapEntry MakeEntry()
        {
            byte[] data = new byte[Length];

            return data.ToMemoryEntry( StartAddress, ReadOnly );
        }

        #endregion

    }

}
