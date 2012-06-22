load('jsmake.dotnet.DotNetUtils.js');

var fs = jsmake.Fs;
var utils = jsmake.Utils;
var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

var version, assemblyVersion;

task('default', 'build');

task('version', function () {
	version = JSON.parse(fs.readFile('version.json'));
	assemblyVersion = [ version.major, version.minor, version.build, 0 ].join('.');
});

task('dependencies', function () {
	var pkgs = fs.createScanner('src').include('**/packages.config').scan();
	dotnet.downloadNuGetPackages(pkgs, 'lib');
});

task('assemblyinfo', 'version', function () {
	dotnet.writeAssemblyInfo('src/SharedAssemblyInfo.cs', {
		AssemblyTitle: 'Log4Rabbit',
		AssemblyProduct: 'Log4Rabbit',
		AssemblyDescription: 'RabbitMQ appender for log4net',
		AssemblyCopyright: 'Copyright © Gian Marco Gherardi ' + new Date().getFullYear(),
		AssemblyTrademark: '',
		AssemblyCompany: 'Gian Marco Gherardi',
		AssemblyConfiguration: '', // Probably a good place to put Git SHA1 and build date
		AssemblyVersion: assemblyVersion,
		AssemblyFileVersion: assemblyVersion,
		AssemblyInformationalVersion: assemblyVersion
	});
});

task('build', [ 'dependencies', 'assemblyinfo' ], function () {
	dotnet.runMSBuild('src/Log4Rabbit.sln', [ 'Clean', 'Rebuild' ]);
});

task('test', 'build', function () {
	var testDlls = fs.createScanner('src').include('*.Tests/bin/Debug/*.Tests.dll').scan();
	dotnet.runNUnit(testDlls);
});

task('release', 'test', function () {
	fs.deletePath('build');
	fs.createDirectory('build');
	
	sys.run('tools/nuget/nuget.exe', 'pack', 'src\\Log4Rabbit\\Log4Rabbit.csproj', '-Build', '-OutputDirectory', 'build', '-Symbols');
	sys.run('tools/nuget/nuget.exe', 'push', 'build\\Log4Rabbit.' + assemblyVersion + '.nupkg');
	
	version.build += 1;
	fs.writeFile('version.json', JSON.stringify(version));
});

