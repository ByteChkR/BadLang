namespace BadVM.Settings;

public class BadLangSettings
{

    public int StackSize { get; set; } = 0xFFFFF; // 1MB

    public int StackStart { get; set; } = 0x8000;

    public int ThreadCount { get; set; } = 1;

    public int HyperThreadCount { get; set; } = 1;

    public MemoryMap[] MemoryLayout { get; set; } = new MemoryMap[]
                                                    {
                                                        new MemoryMap()
                                                        {
                                                            StartAddress = 0x8000,
                                                            Length = 0x4000000
                                                        }
                                                    };

    public AssemblyLoadSettings AssemblySettings { get; set; } = new AssemblyLoadSettings();

}
