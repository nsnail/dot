using System.Reflection;
using Dot;

[assembly: AssemblyCompany(AssemblyInfo.ASSEMBLY_COMPANY)]
[assembly: AssemblyCopyright(AssemblyInfo.ASSEMBLY_COPYRIGHT)]
[assembly: AssemblyFileVersion(AssemblyInfo.ASSEMBLY_FILE_VERSION)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.ASSEMBLY_INFORMATIONAL_VERSION)]
[assembly: AssemblyProduct(AssemblyInfo.ASSEMBLY_PRODUCT)]
[assembly: AssemblyTitle(AssemblyInfo.ASSEMBLY_TITLE)]
[assembly: AssemblyVersion(AssemblyInfo.ASSEMBLY_VERSION)]
[assembly: AssemblyMetadata("RepositoryUrl",  AssemblyInfo.ASSEMBLY_METADATA_REPOSITORY_URL)]
[assembly: AssemblyMetadata("RepositoryType", AssemblyInfo.ASSEMBLY_METADATA_REPOSITORY_TYPE)]

namespace Dot;

internal static class AssemblyInfo
{
    public const  string ASSEMBLY_COMPANY                  = "nsnail";
    public const  string ASSEMBLY_COPYRIGHT                = $"Copyright (c) 2022 {ASSEMBLY_COMPANY}";
    public const  string ASSEMBLY_FILE_VERSION             = _VERSION;
    public const  string ASSEMBLY_INFORMATIONAL_VERSION    = _VERSION;
    public const  string ASSEMBLY_METADATA_REPOSITORY_TYPE = "git";
    public const  string ASSEMBLY_METADATA_REPOSITORY_URL  = "https://github.com/nsnail/dot.git";
    public const  string ASSEMBLY_PRODUCT                  = "dot";
    public const  string ASSEMBLY_TITLE                    = "功能全面的实用工具 - 程序员的瑞士军刀";
    public const  string ASSEMBLY_VERSION                  = _VERSION;
    private const string _VERSION                          = "1.1.7.1";
}