properties {
    $nunitConsolePath = '.\packages\NUnit.Console.3.0.1\tools\nunit3-console.exe'
}


task default -depends Clean, Build, Test 

task Clean {
	exec { msbuild dotnetnes.sln '/t:Clean' /nologo /verbosity:Minimal }
}

task Build {
	exec { msbuild dotnetnes.sln '/t:Build' /nologo /verbosity:Minimal  }
}

task Test {
    $env:TestDataDirectory = ".\dotnetnes.tests\TestRoms"
    & $nunitConsolePath 'dotnetnes.nunit' "--result:TestResult.xml;format=nunit2"
}