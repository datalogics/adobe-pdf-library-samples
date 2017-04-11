How to import Java Samples project into Eclipse:

1. Create Eclipse Workspace.
2. Create a user library.
	a) Open the preferences.
	b) Search for the User Libraries pane.
	c) Click New...
	d) Make a library named Java_Interface.
	e) Click Add External JARs...
	f) For com.Datalogics.PDFL.jar click edit and choose the location of the current library, which should be in the Java/Binaries folder.
	g) Expand the properties and click "Native library location."
	h) Specify the same location folder as for com.Datalogics.PDFL.jar file.
	i) Click OK.
	j) Click the Javadoc location.
	k) Choose the Java documentation directory in the main Documentation directory.
	l) Click Validate... to make sure.
	m) Click OK to dismiss the validation.
	n) Click OK to dismiss the Javadoc choice window.
	o) Click OK to dismiss the Preferences window.
3. Import Samples project:
	a) Choose File -> Import to open the Import dialog.
	b) In the Import dialog select the "General" folder, and then select the option "Existing Projects into Workspace" and click next.
	c) In the "Select root directory" browse to the current directory (e.g. Java/Sample_Source).
	d) A Samples project should appear in the text area. Click Finish.

4. Setting up Run Configuration:
	a) Open project Properties and choose "Run/Debug Settings" option.
	b) Click New..., and switch to the "Java Application" option.
	c) In the dialog that appears, click the "Main class" Search button. These classes define all the samples available. Choose whatever you want to launch.
	d) In the "Arguments" tab select the radio button for "Other" under "Working directory" and find the folder with the com.Datalogics.PDFL.jar library (commonly Java/Binaries folder).
	e) As an alternative to defining absolute path in d) above, you may specify the ${workspace_loc:Sample_Source}/../Binaries/ macro, provided that you keep original folder structure.

5. As an alternative to 4, you may open Run Configuration shipped with the Samples:
	a) Choose the "Import" option for the project.
	b) In the dialog that appears, select "Run/Debug" folder and choose "Launch Configurations."
	c) Specify the directory with Samples project (e.g. current directory).
	d) Click the folder that appears below to select it (don't check it). The "RunConfiguration.launch" file appears in the right pane.
	e) Check the file and click Finish. Keep in mind that this file will be deleted from the disk.
	f) To change launched application execute steps 4a-c above, but instead of 4b just select RunConfiguration from the list and click Edit.
