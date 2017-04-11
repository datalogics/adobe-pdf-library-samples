using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.IO;
using IOPath = System.IO.Path;

using Datalogics.PDFL;


/*
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 * 
 */

namespace WebApplication
{
    //
    // This sample opens a XML Paper Specification (XPS) document and converts it to 
    // a Portable Document Format (PDF) document. You'll find the XPS samples here:
    //
    //  http://msdn.microsoft.com/en-us/windows/hardware/gg463429.aspx
    //
    // Once open, it saves the PDF along side the XPS source document (within <XPSPath>\) 
    // as <XPSFile>.pdf. In the event duplicate PDFs are made (e.g., page refresh, 
    // multiple runs of the solution, etc.), subsequent PDFs are renamed to <XPSFile>.#.pdf 
    // where # is a number growing from 1.
    //
    // The code that is of particular importance (i.e., educational) resides in this 
    // file (Default.asax.cs) as well as the file Globals.asax.cs. Supporting XML 
    // constructs reside in Web.config. Most of the code resides within Page_Load (below). 
    // It does so mostly for convenience. It would not be wise to blindly copy the code 
    // in this sample. There are some educational information regarding performance and 
    // design in the code / commentary but this sample has not been vetted for security 
    // nor subject to any serious design or performance review. You are again, cautioned 
    // NOT to blindly use the material (here) as the direct basis for your own work. The 
    // code and composition (here) are simply for educational purposes.
    //
    
    //
    // To use this sample solution one must perform the following prior to building / debugging...
    // 
    // 1) Update the solution reference to Datalogics.PDFL.dll (see Solution Explorer->References);
    // 
    // 2) Open Web.config, scroll to the <appSettings> section and...
    // 2a) Update DLPath to reflect your local system;
    // 
    // 3) Copy the various Datalogics components to the directory referenced by DLPath.
    // 
    // 4) Open Web.config, scroll to the <appSettings> section and...
    // 4a) Update XPSPath to reflect your local system;
    // 4b) Consider updating the value of XPSFile (XPSPth and XPSFile are combined to construct a path to a test XPS file);
    //
    
    public partial class _Default : System.Web.UI.Page
    {
        //
        // srcXPSFilePath points to one of Microsoft's XPS reference samples (feel free to point it at (essentially) any XPS file)
        //
        // Those samples are available (circa 2012) here...
        //  http://msdn.microsoft.com/en-us/windows/hardware/gg463429.aspx
        //
        // For more information about XPS consult Microsoft's documentation here...
        //  http://msdn.microsoft.com/en-us/library/dd316975%28VS.85%29.aspx
        //

        private readonly string srcXPSFilePath = String.Format("{0}{2}{1}", ConfigurationManager.AppSettings[@"XPSPath"], ConfigurationManager.AppSettings[@"XPSFile"], IOPath.DirectorySeparatorChar);
        
