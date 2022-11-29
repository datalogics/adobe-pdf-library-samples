![Datalogics Adobe PDF Library](https://www.datalogics.com/wp-content/uploads/2022/09/Datalogics-Logo-1-e1664565864201.png)

[Datalogics](https://www.datalogics.com)&nbsp;|&nbsp; [Free Trial Download](https://www.datalogics.com/adobe-pdf-library/) &nbsp;|&nbsp; [Documentation](https://dev.datalogics.com/) &nbsp;|&nbsp; [Support](https://www.datalogics.com/tech-support-pdfs/) &nbsp;|&nbsp;[NuGet](https://www.nuget.org/profiles/Datalogics)

<br>

# .NET Core Sample Programs
## ***Introduction***
The Adobe PDF Library (APDFL) is an Application Programming Interface (API) designed to allow programmers to work with the Adobe PDF file format.  The APDFL Software Development Kit (SDK) provides a method for software developers and vendors to build their own third-party systems that allow them to create, change, process, review, and otherwise work with PDF files. The Library is the same one that Adobe Acrobat is based on.

Datalogics provides a .NET Core interface to the Adobe PDF Library. This Interface offers a set of modules for the Library that allow programmers working in C# or other languages supported by .NET Core to take advantage of Adobe PDF Library tools and resources. This interface encapsulates the original Adobe PDF Library; the interface allows you to work with the original core library functions directly, and seamlessly, in .NET Core.

## ***The Sample Programs***
The Adobe PDF Library supports all of the languages that work with .NET Core, including C# and Visual Basic. The Adobe PDF Library provides a set of sample C# program files, stored under /DotNETCore/Sample_Source.

Most of the code samples in APDFL are designed to demonstrate how an API works by completing a simple programming task. You can open the sample .NET Core program files and review them in Microsoft Visual Studio 2019, Visual Studio Code, or a text editor on a Linux or macOS platform for reference, or you can copy parts of this code to use in your own programs.  You can also build and run sample programs from your Operating System command line.

For example, you would access this directory, where the sample program files are stored, to build and run the sample:

```C:\APDFL18\DotNETCore\Sample_Source\Text\```

We assume a basic level of technical understanding of the PDF file format, though we seek to review the features of each of these sample programs carefully.

Many of these sample programs automatically generate an output file or set of files.  These output files, generally PDF or graphics files (JPG or BMP), are stored in the directory where the application has been run. If you run a sample program a second or third time, it will overwrite any output files that were created and stored earlier.  However, if you run a sample program, generate a PDF output file, and then open that PDF file and try to run that sample program again, you will see an error message.  The program will not be able to overwrite an existing output file if that file is currently open in Adobe Reader or Adobe Acrobat.

Note that the Forms Extension plug-in sample programs provided with the .NET Interface with the Adobe PDF Library are not available with .NET Core.

## ***Building and Running .NET Core Sample Programs***
Start from your DotNETCore directory, and change to the Sample_Source directory for the program you want to work with. Here is an example:

```cd Sample_Source/Images/RasterizePage```

Build a sample project file using the Debug configuration. Use the dotnet program command, which is used with .NET Core to build projects and run applications:

```dotnet build -c Debug ./RasterizePage.csproj /p:Platform=[x64|ARM64]```

If you want to build a Release use this command syntax instead:

```dotnet build -c Release ./RasterizePage.csproj /p:Platform=[x64|ARM64]```

Change to the **Binaries** directory:

```cd ../../../Binaries```

Run the application by specifying the .dll file. Note the .dll file is the .NET Core executable for all platforms:

```dotnet ./RasterizePage.dll```

**Note**: Adobe PDF Library dependencies are found in the DotNETCore/Binaries directory. Samples are setup to write their .NET Core .dll files to this directory along with any other dependencies, such as SkiaSharp.
