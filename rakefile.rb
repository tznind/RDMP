require 'albacore'

load 'rakeconfig.rb'

task :ci_continuous, [:config] => [:setup_connection, :assemblyinfo, :deploy]

task :pluginbuild, [:folder] => [:assemblyinfo, :deployplugins]

task :restorepackages do
    sh "nuget restore HIC.DataManagementPlatform.sln"
end

task :setup_connection do 
    File.open("Tests.Common/TestDatabases.txt", "w") do |f|
        f.write "ServerName:#{DBSERVER}\r\n"
        f.write "Prefix:#{DBPREFIX}\r\n"
    end
end

msbuild :build, [:config] => :restorepackages do |msb, args|
	args.with_defaults(:config => :Debug)
    msb.properties = { :configuration => args.config }
    msb.targets = [ :Clean, :Build ]   
    msb.solution = SOLUTION
end

msbuild :deploy, [:config] => :restorepackages do |msb, args|
	args.with_defaults(:config => :Release)
    msb.targets [ :Clean, :Build ]
    msb.properties = {
        :configuration => args.config,
        :outdir => "#{PUBLISH_DIR}/"
    }
    msb.solution = SOLUTION
end

desc "Sets the version number from GIT"    
assemblyinfo :assemblyinfo do |asm|
	asm.input_file = "SharedAssemblyInfo.cs"
    asm.output_file = "SharedAssemblyInfo.cs"
    version = File.read("SharedAssemblyInfo.cs")[/\d+\.\d+\.\d+(\.\d+)?/]
    describe = `git describe`.strip
    tag, rev, hash = describe.split(/-/)
    major, minor, patch, build = version.split(/\./)
    if PRERELEASE == "true"
        puts "version: #{major}.#{minor}.#{patch}.#{rev} build:#{build} suffix:#{SUFFIX}"
        asm.version = "#{major}.#{minor}.#{patch}.#{rev}"
        asm.file_version = "#{major}.#{minor}.#{patch}.#{rev}"
        asm.informational_version = "#{major}.#{minor}.#{patch}.#{rev}-#{SUFFIX}"
    else if CANDIDATE == "true"
        puts "version: #{major}.#{minor}.#{patch}.0 build:#{build} suffix:#{SUFFIX}"
        asm.version = "#{major}.#{minor}.#{patch}.0"
        asm.file_version = "#{major}.#{minor}.#{patch}.0"
        asm.informational_version = "#{major}.#{minor}.#{patch}.0-#{SUFFIX}"
    else
        puts "version: #{major}.#{minor}.#{patch}.0 build:#{build}"
        asm.version = "#{major}.#{minor}.#{patch}.0"
        asm.file_version = "#{major}.#{minor}.#{patch}.0"
        asm.informational_version = "#{major}.#{minor}.#{patch}.0"
    end
end

task :deployplugins, [:folder] do |t, args|
    Dir.chdir('Plugin/Plugin') do
        sh "./build-and-deploy-local.bat #{args.folder}"
    end
end

# task :link do
# 	if File.exist?("DatabaseCreation.exe") 
# 		File.delete("DatabaseCreation.exe")
# 	end
# 	sh "call mklink DatabaseCreation.exe DatabaseCreation\\Release\\DatabaseCreation.exe"
# end