        protected void Page_Load(object sender, EventArgs e)
        {
            //
            // THE FOLLOWING (CHECKS/LOADING) SHOULD BE PERFORMED UPSTREAM...
            //
            
            //
            // Make sure we can see the .joboptions file
            //
            
            FileInfo finfoJobOptions = new FileInfo(String.Format("{0}{2}{1}", ConfigurationManager.AppSettings[@"DLPath"], @"Resources\joboptions\Standard.joboptions", IOPath.DirectorySeparatorChar));
            if (!finfoJobOptions.Exists)
            {
                DirectoryInfo dinfo = new DirectoryInfo(ConfigurationManager.AppSettings[@"DLPath"]);
                if (!dinfo.Exists)
                {   // (someone forgot to) go to Web.config and set (or fix) DLPath so it refers to a valid location (please read the instructions for this sample)
                    throw new DirectoryNotFoundException(dinfo.FullName);
                }
                else
                {   // It's pointless to continue...
                    throw new FileNotFoundException(finfoJobOptions.FullName);
                }
            }
            
            //
            // Make sure to have a source (xps) file at the specified location
            //
            
            FileInfo finfoSrc = new FileInfo(srcXPSFilePath);
            if (!finfoSrc.Exists)
            {
                DirectoryInfo dinfo = new DirectoryInfo(ConfigurationManager.AppSettings[@"XPSPath"]);
                if (!dinfo.Exists)
                {   // (someone forgot to) go to Web.config and set (or fix) XPSPath so it refers to a valid location (please read the instructions for this sample)
                    throw new DirectoryNotFoundException(dinfo.FullName);
                }
                else
                {   // It's pointless to continue...
                    throw new FileNotFoundException(finfoSrc.FullName);
                }
            }
            
            //
            // Make sure we can see the destination (directory)
            //
            
            FileInfo finfoDst = new FileInfo(IOPath.ChangeExtension(finfoSrc.FullName, @"pdf"));
            if (!finfoDst.Directory.Exists)
            {   // It's pointless to continue...
                throw new DirectoryNotFoundException(finfoDst.Directory.FullName);
            }
            
            //
            // THE FOLLOWING MAY BE PERFORMED DOWNSTREAM (BUT SHOULD BE SPLIT OUT INTO MULTIPLE LOCATIONS IN ANY ADAPTATION)...
            //
            
            Library lib = null;
            try
            {
                //
                // The astute reader (of this greater sample solution) will notice that there are TWO (nested) Library initializations...
                //
                // 1) perform at application start (in Global.asax.cs)
                // 2) performed here, at page load
                //
                // This bifurcation is actually a GOOD thing since it spreads the load of Library initialization (and termination)
                // across a broader window of time. Consequently, secondary/tertiary et al. page loads will be faster than if 
                // a single initialization (at page load) were employed.
                //
                
                lib = new Library();    // NOTE: This is a SECONDARY library initialization (done for performance reasons)
                
                using (XPSConvertParams xpsParms = new XPSConvertParams(finfoJobOptions.FullName))
                {
                    if (xpsParms == null)
                    {   // Something bad happened and no one managed to throw an exception. Let's do so now...
                        throw new NullReferenceException(@"Error: Cannot construct XPSConvertParams");
                    }
                    
                    FileInfo finfoTmp = null;
                    try
                    {
                        //
                        // DESIGN ALERT: Pay VERY close attention to the relationship of doc (and it's implied file handle) 
                        //               to finfoTmp and the design influences upon the (two!) try/catch/finally blocks. 
                        //               One CANNOT reliably delete the tmp file until AFTER all associated file handles 
                        //               are closed. This only happens upon doc close (or doc dispose). Therefore, the ONLY 
                        //               reliable composition is as shown (here).
                        //
                        
                        Document doc = null;
                        try
                        {
                            //
                            // DESIGN ALERT: Pay VERY close attention to the relationship of doc (and it's implied file handle) 
                            //               to finfoTmp and the design influences upon the (two!) try/catch/finally blocks. 
                            //               One CANNOT reliably delete the tmp file until AFTER all associated file handles 
                            //               are closed. This only happens upon doc close (or doc dispose). Therefore, the ONLY 
                            //               reliable composition is as shown (here).
                            //
                            
                            doc = new Document(finfoSrc.FullName, xpsParms);
                            if (doc == null)
                            {   // Something (really) bad happened and no one managed to throw an exception. Let's do so now...
                                throw new NullReferenceException(@"Error: Cannot construct Document");
                            }
                            
                            string tmpPath = String.Format("{0}{1}", IOPath.GetTempPath(), IOPath.GetRandomFileName());
                            tmpPath = IOPath.ChangeExtension(tmpPath, @"pdf");  // Don't allow arbitrary extension
                            int i = 0;
                            do
                            {
                                finfoTmp = new FileInfo(tmpPath);
                                if (finfoTmp.Exists)
                                {   // Bump the path to <filename>.i.pdf (which will trigger a tmp file info re-build/check until no collision exists)
                                    ++i;
                                    string pathWithoutExtension = String.Format("{0}{2}{1}", finfoTmp.DirectoryName, IOPath.GetFileNameWithoutExtension(finfoTmp.FullName), IOPath.DirectorySeparatorChar);
                                    tmpPath = IOPath.ChangeExtension(pathWithoutExtension, String.Format("{0}.pdf", i));
                                }
                            }
                            while (finfoTmp.Exists);
                            
                            doc.Save(SaveFlags.Full, finfoTmp.FullName);
                        }
                        finally
                        {
                            if (doc != null)
                            {   // Close MUST occur so that forces our hand a bit. Let's be deterministic...
                                doc.Close();
                                doc.Dispose();
                            }
                        }
                    }
                    catch
                    {   
                        try
                        {   // Attempt clean up temp file, if any...
                            if (finfoTmp != null)
                            {
                                finfoTmp.Refresh();
                                if (finfoTmp.Exists)
                                {
                                    finfoTmp.Delete();
                                }
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                        
                        //
                        // RECOMMENDATION: The (following) re-throw should be caught (at web app boundary) 
                        //                 and trigger a) a re-direct or, if there is a security concern b) silence...
                        //
                        
                        throw;
                    }
                    
                    //
                    // IMPORTANT: doc (and thus pdf file at finfoTmp.FullName) MUST, MUST, MUST (!!!) be closed before proceeding
                    //
                    
                    if (finfoTmp != null)
                    {
                        if (finfoDst != null)
                        {
                            finfoDst.Refresh();
                            string dstPath = finfoDst.FullName;
                            dstPath = IOPath.ChangeExtension(dstPath, @"pdf");  // Don't allow arbitrary extension
                            int i = 0;
                            do
                            {
                                finfoDst = new FileInfo(dstPath);
                                if (finfoDst.Exists)
                                {   // Bump the path to <filename>.i.pdf (which will trigger a dst file info re-build/check until no collision exists)
                                    ++i;
                                    string pathWithoutExtension = String.Format("{0}{2}{1}", finfoDst.DirectoryName, IOPath.GetFileNameWithoutExtension(finfoDst.FullName), IOPath.DirectorySeparatorChar);
                                    dstPath = IOPath.ChangeExtension(pathWithoutExtension, String.Format("{0}.pdf", i));
                                }
                            }
                            while (finfoDst.Exists);
                            
                            finfoTmp.Refresh();
                            if (finfoTmp.Exists)
                            {
                                DirectoryInfo dinfoTmpRoot = finfoTmp.Directory.Root;
                                DirectoryInfo dinfoDstRoot = finfoDst.Directory.Root;
                                if (String.Compare(dinfoDstRoot.FullName, dinfoTmpRoot.FullName, true) == 0)   // NOTE: One should build a more robust comparison (than this, trivial, sample code)
                                {   // *If tmp and dst lie on the same volume* just do a move (i.e., file system update)...
                                    FileInfo finfoTmpDst = new FileInfo(finfoTmp.FullName); // Clone (because the move will re-point finfoTmpDst to finfoDst)
                                    finfoTmpDst.MoveTo(finfoDst.FullName);
                                }
                                else
                                {   // *Otherwise* copy (which we will do via some stream semantics; for educational purposes)
                                    
                                    //
                                    // A sampler of streaming techniques...
                                    //
                                    
                                    Stream tmpStream = null;
                                    try
                                    {
                                        tmpStream = new FileStream(finfoTmp.FullName, FileMode.Open, FileAccess.Read);
                                        tmpStream.Seek(0, SeekOrigin.Begin);
                                        
                                        if (finfoTmp.Length > 0xFFFFF /* 1M */)
                                        {   // Large transfer handling (buffered)
                                            int tmpBufSize = 0x1FFFE; // 128K (up to 8M)
                                            while (tmpBufSize < 0xFFFFFF /* 16M */ && finfoTmp.Length > tmpBufSize)
                                            {   // Scale tmp stream buffer up to 16M
                                                tmpBufSize *= 2;
                                                
                                                if (tmpBufSize > finfoTmp.Length)
                                                {
                                                    tmpBufSize = (int)finfoTmp.Length;
                                                }
                                            }
                                            
                                            using (BufferedStream tmpBufStream = new BufferedStream(tmpStream, tmpBufSize))
                                            {
                                                Stream dstStream = null;
                                                try
                                                {
                                                    //
                                                    // MAINTAINER'S ALERT: Given host network constraints (circa 2012)...
                                                    //                     there's not much point in making bytesCapacity > 128K
                                                    //
                                                    
                                                    int bytesCapacity = 0x1FFFE; /* 128K */
                                                    byte[] bytes = new byte[bytesCapacity];
                                                    
                                                    dstStream = new FileStream(finfoDst.FullName, FileMode.OpenOrCreate, FileAccess.Write);
                                                    dstStream.Seek(0, SeekOrigin.Begin);
                                                    
                                                    int positionR = 0;  // For debugging
                                                    int positionW = 0;  // For debugging
                                                    
                                                    int bytesRead = 0;
                                                    while ((bytesRead = tmpBufStream.Read(bytes, 0, bytesCapacity)) > 0 && bytesRead <= bytesCapacity)
                                                    {
                                                        positionR += bytesRead;
                                                        dstStream.Write(bytes, 0, bytesRead);
                                                        positionW += bytesRead;
                                                    }
                                                }
                                                finally
                                                {
                                                    if (dstStream != null)
                                                    {   // Close MUST occur so that forces our hand a bit. Let's be deterministic...
                                                        dstStream.Close();
                                                        dstStream.Dispose();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {   // Small transfer handling (unbuffered) ... should (obviously) be <= 1M
                                            Stream dstStream = null;
                                            try
                                            {
                                                //
                                                // MAINTAINER'S ALERT: Given host network constraints (circa 2012)...
                                                //                     there's not much point in making bytesCapacity > 128K
                                                //
                                                
                                                int bytesCapacity = (int)((finfoTmp.Length > 0x1FFFE /* 128K */) ? 0x1FFFE /* 128K */ : finfoTmp.Length);  // Cast is fine since type clipping is n/a
                                                byte[] bytes = new byte[bytesCapacity];
                                                
                                                dstStream = new FileStream(finfoDst.FullName, FileMode.OpenOrCreate, FileAccess.Write);
                                                dstStream.Seek(0, SeekOrigin.Begin);

                                                int positionR = 0;  // For debugging
                                                int positionW = 0;  // For debugging
                                                
                                                int bytesRead = 0;
                                                while ((bytesRead = tmpStream.Read(bytes, 0, bytesCapacity)) > 0 && bytesRead <= bytesCapacity)
                                                {
                                                    positionR += bytesRead;
                                                    dstStream.Write(bytes, 0, bytesRead);
                                                    positionW += bytesRead;
                                                }
                                            }
                                            finally
                                            {
                                                if (dstStream != null)
                                                {   // Close MUST occur so that forces our hand a bit. Let's be deterministic...
                                                    dstStream.Close();
                                                    dstStream.Dispose();
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (tmpStream != null)
                                        {   // Close MUST occur so that forces our hand a bit. Let's be deterministic...
                                            tmpStream.Close();
                                            tmpStream.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                        
                        finfoTmp.Refresh();
                        if (finfoTmp.Exists)
                        {   // If moved, the tmp file won't exist...
                            // If copied/streamed, the tmp file will still exist (and must be deleted)
                            finfoTmp.Delete();
                        }
                    }
                }
            }
            finally
            {
                if (lib != null)
                {   // Terminate MUST occur so that forces our hand a bit. Let's be deterministic...
                    lib.Terminate();
                    lib.Dispose();
                }
            }
        }
    }
}
