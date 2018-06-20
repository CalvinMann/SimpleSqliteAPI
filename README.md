# SimpleSqliteAPI
.NET ORM for SQLite 


SimpleSqlite API
Integration Guide

Integration Process
Integration into you Visual Studio project is fairly simple. Just as you would reference any .Net dll - you’ll reference the SimpleSqlite dll. The only catch is you’ll have to be sure to make the SQLite.Interop.dll and the System.Data.SQLite.dll available in the run folder through a build event. We’ll cover that below in our 3 step integration process. Let’s get started! 

NOTE: Before unzipping the folder, you’ll want to make sure your browser did not put a block on the zip container. Double check by right clicking the zip container, click the properties menu option, select the tab labeled General, and if a button appears at the bottom that says “Unblock” then select that button. If that button doesn’t appear you’re okay to unzip the container


Step 1: Unzip the downloaded SimpleSqlite folder into a location in your solution project. 
Action 1 - We recommend creating a folder at the solution file level for your third party libraries if you haven’t already. We called ours Libraries and unzipped the SimpleSqliteAPI right into this folder. You’ll see an x86 and an x64 folder inside of the SimpleSqliteAPI folder.
 


Step2: Add a reference to the SimpleSqlite.dll in your visual studio project you plan to use SimpleSqlite. 
 Action 1 - Right click on the ‘Reference’ menu item
 


Action 2 - Navigate to the SimpleSqliteAPI folder in Libraries  (or the folder you unzipped the dlls in) and reference the x86 or x64 SimpleSqlite.dll. You should match the target platform that you’re building in. If you don’t know then just use x86.
 









Action 3 -Verify the dll has been added in your projects reference list
 

Step3: Add our provided build event script inside the startup projects build events located in the properties of the Visual Studio project. 
NOTE: This script below in Action 4 is necessary so it knows which Sqlite.Interop dlls to copy to your run folder. Copy the text below and follow the steps in the images. 
Action 1 -Click on ‘Properties’ on the Startup project
 



Action 2 -Navigate to the left side of the window and look for the Build Events option and click on the Build Events tab.
 
Action 3 -Find the section marked POST BUILD event command line. Here is where you’ll add the build event script. 
 
Action 4 -Copy the text below and put the text in the post build event text box. 
NOTE:  The highlighted text reflects the name of the folder where we stored our SimpleSqliteDlls. Your folder name may be different and you’ll need to make sure the path points to the correct location of where your SimpleSqlite dlls are stored.
NOTE: Do not change the spacing of the post build event below. Changing it could cause the compiler to not recognize the build script
if "x86" == "$(PlatformName)" (
    xcopy /Y /C "$(SolutionDir)Libraries\SimpleSqliteAPI\x86\*.*" "$(TargetDir)"
) else if "x64" == "$(PlatformName)" (
    xcopy /Y /C "$(SolutionDir)Libraries\SimpleSqliteAPI\x64\*.*" "$(TargetDir)"
) else if "AnyCPU" == "$(PlatformName)" (
    xcopy /Y /C "$(SolutionDir)Libraries\SimpleSqliteAPI\x86\*.*" "$(TargetDir)"
)

Your post build event script should look like this.
 


Congratulations! 
You have successfully integrated SimpleSqlite into your solution. The important thing to remember here is that the Sqlite.Interop.dll and the System.Data.SQLite.dll always need to be in your run folder. 
