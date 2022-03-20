namespace basm;

internal class CompilerSettings
{

    public string[] IncludeDirectores { get; set; } = new[]
                                                      {
                                                          Path.Combine(
                                                                       AppDomain.CurrentDomain.BaseDirectory,
                                                                       "data",
                                                                       "runtime",
                                                                       "interop",
                                                                       "Headers"
                                                                      )
                                                      };

}
