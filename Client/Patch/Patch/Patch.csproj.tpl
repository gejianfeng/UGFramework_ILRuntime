<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{137966CD-B114-4E8F-84C4-6C511F8F9684}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>PureMVC.Project</RootNamespace>
    <AssemblyName>Patch</AssemblyName>
    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <TargetFrameworkProfile />
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Xml.Linq" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="System.Data" />
    <Reference Include="System.Xml" />
    <Reference Include="Assembly-CSharp">
      <HintPath>..\..\ProjectX\Library\ScriptAssemblies\Assembly-CSharp.dll</HintPath>
    </Reference>
{0}
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Properties\AssemblyInfo.cs" />
{1}
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
  <PropertyGroup>
    <PreBuildEvent>if exist $(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data\game1.data del $(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data\game1.data
if exist $(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data\game2.data del $(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data\game2.data</PreBuildEvent>
  </PropertyGroup>
  <PropertyGroup>
    <PostBuildEvent>if not exist $(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data md $(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data
if exist $(ProjectDir)bin\$(ConfigurationName)\Patch.dll copy "$(ProjectDir)bin\$(ConfigurationName)\Patch.dll" "$(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data\game1.data"
if exist $(ProjectDir)bin\$(ConfigurationName)\Patch.pdb copy "$(ProjectDir)bin\$(ConfigurationName)\Patch.pdb" "$(ProjectDir)..\..\ProjectX\Assets\StreamingAssets\data\game2.data"</PostBuildEvent>
  </PropertyGroup>
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target>
  -->
</Project>